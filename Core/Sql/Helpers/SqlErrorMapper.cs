namespace Core.Sql.Helpers;

/// <summary>
/// Helper class to map MySQL error numbers to user-friendly Vietnamese messages.
/// This provides a consistent error handling mechanism across the application.
/// </summary>
public static class SqlErrorMapper
{
    /// <summary>
    /// Common MySQL error codes and their user-friendly messages.
    /// </summary>
    private static readonly Dictionary<int, string> MySqlErrorMessages = new()
    {
        // Duplicate entry errors
        { 1062, "Dữ liệu này đã tồn tại (Trùng mã hoặc biển số)." },
        { 1586, "Dữ liệu trùng lặp với một bản ghi khác trong hệ thống." },

        // Foreign key constraint errors
        { 1451, "Không thể xóa/sửa đối tượng này vì đang được sử dụng trong dữ liệu khác (ví dụ: Xe đang có chuyến chạy, Tài xế đang có lịch trình)." },
        { 1452, "Dữ liệu tham chiếu không tồn tại. Vui lòng kiểm tra mã xe, mã tuyến, hoặc mã tài xế." },

        // Data integrity errors  
        { 1048, "Thiếu thông tin bắt buộc. Vui lòng điền đầy đủ các trường yêu cầu." },
        { 1054, "Lỗi cấu trúc dữ liệu. Vui lòng liên hệ quản trị viên." },
        { 1064, "Lỗi cú pháp truy vấn. Vui lòng liên hệ quản trị viên." },

        // Data type/range errors
        { 1264, "Giá trị vượt quá giới hạn cho phép của trường dữ liệu." },
        { 1265, "Dữ liệu không hợp lệ đã được cắt bớt. Vui lòng kiểm tra độ dài dữ liệu." },
        { 1292, "Định dạng ngày giờ không hợp lệ." },
        { 1366, "Giá trị không hợp lệ cho trường dữ liệu." },

        // Connection errors
        { 1040, "Hệ thống đang quá tải. Vui lòng thử lại sau." },
        { 1042, "Không thể kết nối đến máy chủ cơ sở dữ liệu." },
        { 1045, "Lỗi xác thực kết nối cơ sở dữ liệu." },

        // Lock/timeout errors
        { 1205, "Thao tác bị timeout do xung đột khóa. Vui lòng thử lại." },
        { 1213, "Xảy ra deadlock khi thực hiện thao tác. Vui lòng thử lại." },

        // Table errors
        { 1146, "Bảng dữ liệu không tồn tại. Vui lòng liên hệ quản trị viên." },

        // Stored procedure errors
        { 1305, "Stored procedure không tồn tại. Vui lòng kiểm tra cấu hình hệ thống." },
        { 1318, "Số lượng tham số không đúng. Vui lòng liên hệ quản trị viên." },

        // Permission errors
        { 1142, "Không có quyền thực hiện thao tác này." },
        { 1044, "Không có quyền truy cập cơ sở dữ liệu." },
    };

    /// <summary>
    /// Maps a MySQL error number to a user-friendly message.
    /// </summary>
    /// <param name="errorNumber">The MySQL error number</param>
    /// <returns>User-friendly error message in Vietnamese</returns>
    public static string GetUserFriendlyMessage(int errorNumber)
    {
        return MySqlErrorMessages.TryGetValue(errorNumber, out var message)
            ? message
            : "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại hoặc liên hệ quản trị viên.";
    }

    /// <summary>
    /// Attempts to extract MySQL error number from an exception and map to friendly message.
    /// </summary>
    public static string GetUserFriendlyMessage(Exception exception)
    {
        // Try to get MySqlException error number
        if (exception is MySqlConnector.MySqlException mySqlEx)
        {
            return GetUserFriendlyMessage((int)mySqlEx.Number);
        }

        // Check inner exception
        if (exception.InnerException is MySqlConnector.MySqlException innerMySqlEx)
        {
            return GetUserFriendlyMessage((int)innerMySqlEx.Number);
        }

        // Default message
        return "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại.";
    }

    /// <summary>
    /// Determines if an error is a constraint violation that should be shown to user.
    /// </summary>
    public static bool IsConstraintViolation(int errorNumber)
    {
        return errorNumber switch
        {
            1062 or 1451 or 1452 or 1586 => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if an error is a connection/infrastructure issue.
    /// </summary>
    public static bool IsInfrastructureError(int errorNumber)
    {
        return errorNumber switch
        {
            1040 or 1042 or 1045 or 1205 or 1213 => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the appropriate HTTP status code for a MySQL error.
    /// </summary>
    public static int GetHttpStatusCode(int errorNumber)
    {
        return errorNumber switch
        {
            1062 or 1586 => 409, // Conflict
            1451 or 1452 => 409, // Conflict (FK violation)
            1048 or 1264 or 1265 or 1292 or 1366 => 400, // Bad Request
            1142 or 1044 => 403, // Forbidden
            1146 or 1305 => 500, // Internal Server Error (missing table/SP)
            1040 or 1042 or 1045 => 503, // Service Unavailable
            1205 or 1213 => 503, // Service Unavailable (retry)
            _ => 500 // Internal Server Error
        };
    }

    /// <summary>
    /// Maps stored procedure custom error codes (from p_response_code) to HTTP status codes.
    /// Convention: -1 = validation error, -2 = business rule violation, -3 = not found
    /// </summary>
    public static int MapSpErrorCodeToHttpStatus(int spErrorCode)
    {
        return spErrorCode switch
        {
            0 => 200,    // Success
            -1 => 500,   // General error
            -2 => 400,   // Validation/Business rule error
            -3 => 404,   // Not found
            -4 => 409,   // Duplicate
            -5 => 401,   // Unauthorized
            _ => 500     // Unknown
        };
    }
}
