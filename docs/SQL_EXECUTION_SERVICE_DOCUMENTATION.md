# SQL Execution Service - Documentation & Prompt Guide

## 📋 Mục lục

1. [Tổng quan kiến trúc](#1-tổng-quan-kiến-trúc)
2. [Các Models cơ bản](#2-các-models-cơ-bản)
3. [Interface ISqlExecuteService](#3-interface-isqlexecuteservice)
4. [Extension Methods hỗ trợ](#4-extension-methods-hỗ-trợ)
5. [Cách sử dụng](#5-cách-sử-dụng)
6. [Prompt Template tạo dự án mới](#6-prompt-template-tạo-dự-án-mới)

---

## 1. Tổng quan kiến trúc

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Layer                         │
│                     (Controllers / Services)                     │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      ISqlExecuteService                          │
│  ┌─────────────────┬──────────────────┬─────────────────────┐   │
│  │ ExecuteProc     │ ExecuteFunction  │ ExecuteRawSQL       │   │
│  │ ReturnPaging    │ Return           │ Command             │   │
│  └─────────────────┴──────────────────┴─────────────────────┘   │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DbContext / MySqlConnector                  │
│                    (Database Connection Layer)                   │
└─────────────────────────────────────────────────────────────────┘
```

### Các thành phần chính:

- **ISqlExecuteService**: Interface định nghĩa các phương thức execute SQL
- **SqlParamModel**: Model chứa thông tin parameter cho stored procedure
- **SqlExecuteModel**: Model chứa kết quả trả về từ stored procedure
- **DataSetExtensions**: Extension chuyển đổi DataTable → Model
- **CustomDataSetAttribute**: Attribute mapping column name với property name

---

## 2. Các Models cơ bản

### 2.1 SqlParamModel - Parameter cho Stored Procedure

```csharp
using MySqlConnector;
using System.Data;

namespace YourNamespace.SqlModels
{
    public class SqlParamModel
    {
        public SqlParamModel(
            string paramName,
            object? paramValue,
            ParameterDirection parameterDirection,
            MySqlDbType mySqlDbType)
        {
            ParamName = paramName;
            ParamValue = paramValue;
            ParameterDirection = parameterDirection;
            MySqlDbType = mySqlDbType;
        }

        public string ParamName { get; }
        public object? ParamValue { get; }
        public ParameterDirection ParameterDirection { get; }
        public MySqlDbType MySqlDbType { get; }
    }
}
```

**Giải thích:**

- `ParamName`: Tên parameter trong stored procedure (vd: `p_user_id`)
- `ParamValue`: Giá trị truyền vào
- `ParameterDirection`: Input/Output/InputOutput
- `MySqlDbType`: Kiểu dữ liệu MySQL (VarChar, Int32, DateTime, etc.)

### 2.2 SqlExecuteModel - Kết quả Execute

```csharp
using System.Collections.Concurrent;

namespace YourNamespace.SqlModels
{
    public class SqlExecuteModel
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ConcurrentDictionary<string, object> OutDics { get; set; }
    }
}
```

**Giải thích:**

- `ErrorCode`: Mã lỗi trả về (1 = SUCCESS, -1 = ERROR)
- `ErrorMessage`: Message mô tả kết quả
- `OutDics`: Dictionary chứa các output parameters khác

### 2.3 SqlConstants - Các hằng số

```csharp
namespace YourNamespace.SqlModels
{
    public static class SqlConstants
    {
        public static class OutputParams
        {
            public static string O_ERROR_CODE = "O_CODE";
            public static string O_ERROR_MESSAGE = "O_MESSAGE";
        }

        public static class OutputValue
        {
            public static int SUCCESS = 1;
            public static int ERROR = -1;
        }
    }
}
```

### 2.4 BaseSqlPagingRes - Base class cho kết quả phân trang

```csharp
namespace YourNamespace.SqlModels
{
    public interface IBaseSqlPagingRes
    {
        public int TotalRow { get; set; }
    }

    public class BaseSqlPagingRes : IBaseSqlPagingRes
    {
        [CustomDataSet("TOTAL_ROW")]
        public int TotalRow { get; set; }
    }
}
```

### 2.5 CustomDataSetAttribute - Mapping Column Name

```csharp
namespace YourNamespace.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class CustomDataSetAttribute : Attribute
    {
        public CustomDataSetAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
```

**Mục đích:** Khi column name trong DB khác với property name trong C#, dùng attribute này để mapping.

```csharp
// Ví dụ:
public class UserResponse : BaseSqlPagingRes
{
    [CustomDataSet("USER_ID")]
    public int UserId { get; set; }

    [CustomDataSet("FULL_NAME")]
    public string FullName { get; set; }
}
```

### 2.6 TPaging - Response phân trang

```csharp
namespace YourNamespace.Models
{
    public class TPaging<T>
    {
        public IEnumerable<T> Source { get; set; }
        public int TotalRecords { get; set; }
        public int Pages { get; set; }
    }
}
```

### 2.7 BaseResponse - Response chuẩn cho API

```csharp
using Newtonsoft.Json;

namespace YourNamespace.Models
{
    public class BaseResponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("messageCode")]
        public string MessageCode { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; } = default;
    }
}
```

---

## 3. Interface ISqlExecuteService

### 3.1 Interface Definition

```csharp
public interface ISqlExecuteService
{
    // 1. Execute stored procedure trả về danh sách có phân trang
    Task<TPaging<T>> ExecuteProcReturnPagingAsync<T>(
        string funcName,
        IEnumerable<SqlParamModel> sqlParams) where T : IBaseSqlPagingRes;

    // 2. Execute stored procedure trả về danh sách
    Task<IEnumerable<T>> ExecuteProceReturnAsync<T>(
        string funcName,
        IEnumerable<SqlParamModel> sqlParams);

    // 3. Execute stored procedure không trả về data (INSERT/UPDATE/DELETE)
    Task<SqlExecuteModel> ProcExecuteNonQueryAsync(
        string procName,
        IEnumerable<SqlParamModel> sqlParams,
        bool? noLogTimeOut = false);

    // 4. Execute raw SQL command
    Task<BaseResponse<bool>> ExecuteSqlRawCommandAsync(string sql);

    // 5. Execute MySQL Function trả về giá trị đơn
    Task<T> ExecuteFunctionReturnAsync<T>(
        string funcName,
        IEnumerable<SqlParamModel> sqlParams);

    // 6. Execute stored procedure chạy background (fire and forget)
    Task<bool> ProcExecuteBackgroundAsync(
        string procName,
        IEnumerable<SqlParamModel> sqlParams);
}
```

### 3.2 Implementation Chi tiết

#### 3.2.1 ExecuteProcReturnPagingAsync - Lấy danh sách có phân trang

```csharp
public async Task<TPaging<T>> ExecuteProcReturnPagingAsync<T>(
    string funcName,
    IEnumerable<SqlParamModel> sqlParams) where T : IBaseSqlPagingRes
{
    try
    {
        DataSet ds = new DataSet();
        var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        try
        {
            using (var command = (MySqlCommand)connection.CreateCommand())
            {
                command.CommandText = funcName;
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                foreach (var p in sqlParams)
                {
                    if (!command.Parameters.Cast<MySqlParameter>()
                        .Any(param => param.ParameterName == p.ParamName))
                    {
                        var param = new MySqlParameter
                        {
                            ParameterName = p.ParamName,
                            MySqlDbType = p.MySqlDbType,
                            Direction = p.ParameterDirection
                        };

                        if (p.ParamValue != null)
                        {
                            param.Value = p.ParamValue;
                        }

                        command.Parameters.Add(param);
                    }
                }

                await command.ExecuteNonQueryAsync();
                MySqlDataAdapter da = new MySqlDataAdapter(command);
                da.Fill(ds);
            }
        }
        finally
        {
            await connection.CloseAsync();
        }

        if (ds.Tables.Count == 0)
        {
            return new TPaging<T> { Source = Enumerable.Empty<T>() };
        }

        // Convert DataTable to Model
        var res = ds.Tables[0].DataTableToModel<T>();
        return new TPaging<T>
        {
            Source = res,
            TotalRecords = res != null && res.Any() ? res.FirstOrDefault().TotalRow : 0
        };
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error: {funcName} - {ex.Message}");
        return new TPaging<T> { Source = Enumerable.Empty<T>() };
    }
}
```

#### 3.2.2 ProcExecuteNonQueryAsync - Execute không trả về data

```csharp
public async Task<SqlExecuteModel> ProcExecuteNonQueryAsync(
    string procName,
    IEnumerable<SqlParamModel> sqlParams,
    bool? noLogTimeOut = false)
{
    try
    {
        var res = new SqlExecuteModel
        {
            OutDics = new ConcurrentDictionary<string, object>()
        };

        var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        try
        {
            using (var command = (MySqlCommand)connection.CreateCommand())
            {
                command.CommandText = procName;
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                foreach (var p in sqlParams)
                {
                    if (!command.Parameters.Cast<MySqlParameter>()
                        .Any(param => param.ParameterName == p.ParamName))
                    {
                        var param = new MySqlParameter
                        {
                            ParameterName = p.ParamName,
                            MySqlDbType = p.MySqlDbType,
                            Direction = p.ParameterDirection
                        };

                        if (p.ParamValue != null)
                        {
                            param.Value = p.ParamValue;
                        }

                        command.Parameters.Add(param);
                    }
                }

                await command.ExecuteNonQueryAsync();

                // Read output parameters
                foreach (var param in sqlParams.Where(s => s.ParameterDirection == ParameterDirection.Output))
                {
                    var outVal = command.Parameters[param.ParamName].Value;

                    if (outVal == DBNull.Value) continue;

                    if (param.ParamName == SqlConstants.OutputParams.O_ERROR_CODE)
                    {
                        res.ErrorCode = int.Parse(outVal.ToString());
                    }
                    else if (param.ParamName == SqlConstants.OutputParams.O_ERROR_MESSAGE)
                    {
                        res.ErrorMessage = outVal.ToString();
                    }
                    else
                    {
                        res.OutDics.TryAdd(param.ParamName, outVal);
                    }
                }
            }
        }
        finally
        {
            await connection.CloseAsync();
        }

        return res;
    }
    catch (Exception ex)
    {
        return new SqlExecuteModel
        {
            ErrorCode = SqlConstants.OutputValue.ERROR,
            ErrorMessage = ex.Message
        };
    }
}
```

---

## 4. Extension Methods hỗ trợ

### 4.1 DataTableToModel - Chuyển DataTable thành List<T>

```csharp
public static class DataSetExtensions
{
    public static IEnumerable<T> DataTableToModel<T>(this DataTable dataTable)
    {
        var rows = new List<T>();
        var propertiesInfo = typeof(T).GetProperties();

        foreach (DataRow row in dataTable.Rows)
        {
            var entityModel = (T)Activator.CreateInstance(typeof(T));

            foreach (PropertyInfo propertyInfo in propertiesInfo)
            {
                var fieldName = propertyInfo.Name;

                // Check CustomDataSetAttribute để lấy tên column thực
                var attributes = propertyInfo.GetCustomAttributes(typeof(CustomDataSetAttribute), true);
                if (attributes.Any())
                {
                    var control = attributes[0] as CustomDataSetAttribute;
                    if (control != null)
                    {
                        fieldName = control.Name;
                    }
                }

                // Skip nếu column không tồn tại
                if (!row.Table.Columns.Contains(fieldName))
                {
                    continue;
                }

                var colVal = row[fieldName];
                if (colVal != DBNull.Value)
                {
                    var field = entityModel.GetType().GetProperty(propertyInfo.Name);

                    // Handle boolean (MySQL trả về 1/0)
                    if (field.PropertyType == typeof(bool) || field.PropertyType == typeof(bool?))
                    {
                        colVal = colVal.ToString() == "1";
                    }

                    SetValueToObject(entityModel, field, colVal);
                }
            }

            rows.Add(entityModel);
        }

        return rows;
    }

    private static void SetValueToObject<T>(T entityModel, PropertyInfo field, object value)
    {
        if (field == null) return;

        var propertyType = field.PropertyType;

        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
        {
            if (DateTime.TryParse(value.ToString(), out DateTime date) && date != DateTime.MinValue)
            {
                field.SetValue(entityModel, date);
            }
        }
        else if (propertyType == typeof(int) || propertyType == typeof(int?))
        {
            if (int.TryParse(value.ToString(), out var val))
            {
                field.SetValue(entityModel, val);
            }
        }
        else if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
        {
            if (decimal.TryParse(value.ToString(), out var val))
            {
                field.SetValue(entityModel, val);
            }
        }
        else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
        {
            if (bool.TryParse(value.ToString(), out var val))
            {
                field.SetValue(entityModel, val);
            }
        }
        else if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
        {
            if (Guid.TryParse(value.ToString(), out var val))
            {
                field.SetValue(entityModel, val);
            }
        }
        else
        {
            field.SetValue(entityModel, value);
        }
    }
}
```

---

## 5. Cách sử dụng

### 5.1 Gọi Stored Procedure lấy danh sách có phân trang

```csharp
// Response Model
public class UserListResponse : BaseSqlPagingRes
{
    [CustomDataSet("USER_ID")]
    public int UserId { get; set; }

    [CustomDataSet("USER_NAME")]
    public string UserName { get; set; }

    [CustomDataSet("EMAIL")]
    public string Email { get; set; }

    [CustomDataSet("CREATED_AT")]
    public DateTime CreatedAt { get; set; }
}

// Service usage
public async Task<TPaging<UserListResponse>> GetUsersAsync(int pageIndex, int pageSize, string keyword)
{
    var sqlParams = new List<SqlParamModel>
    {
        new SqlParamModel("p_page_index", pageIndex, ParameterDirection.Input, MySqlDbType.Int32),
        new SqlParamModel("p_page_size", pageSize, ParameterDirection.Input, MySqlDbType.Int32),
        new SqlParamModel("p_keyword", keyword, ParameterDirection.Input, MySqlDbType.VarChar)
    };

    return await _sqlExecuteService.ExecuteProcReturnPagingAsync<UserListResponse>("sp_get_users", sqlParams);
}
```

### 5.2 Gọi Stored Procedure Insert/Update/Delete

```csharp
public async Task<SqlExecuteModel> CreateUserAsync(string userName, string email)
{
    var sqlParams = new List<SqlParamModel>
    {
        new SqlParamModel("p_user_name", userName, ParameterDirection.Input, MySqlDbType.VarChar),
        new SqlParamModel("p_email", email, ParameterDirection.Input, MySqlDbType.VarChar),
        new SqlParamModel(SqlConstants.OutputParams.O_ERROR_CODE, null, ParameterDirection.Output, MySqlDbType.Int32),
        new SqlParamModel(SqlConstants.OutputParams.O_ERROR_MESSAGE, null, ParameterDirection.Output, MySqlDbType.VarChar)
    };

    return await _sqlExecuteService.ProcExecuteNonQueryAsync("sp_create_user", sqlParams);
}

// Controller usage
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    var result = await _userService.CreateUserAsync(request.UserName, request.Email);

    if (result.ErrorCode == SqlConstants.OutputValue.SUCCESS)
    {
        return Ok(new { success = true, message = result.ErrorMessage });
    }

    return BadRequest(new { success = false, message = result.ErrorMessage });
}
```

### 5.3 Stored Procedure mẫu trong MySQL

```sql
-- Stored Procedure lấy danh sách có phân trang
DELIMITER //
CREATE PROCEDURE sp_get_users(
    IN p_page_index INT,
    IN p_page_size INT,
    IN p_keyword VARCHAR(255)
)
BEGIN
    DECLARE v_offset INT;
    SET v_offset = (p_page_index - 1) * p_page_size;

    SELECT
        u.id AS USER_ID,
        u.user_name AS USER_NAME,
        u.email AS EMAIL,
        u.created_at AS CREATED_AT,
        (SELECT COUNT(*) FROM users WHERE user_name LIKE CONCAT('%', p_keyword, '%')) AS TOTAL_ROW
    FROM users u
    WHERE u.user_name LIKE CONCAT('%', p_keyword, '%')
    ORDER BY u.created_at DESC
    LIMIT p_page_size OFFSET v_offset;
END //
DELIMITER ;

-- Stored Procedure Insert với output
DELIMITER //
CREATE PROCEDURE sp_create_user(
    IN p_user_name VARCHAR(255),
    IN p_email VARCHAR(255),
    OUT O_CODE INT,
    OUT O_MESSAGE VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET O_CODE = -1;
        SET O_MESSAGE = 'Error occurred';
        ROLLBACK;
    END;

    START TRANSACTION;

    -- Check duplicate
    IF EXISTS(SELECT 1 FROM users WHERE email = p_email) THEN
        SET O_CODE = -1;
        SET O_MESSAGE = 'Email already exists';
    ELSE
        INSERT INTO users (user_name, email, created_at)
        VALUES (p_user_name, p_email, NOW());

        SET O_CODE = 1;
        SET O_MESSAGE = 'User created successfully';
    END IF;

    COMMIT;
END //
DELIMITER ;
```

---

## 6. Checklist khi tạo dự án mới

- [ ] Tạo SqlModels folder với các models
- [ ] Tạo CustomDataSetAttribute
- [ ] Tạo DataSetExtensions với DataTableToModel<T>
- [ ] Tạo ISqlExecuteService interface
- [ ] Implement SqlExecuteService
- [ ] Configure DbContext trong Program.cs
- [ ] Register services trong DI container
- [ ] Tạo stored procedures trong MySQL
- [ ] Tạo response models với CustomDataSet attributes
- [ ] Test các methods với unit tests

---

## 7. Packages cần thiết

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="MySqlConnector" Version="2.0.0" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.0" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="8.0.0" />
```

---

## 8. Tips & Best Practices

1. **Connection Management**: Luôn đóng connection trong finally block
2. **Error Handling**: Sanitize connection string trong error messages
3. **Logging**: Log đủ thông tin để debug nhưng không log sensitive data
4. **Output Parameters**: Convention sử dụng O_CODE, O_MESSAGE
5. **Column Mapping**: Sử dụng CustomDataSetAttribute khi tên khác nhau
6. **Paging**: TotalRow column trong stored procedure để đếm tổng records
7. **Batch Operations**: Sử dụng ConcurrentDictionary cho thread-safety

---

## 9. Dynamic Stored Procedure Resolution (ADVANCED)

### 9.1 Tổng quan kiến trúc mới

```
┌─────────────────────────────────────────────────────────────────────┐
│                           Controller                                │
└────────────────────────────────┬────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     Feature Service : BaseService                   │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ GetProcNameAsync(FunctionKey) → IProcedureConfigProvider    │   │
│  │ HandleException() → SqlErrorMapper                          │   │
│  │ ValidateRequired(), ValidateDateRange() → Validation        │   │
│  └─────────────────────────────────────────────────────────────┘   │
└────────────────────────────────┬────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│              IProcedureConfigProvider (Singleton + MemoryCache)     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ First Call: Load all from Sys_ProcedureConfig → Cache       │   │
│  │ Subsequent: Return from MemoryCache (30 min TTL)            │   │
│  └─────────────────────────────────────────────────────────────┘   │
└────────────────────────────────┬────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       ISqlExecuteService                            │
│        (Execute the dynamically resolved SP name)                   │
└─────────────────────────────────────────────────────────────────────┘
```

### 9.2 Sys_ProcedureConfig Table

```sql
CREATE TABLE Sys_ProcedureConfig (
    FunctionKey    VARCHAR(100) PRIMARY KEY,
    ProcedureName  VARCHAR(100) NOT NULL,
    Description    VARCHAR(500),
    ModuleName     VARCHAR(50),
    IsActive       BIT DEFAULT 1,
    CreatedAt      DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt      DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Example seed data
INSERT INTO Sys_ProcedureConfig VALUES
('REPORT_CHI_PHI_CO_BAN', 'proc_chi_phi_co_ban', 'Báo cáo chi phí cơ bản', 'Report', 1),
('MAINTENANCE_STATUS_CHECK', 'sp_maintenance_status_check', 'Kiểm tra bảo trì xe', 'Maintenance', 1),
('TRIP_CREATE', 'sp_trip_create', 'Tạo chuyến xe mới', 'Trip', 1);
```

### 9.3 FunctionKeys Constants

```csharp
namespace Core.Sql.Config;

/// <summary>
/// Type-safe function key constants for dynamic SP resolution.
/// Use these instead of hardcoded strings.
/// </summary>
public static class FunctionKeys
{
    public static class Report
    {
        public const string CHI_PHI_CO_BAN = "REPORT_CHI_PHI_CO_BAN";
        public const string DOANH_THU_XE_BUS_NGOI_THANG = "REPORT_DOANH_THU_XE_BUS_NGOI_THANG";
    }

    public static class Maintenance
    {
        public const string STATUS_CHECK = "MAINTENANCE_STATUS_CHECK";
        public const string CREATE = "MAINTENANCE_CREATE";
    }

    public static class Trip
    {
        public const string CREATE = "TRIP_CREATE";
        public const string COMPLETE = "TRIP_COMPLETE";
        public const string SEARCH = "TRIP_SEARCH";
    }
}
```

### 9.4 IProcedureConfigProvider Interface

```csharp
namespace Core.Sql.Config;

public interface IProcedureConfigProvider
{
    /// <summary>
    /// Gets SP name for function key. Throws if not found.
    /// </summary>
    Task<string> GetSpNameAsync(string functionKey);

    /// <summary>
    /// Gets SP name or null if not configured.
    /// </summary>
    Task<string?> GetSpNameOrDefaultAsync(string functionKey);

    /// <summary>
    /// Forces cache refresh.
    /// </summary>
    Task RefreshCacheAsync();

    /// <summary>
    /// Checks if function key exists.
    /// </summary>
    Task<bool> ExistsAsync(string functionKey);
}
```

### 9.5 BaseService Pattern

```csharp
namespace Application.Services;

public abstract class BaseService
{
    protected readonly ISqlExecuteService SqlService;
    protected readonly IProcedureConfigProvider ProcProvider;
    protected readonly ILogger Logger;

    protected BaseService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger logger)
    {
        SqlService = sqlService;
        ProcProvider = procProvider;
        Logger = logger;
    }

    /// <summary>
    /// Gets SP name dynamically. Throws if not configured.
    /// </summary>
    protected async Task<string> GetProcNameAsync(string functionKey)
    {
        var spName = await ProcProvider.GetSpNameAsync(functionKey);
        Logger.LogDebug("Resolved {Key} → {SpName}", functionKey, spName);
        return spName;
    }

    /// <summary>
    /// Handles exceptions with user-friendly messages.
    /// </summary>
    protected BaseResponse<T> HandleException<T>(Exception ex, string operation)
    {
        Logger.LogError(ex, "Error during {Operation}", operation);
        var userMessage = SqlErrorMapper.GetUserFriendlyMessage(ex);
        return BaseResponse<T>.Error(userMessage);
    }
}
```

### 9.6 Feature Service Example (Refactored)

**BEFORE (Hardcoded SP names - AVOID):**

```csharp
public async Task<BaseResponse<List<ChiPhiCoBanDto>>> GetChiPhiCoBanAsync()
{
    // ❌ BAD: Hardcoded SP name
    var model = new SqlExecuteModel("proc_chi_phi_co_ban")
    {
        IsStoredProcedure = true
    };
    return await _sqlService.ExecuteProceReturnAsync<ChiPhiCoBanDto>(model);
}
```

**AFTER (Dynamic SP Resolution - RECOMMENDED):**

```csharp
public class BaoCaoServiceRefactored : BaseService, IBaoCaoService
{
    public async Task<BaseResponse<List<ChiPhiCoBanDto>>> GetChiPhiCoBanAsync()
    {
        try
        {
            // ✅ GOOD: Dynamic SP resolution from database config
            string spName = await GetProcNameAsync(FunctionKeys.Report.CHI_PHI_CO_BAN);

            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true
            };

            return await SqlService.ExecuteProceReturnAsync<ChiPhiCoBanDto>(model);
        }
        catch (Exception ex)
        {
            return HandleException<List<ChiPhiCoBanDto>>(ex, "GetChiPhiCoBan");
        }
    }
}
```

### 9.7 DI Registration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Core SQL Service
builder.Services.AddSqlExecuteService();

// MemoryCache for SP Configuration
builder.Services.AddMemoryCache();

// Procedure Config Provider (Singleton - cached)
builder.Services.AddSingleton<IProcedureConfigProvider, ProcedureConfigProvider>();

// Feature Services (Scoped)
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<ITicketService, TicketService>();
```

### 9.8 SqlErrorMapper - Error Code Translation

```csharp
namespace Core.Sql.Helpers;

public static class SqlErrorMapper
{
    private static readonly Dictionary<int, string> MySqlErrors = new()
    {
        { 1062, "Dữ liệu đã tồn tại trong hệ thống." },    // Duplicate entry
        { 1451, "Không thể xóa. Dữ liệu đang được sử dụng." },  // FK constraint
        { 1452, "Tham chiếu không hợp lệ." },              // FK reference
        { 1406, "Dữ liệu vượt quá độ dài cho phép." }     // Data too long
    };

    public static string GetUserFriendlyMessage(Exception ex)
    {
        if (ex is MySqlException mysqlEx && MySqlErrors.TryGetValue(mysqlEx.Number, out var msg))
            return msg;

        return "Có lỗi xảy ra. Vui lòng thử lại sau.";
    }
}
```

### 9.9 Maintenance Algorithm Flow

```
┌───────────────────────────────────────────────────────────────────┐
│                    MAINTENANCE TRACKING FLOW                       │
├───────────────────────────────────────────────────────────────────┤
│                                                                    │
│  1. Trip Created → Bus assigned, status = "Scheduled"              │
│                                                                    │
│  2. Trip Completed (API: PUT /api/Trip/{id}/complete)             │
│     └─→ sp_trip_complete_update_stats:                            │
│         ├─ Update trip status = "Completed"                       │
│         ├─ Calculate: AddedKm = RouteDistance × DifficultyCoef    │
│         ├─ UPDATE xe SET tong_km_van_hanh += AddedKm              │
│         └─ UPDATE tai_xe SET tong_so_chuyen += 1                  │
│                                                                    │
│  3. Check Maintenance Status (API: GET /api/Maintenance/status)   │
│     └─→ sp_maintenance_status_check:                              │
│         └─ can_bao_tri = (days_since > 360)                       │
│                        OR (tong_km_van_hanh > threshold)          │
│                                                                    │
│  4. Create Maintenance (API: POST /api/Maintenance)               │
│     └─→ sp_maintenance_create:                                    │
│         ├─ INSERT bao_tri record                                  │
│         └─ UPDATE xe SET tong_km_van_hanh = 0 (RESET)             │
│                                                                    │
└───────────────────────────────────────────────────────────────────┘
```

### 9.10 Benefits of Dynamic SP Resolution

| Aspect             | Hardcoded            | Dynamic Resolution       |
| ------------------ | -------------------- | ------------------------ |
| **SP Rename**      | Recompile + Redeploy | Update 1 row in DB       |
| **Feature Toggle** | Code change          | Set IsActive = 0         |
| **Multi-tenant**   | Separate codebase    | Different config tables  |
| **Testing**        | Mock entire service  | Configure test SPs       |
| **Audit Trail**    | None                 | UpdatedAt column         |
| **Performance**    | N/A                  | MemoryCache (30 min TTL) |
