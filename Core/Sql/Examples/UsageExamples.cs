/*
 * ============================================================================
 * SQL EXECUTION SERVICE - USAGE EXAMPLES
 * ============================================================================
 * This file demonstrates how to use the SQL Execution Service in various
 * scenarios. These examples follow the Clean Architecture pattern.
 * ============================================================================
 */

using Core.Sql.Models;
using Core.Sql.Services;
using MySqlConnector;

namespace Core.Sql.Examples;

/// <summary>
/// Sample entity for demonstration purposes.
/// </summary>
public class UserEntity
{
    [CustomDataSet("user_id")]
    public int UserId { get; set; }

    [CustomDataSet("full_name")]
    public string FullName { get; set; } = string.Empty;

    [CustomDataSet("email")]
    public string Email { get; set; } = string.Empty;

    [CustomDataSet("is_active")]
    public bool IsActive { get; set; }

    [CustomDataSet("created_at")]
    public DateTime CreatedAt { get; set; }

    // This will match column "role_id" via snake_case conversion
    public int RoleId { get; set; }
}

/// <summary>
/// Sample repository demonstrating ISqlExecuteService usage.
/// </summary>
public class UserRepository
{
    private readonly ISqlExecuteService _sqlService;

    public UserRepository(ISqlExecuteService sqlService)
    {
        _sqlService = sqlService;
    }

    // =========================================================================
    // EXAMPLE 1: Get paginated list of users
    // =========================================================================
    public async Task<BaseResponse<TPaging<UserEntity>>> GetUsersPaginatedAsync(
        int pageIndex,
        int pageSize,
        string? searchTerm = null)
    {
        var model = SqlExecuteModel.StoredProcedure("sp_GetUsers")
            .AddInput("p_SearchTerm", searchTerm);

        return await _sqlService.ExecuteProcReturnPagingAsync<UserEntity>(
            model, pageIndex, pageSize);
    }

    // =========================================================================
    // EXAMPLE 2: Get single user by ID
    // =========================================================================
    public async Task<BaseResponse<UserEntity?>> GetUserByIdAsync(int userId)
    {
        var model = SqlExecuteModel.StoredProcedure("sp_GetUserById")
            .AddInput("p_UserId", userId);

        return await _sqlService.ExecuteProcReturnSingleAsync<UserEntity>(model);
    }

    // =========================================================================
    // EXAMPLE 3: Get list of users (no paging)
    // =========================================================================
    public async Task<BaseResponse<List<UserEntity>>> GetActiveUsersAsync()
    {
        var model = SqlExecuteModel.StoredProcedure("sp_GetActiveUsers");

        return await _sqlService.ExecuteProceReturnAsync<UserEntity>(model);
    }

    // =========================================================================
    // EXAMPLE 4: Create/Update user (CUD operation)
    // =========================================================================
    public async Task<BaseResponse<object>> CreateUserAsync(
        string fullName,
        string email,
        int roleId)
    {
        var model = new SqlExecuteModel("sp_CreateUser")
        {
            Params =
            [
                SqlParamModel.Input("p_FullName", fullName),
                SqlParamModel.Input("p_Email", email),
                SqlParamModel.Input("p_RoleId", roleId),
                // Output parameters for response
                SqlParamModel.Output("p_ResponseCode", MySqlDbType.Int32),
                SqlParamModel.Output("p_ResponseMessage", MySqlDbType.VarChar, 500),
                SqlParamModel.Output("p_LastInsertId", MySqlDbType.Int64)
            ]
        };

        return await _sqlService.ProcExecuteNonQueryAsync(model);
    }

    // =========================================================================
    // EXAMPLE 5: Batch insert users (transactional)
    // =========================================================================
    public async Task<BaseResponse<BatchExecutionResult>> CreateUsersBatchAsync(
        List<(string Name, string Email)> users)
    {
        var batchModel = SqlExecuteBatchModel.StoredProcedure("sp_CreateUser");

        foreach (var user in users)
        {
            batchModel.AddBatchItem(
                SqlParamModel.Input("p_FullName", user.Name),
                SqlParamModel.Input("p_Email", user.Email)
            );
        }

        return await _sqlService.ProcExecuteBatchAsync(batchModel);
    }

    // =========================================================================
    // EXAMPLE 6: Execute MySQL function
    // =========================================================================
    public async Task<BaseResponse<int>> GetUserCountAsync()
    {
        var model = SqlExecuteModel.StoredProcedure("fn_GetUserCount");
        return await _sqlService.ExecuteFunctionReturnAsync<int>(model);
    }

