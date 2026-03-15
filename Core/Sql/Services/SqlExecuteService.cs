using System.Data;
using Core.Sql.Extensions;
using Core.Sql.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Core.Sql.Services;

/// <summary>
/// Production-ready SQL execution service implementation.
/// Provides comprehensive database operations with proper error handling,
/// connection management, and logging.
/// </summary>
/// <remarks>
/// Key features:
/// - Async/await throughout for non-blocking I/O
/// - Proper connection pooling (connections are returned to pool on dispose)
/// - SQL injection prevention via parameterized queries
/// - Sanitized error messages for production safety
/// - Transaction support for batch operations
/// - Background execution capability
/// </remarks>
public class SqlExecuteService : ISqlExecuteService
{
    private readonly string _connectionString;
    private readonly ILogger<SqlExecuteService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly bool _isDevelopment;

    /// <summary>
    /// Initializes a new instance of the SqlExecuteService.
    /// </summary>
    /// <param name="configuration">Application configuration for connection string.</param>
    /// <param name="logger">Logger for error and diagnostic logging.</param>
    /// <param name="serviceProvider">Service provider for creating scoped services in background operations.</param>
    public SqlExecuteService(
        IConfiguration configuration,
        ILogger<SqlExecuteService> logger,
        IServiceProvider serviceProvider)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
        _serviceProvider = serviceProvider;
        _isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }

    #region Stored Procedure Execution with Paging

    /// <inheritdoc/>
    public async Task<BaseResponse<TPaging<T>>> ExecuteProcReturnPagingAsync<T>(
        SqlExecuteModel model,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        // Normalize paging parameters
        (pageIndex, pageSize) = PagingExtensions.NormalizePagingParams(pageIndex, pageSize);

        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(connection, model);

            // Check if paging parameters already exist in model (from BuildBaseSearchParams)
            // Only add if not already present to avoid duplicate parameters
            var normalizedTotalRecordParamName = SqlConstants.P_TotalRecord.StartsWith("@") 
                ? SqlConstants.P_TotalRecord 
                : $"@{SqlConstants.P_TotalRecord}";
            
            // Check if p_total_record output parameter already exists
            MySqlParameter? totalRecordParam = null;
            if (command.Parameters.Contains(normalizedTotalRecordParamName))
            {
                // Use existing parameter from model
                totalRecordParam = command.Parameters[normalizedTotalRecordParamName];
            }
            else
            {
                // Add output parameter for total records if not present
                totalRecordParam = command.Parameters.Add(normalizedTotalRecordParamName, MySqlDbType.Int64);
                totalRecordParam.Direction = ParameterDirection.Output;
            }

            // Execute and fill DataSet
            using var adapter = new MySqlDataAdapter(command);
            var dataSet = new DataSet();
            await Task.Run(() => adapter.Fill(dataSet), cancellationToken);

            // Get total records from output parameter
            var totalRecords = totalRecordParam.Value != DBNull.Value
                ? Convert.ToInt64(totalRecordParam.Value)
                : 0L;

            // Map first table to items
            var items = dataSet.Tables.Count > 0
                ? dataSet.Tables[0].DataTableToList<T>()
                : [];

            var paging = new TPaging<T>(items, pageIndex, pageSize, totalRecords);

            return BaseResponse<TPaging<T>>.Ok(paging);
        }
        catch (Exception ex)
        {
            return HandleException<TPaging<T>>(ex, model.SqlProcOrQuery, "ExecuteProcReturnPagingAsync");
        }
    }

    #endregion

    #region Stored Procedure Execution with Return Data

    /// <inheritdoc/>
    public async Task<BaseResponse<List<T>>> ExecuteProceReturnAsync<T>(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(connection, model);

            // Use DataTable.Load with reader for optimal performance
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var dataTable = new DataTable();
            dataTable.Load(reader);

            var result = dataTable.DataTableToList<T>();
            return BaseResponse<List<T>>.Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException<List<T>>(ex, model.SqlProcOrQuery, "ExecuteProceReturnAsync");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<T?>> ExecuteProcReturnSingleAsync<T>(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(connection, model);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var dataTable = new DataTable();
            dataTable.Load(reader);

            var result = dataTable.DataTableToObject<T>();
            return BaseResponse<T?>.Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException<T?>(ex, model.SqlProcOrQuery, "ExecuteProcReturnSingleAsync");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<DataTable>> ExecuteProcReturnDataTableAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(connection, model);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var dataTable = new DataTable();
            dataTable.Load(reader);

            return BaseResponse<DataTable>.Ok(dataTable);
        }
        catch (Exception ex)
        {
            return HandleException<DataTable>(ex, model.SqlProcOrQuery, "ExecuteProcReturnDataTableAsync");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<DataSet>> ExecuteProcReturnDataSetAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(connection, model);

            using var adapter = new MySqlDataAdapter(command);
            var dataSet = new DataSet();
            await Task.Run(() => adapter.Fill(dataSet), cancellationToken);

            return BaseResponse<DataSet>.Ok(dataSet);
        }
        catch (Exception ex)
        {
            return HandleException<DataSet>(ex, model.SqlProcOrQuery, "ExecuteProcReturnDataSetAsync");
        }
    }

    #endregion

    #region Non-Query Execution (CUD Operations)

    /// <inheritdoc/>
    public async Task<BaseResponse<object>> ProcExecuteNonQueryAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(connection, model);

            // Check if output parameters exist for response code/message
            var hasResponseCodeParam = model.Params.Any(p =>
                p.ParamName.Equals(SqlConstants.P_ResponseCode, StringComparison.OrdinalIgnoreCase));
            var hasResponseMessageParam = model.Params.Any(p =>
                p.ParamName.Equals(SqlConstants.P_ResponseMessage, StringComparison.OrdinalIgnoreCase));

            // Add standard output parameters if not already present
            MySqlParameter? responseCodeParam = null;
            MySqlParameter? responseMessageParam = null;

            if (!hasResponseCodeParam)
            {
                responseCodeParam = command.Parameters.Add(SqlConstants.P_ResponseCode, MySqlDbType.Int32);
                responseCodeParam.Direction = ParameterDirection.Output;
            }
            else
            {
                responseCodeParam = command.Parameters[SqlConstants.P_ResponseCode];
            }

            if (!hasResponseMessageParam)
            {
                responseMessageParam = command.Parameters.Add(SqlConstants.P_ResponseMessage, MySqlDbType.VarChar, 500);
                responseMessageParam.Direction = ParameterDirection.Output;
            }
            else
            {
                responseMessageParam = command.Parameters[SqlConstants.P_ResponseMessage];
            }

            await command.ExecuteNonQueryAsync(cancellationToken);

            // Read output parameters
            var responseCode = responseCodeParam?.Value != DBNull.Value
                ? Convert.ToInt32(responseCodeParam?.Value ?? SqlConstants.ResponseCode_Success)
                : SqlConstants.ResponseCode_Success;

            var responseMessage = responseMessageParam?.Value != DBNull.Value
                ? responseMessageParam?.Value?.ToString() ?? SqlConstants.Message_Success
                : SqlConstants.Message_Success;

            // Collect any output parameter values to return
            var outputData = new Dictionary<string, object?>();
            foreach (MySqlParameter param in command.Parameters)
            {
                if (param.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                {
                    outputData[param.ParameterName] = param.Value == DBNull.Value ? null : param.Value;
                }
            }

            return BaseResponse<object>.FromProcedureOutput(responseCode, responseMessage, outputData);
        }
        catch (Exception ex)
        {
            return HandleException<object>(ex, model.SqlProcOrQuery, "ProcExecuteNonQueryAsync");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<int>> ProcExecuteNonQueryWithRowCountAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(connection, model);

            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            return BaseResponse<int>.Ok(affectedRows);
        }
        catch (Exception ex)
        {
            return HandleException<int>(ex, model.SqlProcOrQuery, "ProcExecuteNonQueryWithRowCountAsync");
        }
    }

    #endregion

    #region Batch Execution (Transactional)

    /// <inheritdoc/>
    public async Task<BaseResponse<BatchExecutionResult>> ProcExecuteBatchAsync(
        SqlExecuteBatchModel model,
        CancellationToken cancellationToken = default)
    {
        var result = new BatchExecutionResult
        {
            TotalItems = model.BatchCount
        };

        if (model.BatchCount == 0)
        {
            result.SuccessCount = 0;
            return BaseResponse<BatchExecutionResult>.Ok(result);
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Start transaction for atomic batch execution
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            for (int i = 0; i < model.BatchParams.Count; i++)
            {
                var batchItem = model.BatchParams[i];

                try
                {
                    await using var command = new MySqlCommand(model.SqlProcOrQuery, connection, transaction)
                    {
                        CommandType = model.IsStoredProcedure
                            ? CommandType.StoredProcedure
                            : CommandType.Text
                    };

                    if (model.CommandTimeoutSeconds.HasValue)
                    {
                        command.CommandTimeout = model.CommandTimeoutSeconds.Value;
                    }

                    // Add parameters for this batch item
                    AddParameters(command, batchItem.Params);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                    result.SuccessCount++;
                }
                catch (Exception itemEx)
                {
                    result.FailedCount++;

                    if (result.FirstFailedIndex < 0)
                    {
                        result.FirstFailedIndex = i;
                        result.FirstErrorMessage = SanitizeErrorMessage(itemEx);
                    }

                    // Log the individual failure
                    _logger.LogWarning(itemEx,
                        "Batch item {Index} failed for procedure {Procedure}. Rolling back transaction.",
                        i, MaskProcedureName(model.SqlProcOrQuery));

                    // If not continuing on error, break and rollback
                    if (!model.ContinueOnError)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        var errorResponse = BaseResponse<BatchExecutionResult>.Error(
                            $"Batch execution failed at item {i + 1}. All changes rolled back.",
                            SqlConstants.ResponseCode_Error);
                        errorResponse.Data = result;
                        return errorResponse;
                    }
                }
            }

            // Commit if we got here (either all succeeded or ContinueOnError was true)
            if (result.FailedCount == 0 || model.ContinueOnError)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            if (result.AllSucceeded)
            {
                return BaseResponse<BatchExecutionResult>.Ok(result,
                    $"Successfully executed all {result.TotalItems} batch items.");
            }
            else
            {
                return BaseResponse<BatchExecutionResult>.Ok(result,
                    $"Completed with {result.SuccessCount} successes and {result.FailedCount} failures.");
            }
        }
        catch (Exception ex)
        {
            // Rollback on any unexpected error
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Failed to rollback transaction");
            }

            return HandleException<BatchExecutionResult>(ex, model.SqlProcOrQuery, "ProcExecuteBatchAsync");
        }
    }

    #endregion

    #region Function Execution

    /// <inheritdoc/>
    public async Task<BaseResponse<T?>> ExecuteFunctionReturnAsync<T>(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Build function call: SELECT function_name(params)
            var functionCall = BuildFunctionCall(model);

            await using var command = new MySqlCommand(functionCall, connection)
            {
                CommandType = CommandType.Text
            };

            if (model.CommandTimeoutSeconds.HasValue)
            {
                command.CommandTimeout = model.CommandTimeoutSeconds.Value;
            }

            AddParameters(command, model.Params);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var convertedResult = TypeConversionHelper.SafeConvert<T>(result);

            return BaseResponse<T?>.Ok(convertedResult);
        }
        catch (Exception ex)
        {
            return HandleException<T?>(ex, model.SqlProcOrQuery, "ExecuteFunctionReturnAsync");
        }
    }

    #endregion

    #region Raw SQL Execution

    /// <inheritdoc/>
    public async Task<BaseResponse<DataTable>> ExecuteSqlRawCommandAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new MySqlCommand(model.SqlProcOrQuery, connection)
            {
                CommandType = CommandType.Text
            };

            if (model.CommandTimeoutSeconds.HasValue)
            {
                command.CommandTimeout = model.CommandTimeoutSeconds.Value;
            }

            AddParameters(command, model.Params);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var dataTable = new DataTable();
            dataTable.Load(reader);

            return BaseResponse<DataTable>.Ok(dataTable);
        }
        catch (Exception ex)
        {
            return HandleException<DataTable>(ex, model.SqlProcOrQuery, "ExecuteSqlRawCommandAsync");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<List<T>>> ExecuteSqlRawCommandAsync<T>(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        var tableResult = await ExecuteSqlRawCommandAsync(model, cancellationToken);

        if (!tableResult.Success)
        {
            return BaseResponse<List<T>>.Error(tableResult.Message, tableResult.ErrorCode);
        }

        var list = tableResult.Data?.DataTableToList<T>() ?? [];
        return BaseResponse<List<T>>.Ok(list);
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<int>> ExecuteSqlRawNonQueryAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new MySqlCommand(model.SqlProcOrQuery, connection)
            {
                CommandType = CommandType.Text
            };

            if (model.CommandTimeoutSeconds.HasValue)
            {
                command.CommandTimeout = model.CommandTimeoutSeconds.Value;
            }

            AddParameters(command, model.Params);

            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            return BaseResponse<int>.Ok(affectedRows);
        }
        catch (Exception ex)
        {
            return HandleException<int>(ex, model.SqlProcOrQuery, "ExecuteSqlRawNonQueryAsync");
        }
    }

    #endregion

    #region Background Execution

    /// <inheritdoc/>
    public void ProcExecuteBackgroundAsync(SqlExecuteModel model)
    {
        ProcExecuteBackgroundAsync(model, null);
    }

    /// <inheritdoc/>
    public void ProcExecuteBackgroundAsync(SqlExecuteModel model, Action<bool, string?>? onComplete)
    {
        // Fire-and-forget with proper error handling
        // Note: This creates its own connection/scope to avoid disposed object issues
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogDebug("Starting background execution of {Procedure}",
                    MaskProcedureName(model.SqlProcOrQuery));

                // Create a new connection for background execution
                // This is intentional to avoid scope disposal issues
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(model.SqlProcOrQuery, connection)
                {
                    CommandType = model.IsStoredProcedure
                        ? CommandType.StoredProcedure
                        : CommandType.Text
                };

                if (model.CommandTimeoutSeconds.HasValue)
                {
                    command.CommandTimeout = model.CommandTimeoutSeconds.Value;
                }

                AddParameters(command, model.Params);

                await command.ExecuteNonQueryAsync();

                _logger.LogDebug("Background execution completed successfully for {Procedure}",
                    MaskProcedureName(model.SqlProcOrQuery));

                onComplete?.Invoke(true, null);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - this is fire-and-forget
                _logger.LogError(ex,
                    "Background execution failed for procedure {Procedure}",
                    MaskProcedureName(model.SqlProcOrQuery));

                onComplete?.Invoke(false, SanitizeErrorMessage(ex));
            }
        });
    }

    #endregion

    #region Utility Methods

    /// <inheritdoc/>
    public async Task<BaseResponse<bool>> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return BaseResponse<bool>.Ok(true, "Connection successful.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return BaseResponse<bool>.Error(SqlConstants.Message_ConnectionError);
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<string>> GetServerVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return BaseResponse<string>.Ok(connection.ServerVersion);
        }
        catch (Exception ex)
        {
            return HandleException<string>(ex, "GetServerVersion", "GetServerVersionAsync");
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Creates and configures a MySqlCommand from the execution model.
    /// </summary>
    private MySqlCommand CreateCommand(MySqlConnection connection, SqlExecuteModel model)
    {
        var command = new MySqlCommand(model.SqlProcOrQuery, connection)
        {
            CommandType = model.IsStoredProcedure
                ? CommandType.StoredProcedure
                : CommandType.Text,
            CommandTimeout = model.CommandTimeoutSeconds ?? SqlConstants.DefaultCommandTimeoutSeconds
        };

        AddParameters(command, model.Params);

        return command;
    }

    /// <summary>
    /// Adds parameters to a MySqlCommand from a list of SqlParamModel.
    /// </summary>
    private static void AddParameters(MySqlCommand command, List<SqlParamModel> parameters)
    {
        foreach (var param in parameters)
        {
            var mySqlParam = new MySqlParameter
            {
                ParameterName = param.ParamName.StartsWith("@") ? param.ParamName : $"@{param.ParamName}",
                Value = param.ParamValue ?? DBNull.Value,
                Direction = param.Direction
            };

            if (param.DbType.HasValue)
            {
                mySqlParam.MySqlDbType = param.DbType.Value;
            }

            if (param.Size.HasValue)
            {
                mySqlParam.Size = param.Size.Value;
            }

            command.Parameters.Add(mySqlParam);
        }
    }

    /// <summary>
    /// Builds a function call SQL statement from the model.
    /// </summary>
    private static string BuildFunctionCall(SqlExecuteModel model)
    {
        // If it already looks like a SELECT, use as-is
        if (model.SqlProcOrQuery.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return model.SqlProcOrQuery;
        }

        // Build: SELECT function_name(@param1, @param2, ...)
        var paramList = string.Join(", ",
            model.Params
                .Where(p => p.Direction == ParameterDirection.Input)
                .Select(p => p.ParamName.StartsWith("@") ? p.ParamName : $"@{p.ParamName}"));

        return $"SELECT {model.SqlProcOrQuery}({paramList})";
    }

    /// <summary>
    /// Handles exceptions uniformly, logging and returning sanitized responses.
    /// </summary>
    private BaseResponse<T> HandleException<T>(Exception ex, string procedureOrQuery, string methodName)
    {
        // Log the full exception with details
        _logger.LogError(ex,
            "Database error in {Method} executing {Procedure}",
            methodName, MaskProcedureName(procedureOrQuery));

        // Determine the appropriate user-facing message
        var (message, errorCode) = ex switch
        {
            MySqlException mySqlEx => ClassifyMySqlException(mySqlEx),
            OperationCanceledException => (SqlConstants.Message_TimeoutError, SqlConstants.ResponseCode_Error),
            InvalidOperationException => (SqlConstants.Message_ConnectionError, SqlConstants.ResponseCode_Error),
            _ => (SqlConstants.Message_Error, SqlConstants.ResponseCode_Error)
        };

        var response = BaseResponse<T>.Error(message, errorCode);

        // Only include debug info in development
        if (_isDevelopment)
        {
            response.DebugInfo = ex.Message;
        }

        return response;
    }

    /// <summary>
    /// Classifies MySQL exceptions to provide appropriate user messages.
    /// </summary>
    private static (string Message, int ErrorCode) ClassifyMySqlException(MySqlException ex)
    {
        return ex.Number switch
        {
            // Connection errors
            0 or 1042 or 1045 or 2002 or 2003 =>
                (SqlConstants.Message_ConnectionError, SqlConstants.ResponseCode_Error),

            // Duplicate entry
            1062 =>
                ("A record with the same key already exists.", SqlConstants.ResponseCode_Duplicate),

            // Foreign key constraint
            1451 or 1452 =>
                ("This operation violates data integrity constraints.", SqlConstants.ResponseCode_ValidationError),

            // Deadlock
            1213 =>
                ("A database conflict occurred. Please retry the operation.", SqlConstants.ResponseCode_Error),

            // Lock wait timeout
            1205 =>
                (SqlConstants.Message_TimeoutError, SqlConstants.ResponseCode_Error),

            // Data too long
            1406 =>
                ("The provided data exceeds the maximum allowed length.", SqlConstants.ResponseCode_ValidationError),

            // Default
            _ => (SqlConstants.Message_Error, SqlConstants.ResponseCode_Error)
        };
    }

    /// <summary>
    /// Sanitizes error messages for user display.
    /// </summary>
    private string SanitizeErrorMessage(Exception ex)
    {
        if (_isDevelopment)
        {
            return ex.Message;
        }

        return ex switch
        {
            MySqlException mySqlEx => ClassifyMySqlException(mySqlEx).Message,
            _ => SqlConstants.Message_Error
        };
    }

    /// <summary>
    /// Masks procedure/query names in logs for security.
    /// </summary>
    private static string MaskProcedureName(string procedureOrQuery)
    {
        // For stored procedures, show the name
        // For raw SQL, truncate to prevent logging sensitive queries
        if (procedureOrQuery.Contains(' '))
        {
            return procedureOrQuery.Length > 50
                ? procedureOrQuery[..50] + "..."
                : procedureOrQuery;
        }
        return procedureOrQuery;
    }

    #endregion
}
