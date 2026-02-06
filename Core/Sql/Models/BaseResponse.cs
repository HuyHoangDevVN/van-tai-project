using System.Text.Json.Serialization;

namespace Core.Sql.Models;

/// <summary>
/// Standard response wrapper for all API/service responses.
/// Provides consistent structure for success, error, and data handling.
/// </summary>
/// <typeparam name="T">The type of data contained in the response.</typeparam>
public class BaseResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message describing the result.
    /// For errors, this contains a user-friendly message (not raw exception details).
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The response data. Null if the operation failed or returned no data.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// Error code for programmatic error handling.
    /// 0 = Success, negative values = various error types.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; set; }

    /// <summary>
    /// Optional: Additional error details for debugging (only populated in development).
    /// </summary>
    [JsonPropertyName("debugInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DebugInfo { get; set; }

    /// <summary>
    /// Timestamp when the response was generated.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new empty BaseResponse.
    /// </summary>
    public BaseResponse() { }

    /// <summary>
    /// Creates a new BaseResponse with specified values.
    /// </summary>
    public BaseResponse(bool success, string message, T? data = default, int errorCode = 0)
    {
        Success = success;
        Message = message;
        Data = data;
        ErrorCode = errorCode;
    }

    #region Factory Methods

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static BaseResponse<T> Ok(T data, string message = "Operation completed successfully.")
        => new(true, message, data, SqlConstants.ResponseCode_Success);

    /// <summary>
    /// Creates a successful response without data.
    /// </summary>
    public static BaseResponse<T> Ok(string message = "Operation completed successfully.")
        => new(true, message, default, SqlConstants.ResponseCode_Success);

    /// <summary>
    /// Creates an error response.
    /// </summary>
    public static BaseResponse<T> Error(string message, int errorCode = SqlConstants.ResponseCode_Error)
        => new(false, message, default, errorCode);

    /// <summary>
    /// Creates a not found response.
    /// </summary>
    public static BaseResponse<T> NotFound(string message = "The requested resource was not found.")
        => new(false, message, default, SqlConstants.ResponseCode_NotFound);

    /// <summary>
    /// Creates a validation error response.
    /// </summary>
    public static BaseResponse<T> ValidationError(string message)
        => new(false, message, default, SqlConstants.ResponseCode_ValidationError);

    /// <summary>
    /// Creates a duplicate entry error response.
    /// </summary>
    public static BaseResponse<T> Duplicate(string message = "A record with the same key already exists.")
        => new(false, message, default, SqlConstants.ResponseCode_Duplicate);

    /// <summary>
    /// Creates a response from a stored procedure output.
    /// </summary>
    public static BaseResponse<T> FromProcedureOutput(int responseCode, string responseMessage, T? data = default)
    {
        return new BaseResponse<T>
        {
            Success = responseCode == SqlConstants.ResponseCode_Success,
            Message = responseMessage,
            Data = data,
            ErrorCode = responseCode
        };
    }

    #endregion
}

/// <summary>
/// Non-generic version for operations that don't return data.
/// </summary>
public class BaseResponse : BaseResponse<object>
{
    public BaseResponse() : base() { }

    public BaseResponse(bool success, string message, int errorCode = 0)
        : base(success, message, null, errorCode) { }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public new static BaseResponse Ok(string message = "Operation completed successfully.")
        => new(true, message, SqlConstants.ResponseCode_Success);

    /// <summary>
    /// Creates an error response.
    /// </summary>
    public new static BaseResponse Error(string message, int errorCode = SqlConstants.ResponseCode_Error)
        => new(false, message, errorCode);
}