    // =========================================================================
    // EXAMPLE 7: Raw SQL query (use with caution!)
    // =========================================================================
    public async Task<BaseResponse<List<UserEntity>>> SearchUsersRawAsync(string searchTerm)
    {
        // IMPORTANT: Always use parameters to prevent SQL injection!
        var model = SqlExecuteModel.RawQuery(
            @"SELECT user_id, full_name, email, is_active, created_at, role_id 
              FROM users 
              WHERE full_name LIKE @SearchTerm OR email LIKE @SearchTerm 
              LIMIT 100")
            .AddInput("SearchTerm", $"%{searchTerm}%");

        return await _sqlService.ExecuteSqlRawCommandAsync<UserEntity>(model);
    }

    // =========================================================================
    // EXAMPLE 8: Fire-and-forget background execution
    // =========================================================================
    public void LogUserActivityBackground(int userId, string action)
    {
        var model = SqlExecuteModel.StoredProcedure("sp_LogUserActivity")
            .AddInput("p_UserId", userId)
            .AddInput("p_Action", action)
            .AddInput("p_Timestamp", DateTime.UtcNow);

        // This won't block the main thread
        _sqlService.ProcExecuteBackgroundAsync(model, (success, error) =>
        {
            if (!success)
            {
                // Handle error (e.g., queue for retry)
                Console.WriteLine($"Background logging failed: {error}");
            }
        });
    }

    // =========================================================================
    // EXAMPLE 9: Using with output parameters
    // =========================================================================
    public async Task<(bool Success, long? NewId, string Message)> CreateUserWithOutputAsync(
        string fullName,
        string email)
    {
        var model = new SqlExecuteModel("sp_CreateUserWithOutput")
        {
            Params =
            [
                SqlParamModel.Input("p_FullName", fullName),
                SqlParamModel.Input("p_Email", email),
                SqlParamModel.Output("p_NewUserId", MySqlDbType.Int64),
                SqlParamModel.Output("p_ResponseCode", MySqlDbType.Int32),
                SqlParamModel.Output("p_ResponseMessage", MySqlDbType.VarChar, 500)
            ]
        };

        var result = await _sqlService.ProcExecuteNonQueryAsync(model);

        if (result.Success && result.Data is Dictionary<string, object?> outputs)
        {
            var newId = outputs.TryGetValue("p_NewUserId", out var id)
                ? Convert.ToInt64(id)
                : (long?)null;

            return (true, newId, result.Message);
        }

        return (false, null, result.Message);
    }
}

// =============================================================================
// PROGRAM.CS SETUP EXAMPLE
// =============================================================================
/*
 * Add this to your Program.cs or Startup.cs:
 * 
 * // In ConfigureServices or Program.cs:
 * builder.Services.AddSqlExecuteService();
 * 
 * // Or with options:
 * builder.Services.AddSqlExecuteService(options =>
 * {
 *     options.DefaultCommandTimeoutSeconds = 60;
 *     options.EnableDetailedLogging = builder.Environment.IsDevelopment();
 * });
 * 
 * // Connection string in appsettings.json:
 * {
 *   "ConnectionStrings": {
 *     "DefaultConnection": "Server=localhost;Database=mydb;User=root;Password=secret;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;"
 *   }
 * }
 */

// =============================================================================
// SAMPLE STORED PROCEDURES (MySQL)
// =============================================================================
/*
 * -- Paging example with output parameter
 * DELIMITER //
 * CREATE PROCEDURE sp_GetUsers(
 *     IN p_SearchTerm VARCHAR(100),
 *     IN p_PageIndex INT,
 *     IN p_PageSize INT,
 *     OUT p_TotalRecord BIGINT
 * )
 * BEGIN
 *     DECLARE v_Offset INT;
 *     SET v_Offset = (p_PageIndex - 1) * p_PageSize;
 *     
 *     -- Get total count
 *     SELECT COUNT(*) INTO p_TotalRecord 
 *     FROM users 
 *     WHERE p_SearchTerm IS NULL 
 *        OR full_name LIKE CONCAT('%', p_SearchTerm, '%');
 *     
 *     -- Return paged data
 *     SELECT user_id, full_name, email, is_active, created_at, role_id
 *     FROM users
 *     WHERE p_SearchTerm IS NULL 
 *        OR full_name LIKE CONCAT('%', p_SearchTerm, '%')
 *     ORDER BY created_at DESC
 *     LIMIT p_PageSize OFFSET v_Offset;
 * END //
 * DELIMITER ;
 * 
 * -- CUD operation with response codes
 * DELIMITER //
 * CREATE PROCEDURE sp_CreateUser(
 *     IN p_FullName VARCHAR(100),
 *     IN p_Email VARCHAR(255),
 *     IN p_RoleId INT,
 *     OUT p_ResponseCode INT,
 *     OUT p_ResponseMessage VARCHAR(500),
 *     OUT p_LastInsertId BIGINT
 * )
 * BEGIN
 *     DECLARE EXIT HANDLER FOR 1062
 *     BEGIN
 *         SET p_ResponseCode = -4;
 *         SET p_ResponseMessage = 'Email already exists';
 *     END;
 *     
 *     INSERT INTO users (full_name, email, role_id, is_active, created_at)
 *     VALUES (p_FullName, p_Email, p_RoleId, 1, NOW());
 *     
 *     SET p_LastInsertId = LAST_INSERT_ID();
 *     SET p_ResponseCode = 0;
 *     SET p_ResponseMessage = 'User created successfully';
 * END //
 * DELIMITER ;
 */
