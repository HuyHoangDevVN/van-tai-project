namespace Core.Sql.Models;

/// <summary>
/// Contains constant values used throughout the SQL execution layer.
/// These can be configured based on your stored procedure conventions.
/// </summary>
public static class SqlConstants
{
    #region Output Parameter Names

    /// <summary>
    /// Standard output parameter name for total record count in paging operations.
    /// IMPORTANT: Must match the SP parameter name convention (snake_case).
    /// </summary>
    public const string P_TotalRecord = "p_total_record";

    /// <summary>
    /// Standard output parameter name for response/status code.
    /// </summary>
    public const string P_ResponseCode = "p_ResponseCode";

    /// <summary>
    /// Standard output parameter name for response message.
    /// </summary>
    public const string P_ResponseMessage = "p_ResponseMessage";

    /// <summary>
    /// Standard output parameter name for affected row count.
    /// </summary>
    public const string P_AffectedRows = "p_AffectedRows";

    /// <summary>
    /// Standard output parameter name for last inserted ID.
    /// </summary>
    public const string P_LastInsertId = "p_LastInsertId";

    #endregion

    #region Response Codes

    /// <summary>
    /// Response code indicating successful operation.
    /// </summary>
    public const int ResponseCode_Success = 0;

    /// <summary>
    /// Response code indicating a general error.
    /// </summary>
    public const int ResponseCode_Error = -1;

    /// <summary>
    /// Response code indicating validation failure.
    /// </summary>
    public const int ResponseCode_ValidationError = -2;

    /// <summary>
    /// Response code indicating not found.
    /// </summary>
    public const int ResponseCode_NotFound = -3;

    /// <summary>
    /// Response code indicating duplicate entry.
    /// </summary>
    public const int ResponseCode_Duplicate = -4;

    /// <summary>
    /// Response code indicating unauthorized access.
    /// </summary>
    public const int ResponseCode_Unauthorized = -5;

    #endregion

    #region Default Messages

    /// <summary>
    /// Default success message.
    /// </summary>
    public const string Message_Success = "Operation completed successfully.";

    /// <summary>
    /// Default error message (sanitized for user display).
    /// </summary>
    public const string Message_Error = "An error occurred while processing your request.";

    /// <summary>
    /// Default database connection error message.
    /// </summary>
    public const string Message_ConnectionError = "Unable to connect to the database. Please try again later.";

    /// <summary>
    /// Default timeout error message.
    /// </summary>
    public const string Message_TimeoutError = "The operation timed out. Please try again.";

    /// <summary>
    /// Default validation error message.
    /// </summary>
    public const string Message_ValidationError = "The provided data is invalid.";

    #endregion

    #region Configuration

    /// <summary>
    /// Default command timeout in seconds.
    /// </summary>
    public const int DefaultCommandTimeoutSeconds = 30;

    /// <summary>
    /// Default page size for paging operations.
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Maximum allowed page size to prevent memory issues.
    /// </summary>
    public const int MaxPageSize = 1000;

    /// <summary>
    /// Default page index (1-based).
    /// </summary>
    public const int DefaultPageIndex = 1;

    #endregion
}
