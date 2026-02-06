using System.Data;
using Core.Sql.Models;

namespace Core.Sql.Services;

/// <summary>
/// Interface for SQL execution service providing database operations.
/// Supports stored procedures, raw queries, transactions, and background execution.
/// </summary>
public interface ISqlExecuteService
{
    #region Stored Procedure Execution with Paging

    /// <summary>
    /// Executes a stored procedure and returns paginated results.
    /// The stored procedure should have output parameter for total record count.
    /// </summary>
    /// <typeparam name="T">The type to map results to.</typeparam>
    /// <param name="model">The SQL execution model containing procedure name and parameters.</param>
    /// <param name="pageIndex">The page index (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A paged result set with items and metadata.</returns>
    Task<BaseResponse<TPaging<T>>> ExecuteProcReturnPagingAsync<T>(
        SqlExecuteModel model,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default) where T : class, new();

    #endregion

    #region Stored Procedure Execution with Return Data

    /// <summary>
    /// Executes a stored procedure and returns a list of mapped objects.
    /// Uses ExecuteReaderAsync for optimal performance.
    /// </summary>
    /// <typeparam name="T">The type to map results to.</typeparam>
    /// <param name="model">The SQL execution model containing procedure name and parameters.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of mapped objects.</returns>
    Task<BaseResponse<List<T>>> ExecuteProceReturnAsync<T>(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Executes a stored procedure and returns a single mapped object.
    /// </summary>
    /// <typeparam name="T">The type to map the result to.</typeparam>
    /// <param name="model">The SQL execution model.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A single mapped object or null.</returns>
    Task<BaseResponse<T?>> ExecuteProcReturnSingleAsync<T>(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Executes a stored procedure and returns a DataTable.
    /// </summary>
    /// <param name="model">The SQL execution model.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A DataTable containing the results.</returns>
    Task<BaseResponse<DataTable>> ExecuteProcReturnDataTableAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a stored procedure and returns multiple result sets as a DataSet.
    /// </summary>
    /// <param name="model">The SQL execution model.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A DataSet containing multiple result tables.</returns>
    Task<BaseResponse<DataSet>> ExecuteProcReturnDataSetAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default);

    #endregion

    #region Non-Query Execution (CUD Operations)

    /// <summary>
    /// Executes a stored procedure for Create/Update/Delete operations.
    /// Captures output parameters (ResponseCode, ResponseMessage) for result.
    /// </summary>
    /// <param name="model">The SQL execution model.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A response indicating success/failure with any output data.</returns>
    Task<BaseResponse<object>> ProcExecuteNonQueryAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a stored procedure for CUD operations and returns affected row count.
    /// </summary>
    /// <param name="model">The SQL execution model.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A response containing the number of affected rows.</returns>
    Task<BaseResponse<int>> ProcExecuteNonQueryWithRowCountAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default);

    #endregion

    #region Batch Execution (Transactional)

    /// <summary>
    /// Executes a stored procedure multiple times within a single transaction.
    /// Rolls back all changes if any execution fails.
    /// </summary>
    /// <param name="model">The batch execution model containing procedure and parameter sets.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A response indicating overall success/failure.</returns>
    Task<BaseResponse<BatchExecutionResult>> ProcExecuteBatchAsync(
        SqlExecuteBatchModel model,
        CancellationToken cancellationToken = default);

    #endregion

    #region Function Execution

    /// <summary>
    /// Executes a MySQL scalar function and returns the result.
    /// Example: SELECT MyFunc(param1, param2)
    /// </summary>
    /// <typeparam name="T">The expected return type of the function.</typeparam>
    /// <param name="model">The SQL execution model (SqlProcOrQuery should be the function call).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The function result.</returns>
    Task<BaseResponse<T?>> ExecuteFunctionReturnAsync<T>(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default);

    #endregion

    #region Raw SQL Execution

    /// <summary>
    /// Executes a raw SQL command and returns a DataTable.
    /// WARNING: Use with caution - ensure proper parameterization to prevent SQL injection.
    /// </summary>
    /// <param name="model">The SQL execution model with raw query.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A DataTable containing the results.</returns>
    Task<BaseResponse<DataTable>> ExecuteSqlRawCommandAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a raw SQL command and returns mapped objects.
    /// </summary>
    /// <typeparam name="T">The type to map results to.</typeparam>
    /// <param name="model">The SQL execution model with raw query.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of mapped objects.</returns>
    Task<BaseResponse<List<T>>> ExecuteSqlRawCommandAsync<T>(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Executes a raw SQL non-query command (INSERT, UPDATE, DELETE).
    /// </summary>
    /// <param name="model">The SQL execution model with raw query.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    Task<BaseResponse<int>> ExecuteSqlRawNonQueryAsync(
        SqlExecuteModel model,
        CancellationToken cancellationToken = default);

    #endregion

    #region Background Execution

    /// <summary>
    /// Executes a stored procedure in the background (fire-and-forget).
    /// Creates its own scope and connection. Errors are logged but don't affect the main thread.
    /// </summary>
    /// <param name="model">The SQL execution model.</param>
    /// <remarks>
    /// This method returns immediately. The actual execution happens asynchronously.
    /// Any errors are logged to the configured logger but will not throw exceptions.
    /// </remarks>
    void ProcExecuteBackgroundAsync(SqlExecuteModel model);

    /// <summary>
    /// Executes a stored procedure in the background with a callback on completion.
    /// </summary>
    /// <param name="model">The SQL execution model.</param>
    /// <param name="onComplete">Optional callback when execution completes (success or failure).</param>
    void ProcExecuteBackgroundAsync(SqlExecuteModel model, Action<bool, string?>? onComplete);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Tests the database connection.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if connection is successful.</returns>
    Task<BaseResponse<bool>> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the server version information.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Server version string.</returns>
    Task<BaseResponse<string>> GetServerVersionAsync(CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Result model for batch execution operations.
/// </summary>
public class BatchExecutionResult
{
    /// <summary>
    /// Total number of batch items attempted.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of successfully executed items.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed items.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Index of the first failed item (-1 if all succeeded).
    /// </summary>
    public int FirstFailedIndex { get; set; } = -1;

    /// <summary>
    /// Error message from the first failure.
    /// </summary>
    public string? FirstErrorMessage { get; set; }

    /// <summary>
    /// Indicates if all items were executed successfully.
    /// </summary>
    public bool AllSucceeded => FailedCount == 0;
}
