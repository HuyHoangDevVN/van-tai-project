using Application.Models;
using Core.Sql.Config;
using Core.Sql.Helpers;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Application.Services;

/// <summary>
/// Base service class providing common functionality for all business services.
/// Implements the Dynamic SP Resolution pattern.
/// 
/// Architecture Flow:
/// Controller -> Service -> GetProcName() -> IProcedureConfigProvider (cached) -> Execute SQL
/// </summary>
public abstract class BaseService
{
    protected readonly ISqlExecuteService SqlService;
    protected readonly IProcedureConfigProvider ProcProvider;
    protected readonly ILogger Logger;

    // =========================================================================
    // STANDARD SP PARAMETER NAMES (CONVENTION)
    // ==========================================================================
    protected static class SpParams
    {
        // Paging & Search
        public const string Keyword = "p_keyword";
        public const string Offset = "p_offset";
        public const string Limit = "p_limit";
        public const string SortBy = "p_sort_by";
        public const string SortDesc = "p_sort_desc";
        public const string DateFrom = "p_date_from";
        public const string DateTo = "p_date_to";
        public const string TotalRecord = "p_total_record";

        // Response
        public const string ResponseCode = "p_response_code";
        public const string ResponseMessage = "p_response_message";
    }

    protected BaseService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger logger)
    {
        SqlService = sqlService ?? throw new ArgumentNullException(nameof(sqlService));
        ProcProvider = procProvider ?? throw new ArgumentNullException(nameof(procProvider));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ==========================================================================
    // DYNAMIC SP RESOLUTION
    // ==========================================================================

    /// <summary>
    /// Gets the stored procedure name for a function key.
    /// Throws a friendly exception if the key is not configured.
    /// </summary>
    /// <param name="functionKey">The function key (use FunctionKeys constants)</param>
    /// <returns>The actual SP name from database configuration</returns>
    protected async Task<string> GetProcNameAsync(string functionKey)
    {
        try
        {
            var spName = await ProcProvider.GetSpNameAsync(functionKey);
            Logger.LogDebug("Resolved {Key} to SP: {SpName}", functionKey, spName);
            return spName;
        }
        catch (KeyNotFoundException ex)
        {
            Logger.LogError(ex, "Function key not configured: {Key}", functionKey);
            throw new InvalidOperationException(
                $"Cấu hình hệ thống thiếu. Chức năng '{functionKey}' chưa được định nghĩa trong Sys_ProcedureConfig.", ex);
        }
    }

    /// <summary>
    /// Gets the SP name, returning null if not configured (no exception).
    /// Use this when the SP is optional.
    /// </summary>
    protected async Task<string?> GetProcNameOrDefaultAsync(string functionKey)
    {
        return await ProcProvider.GetSpNameOrDefaultAsync(functionKey);
    }

    /// <summary>
    /// Creates a SqlExecuteModel with the dynamically resolved SP name.
    /// </summary>
    protected async Task<SqlExecuteModel> CreateSpModelAsync(string functionKey)
    {
        var spName = await GetProcNameAsync(functionKey);
        return new SqlExecuteModel(spName) { IsStoredProcedure = true };
    }

    // ==========================================================================
    // UNIVERSAL SEARCH PARAMETER BUILDER
    // ==========================================================================

    /// <summary>
    /// Builds the standard search/paging parameters for SP execution.
    /// 
    /// SP Parameter Convention:
    /// - p_keyword: Search keyword (nullable)
    /// - p_offset: OFFSET value = (pageIndex - 1) * pageSize
    /// - p_limit: LIMIT value = pageSize
    /// - p_sort_by: Column name to sort (nullable)
    /// - p_sort_desc: 1 = DESC, 0 = ASC
    /// - p_date_from: Start date filter (nullable)
    /// - p_date_to: End date filter (nullable)
    /// - p_total_record: OUTPUT parameter for total count
    /// 
    /// Usage:
    /// var baseParams = BuildBaseSearchParams(request);
    /// baseParams.Add(SqlParamModel.Input("p_status", request.Status)); // Add entity-specific filters
    /// </summary>
    /// <param name="request">The search request containing paging/sorting/filtering parameters</param>
    /// <returns>List of SqlParamModel ready for SP execution</returns>
    protected List<SqlParamModel> BuildBaseSearchParams(BaseSearchReq request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parameters = new List<SqlParamModel>
        {
            // Search keyword (nullable - SP handles NULL check)
            SqlParamModel.Input(SpParams.Keyword, NullIfEmpty(request.NormalizedKeyword)),

            // Paging: Calculate OFFSET from PageIndex
            // Formula: OFFSET = (PageIndex - 1) * PageSize
            SqlParamModel.Input(SpParams.Offset, request.Offset, MySqlDbType.Int32),
            SqlParamModel.Input(SpParams.Limit, request.NormalizedPageSize, MySqlDbType.Int32),

            // Sorting (nullable - SP uses default if NULL)
            SqlParamModel.Input(SpParams.SortBy, NullIfEmpty(request.NormalizedSortBy)),
            SqlParamModel.Input(SpParams.SortDesc, request.SortDesc ? 1 : 0, MySqlDbType.Int32),

            // Date range filtering (nullable)
            SqlParamModel.Input(SpParams.DateFrom, request.FromDate),
            SqlParamModel.Input(SpParams.DateTo, request.ToDate),

            // Output parameter for total record count (required for paging metadata)
            SqlParamModel.Output(SpParams.TotalRecord, MySqlDbType.Int64)
        };

        return parameters;
    }

    /// <summary>
    /// Creates a complete SqlExecuteModel for search/paging operations.
    /// Combines base parameters with entity-specific filters.
    /// </summary>
    /// <param name="functionKey">The function key to resolve SP name</param>
    /// <param name="request">The search request</param>
    /// <param name="additionalParams">Entity-specific filter parameters</param>
    /// <returns>Configured SqlExecuteModel ready for execution</returns>
    protected async Task<SqlExecuteModel> BuildSearchModelAsync(
        string functionKey,
        BaseSearchReq request,
        params SqlParamModel[] additionalParams)
    {
        var spName = await GetProcNameAsync(functionKey);
        var model = new SqlExecuteModel(spName) { IsStoredProcedure = true };

        // Add base search parameters
        var baseParams = BuildBaseSearchParams(request);
        model.Params.AddRange(baseParams);

        // Add entity-specific parameters
        if (additionalParams?.Length > 0)
        {
            model.Params.AddRange(additionalParams);
        }

        return model;
    }

    /// <summary>
    /// Executes a paged search operation and returns results with paging metadata.
    /// </summary>
    /// <typeparam name="T">The DTO type for result items</typeparam>
    /// <param name="functionKey">The function key to resolve SP name</param>
    /// <param name="request">The search request containing paging parameters</param>
    /// <param name="additionalParams">Entity-specific filter parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>BaseResponse containing TPaging with items and metadata</returns>
    protected async Task<BaseResponse<TPaging<T>>> ExecutePagedSearchAsync<T>(
        string functionKey,
        BaseSearchReq request,
        SqlParamModel[]? additionalParams = null,
        CancellationToken cancellationToken = default)
        where T : class, new()
    {
        try
        {
            var model = await BuildSearchModelAsync(functionKey, request, additionalParams ?? []);

            Logger.LogDebug(
                "Executing paged search: SP={SpName}, Keyword={Keyword}, Page={Page}, Size={Size}",
                model.SqlProcOrQuery,
                request.NormalizedKeyword,
                request.NormalizedPageIndex,
                request.NormalizedPageSize);

            var result = await SqlService.ExecuteProcReturnPagingAsync<T>(
                model,
                request.NormalizedPageIndex,
                request.NormalizedPageSize,
                cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return HandleException<TPaging<T>>(ex, $"PagedSearch_{functionKey}");
        }
    }

    // ==========================================================================
    // ERROR HANDLING
    // ==========================================================================

    /// <summary>
    /// Handles exceptions and maps them to appropriate BaseResponse errors.
    /// Uses SqlErrorMapper for MySQL-specific error handling.
    /// </summary>
    protected BaseResponse<T> HandleException<T>(Exception ex, string operation)
    {
        Logger.LogError(ex, "Error during {Operation}", operation);

        // Try to get user-friendly message from SQL error
        var userMessage = SqlErrorMapper.GetUserFriendlyMessage(ex);

        return BaseResponse<T>.Error(userMessage, SqlConstants.ResponseCode_Error);
    }

    /// <summary>
    /// Maps SP output parameters to BaseResponse.
    /// Checks p_response_code and p_response_message from stored procedure.
    /// </summary>
    protected BaseResponse<T> MapSpResponse<T>(BaseResponse<T> result)
    {
        if (!result.Success)
        {
            // Check if this is a known error that needs user-friendly mapping
            if (result.ErrorCode != 0)
            {
                var httpStatus = SqlErrorMapper.MapSpErrorCodeToHttpStatus(result.ErrorCode);
                Logger.LogWarning("SP returned error code {Code}: {Message}", result.ErrorCode, result.Message);
            }
        }

        return result;
    }

    // ==========================================================================
    // VALIDATION HELPERS
    // ==========================================================================

    /// <summary>
    /// Validates that a required field is not null or empty.
    /// </summary>
    protected BaseResponse<T>? ValidateRequired<T>(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return BaseResponse<T>.ValidationError($"{fieldName} là bắt buộc.");
        }
        return null;
    }

    /// <summary>
    /// Validates date range (from must be <= to).
    /// </summary>
    protected BaseResponse<T>? ValidateDateRange<T>(DateTime from, DateTime to)
    {
        if (from > to)
        {
            return BaseResponse<T>.ValidationError("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        }
        return null;
    }

    /// <summary>
    /// Validates a search request including date range.
    /// </summary>
    protected BaseResponse<T>? ValidateSearchRequest<T>(BaseSearchReq request)
    {
        if (!request.IsDateRangeValid())
        {
            return BaseResponse<T>.ValidationError("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        }
        return null;
    }

    // ==========================================================================
    // UTILITY HELPERS
    // ==========================================================================

    /// <summary>
    /// Converts empty/whitespace string to null for nullable SP parameters.
    /// SP Pattern: WHERE (@param IS NULL OR column = @param)
    /// </summary>
    protected static object? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// Converts default DateTime to null.
    /// </summary>
    protected static object? NullIfDefault(DateTime? value)
        => value.HasValue && value.Value != default ? value : null;

    /// <summary>
    /// Validates and normalizes sort column name to prevent injection.
    /// Only allows alphanumeric characters and underscores.
    /// </summary>
    protected static string? SanitizeSortColumn(string? sortBy, HashSet<string> allowedColumns)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return null;

        var normalized = sortBy.Trim();

        // Check against whitelist of allowed columns
        if (allowedColumns.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            return normalized;
        }

        // Log attempted invalid column (potential injection attempt)
        return null;
    }
}
