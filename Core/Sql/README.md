# SQL Execution Service - Implementation Guide

## 📁 Project Structure

```
Core/
└── Sql/
    ├── Core.Sql.csproj                    # Project file with dependencies
    ├── ServiceCollectionExtensions.cs     # DI registration
    ├── Models/
    │   ├── SqlParamModel.cs               # SQL parameter definition
    │   ├── SqlParamBatchModel.cs          # Batch parameter collection
    │   ├── SqlExecuteModel.cs             # Single execution model
    │   ├── SqlExecuteBatchModel.cs        # Batch execution model
    │   ├── SqlConstants.cs                # Configuration constants
    │   ├── CustomDataSetAttribute.cs      # Column mapping attribute
    │   ├── BaseResponse.cs                # Standard response wrapper
    │   └── TPaging.cs                     # Paging result container
    ├── Extensions/
    │   ├── TypeConversionHelper.cs        # Type conversion utilities
    │   └── DataTableExtensions.cs         # DataTable to object mapping
    ├── Services/
    │   ├── ISqlExecuteService.cs          # Service interface
    │   └── SqlExecuteService.cs           # Implementation
    └── Examples/
        └── UsageExamples.cs               # Usage demonstrations
```

## 🚀 Quick Start

### 1. Add Package References

The `Core.Sql.csproj` includes:

- `MySqlConnector` - Pure ADO.NET wrapper for MySQL
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`

### 2. Configure Connection String

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mydb;User=root;Password=secret;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;"
  }
}
```

### 3. Register Service

In `Program.cs`:

```csharp
builder.Services.AddSqlExecuteService();
```

### 4. Inject and Use

```csharp
public class UserService
{
    private readonly ISqlExecuteService _sqlService;

    public UserService(ISqlExecuteService sqlService)
    {
        _sqlService = sqlService;
    }

    public async Task<BaseResponse<List<User>>> GetUsersAsync()
    {
        var model = SqlExecuteModel.StoredProcedure("sp_GetUsers");
        return await _sqlService.ExecuteProceReturnAsync<User>(model);
    }
}
```

## 📖 API Reference

### Core Methods

| Method                            | Description                              | Use Case                          |
| --------------------------------- | ---------------------------------------- | --------------------------------- |
| `ExecuteProcReturnPagingAsync<T>` | Execute SP with pagination               | List pages with total count       |
| `ExecuteProceReturnAsync<T>`      | Execute SP, return list                  | Simple SELECT operations          |
| `ExecuteProcReturnSingleAsync<T>` | Execute SP, return single item           | Get by ID operations              |
| `ProcExecuteNonQueryAsync`        | Execute SP for CUD operations            | INSERT/UPDATE/DELETE              |
| `ProcExecuteBatchAsync`           | Execute SP multiple times in transaction | Bulk operations                   |
| `ExecuteFunctionReturnAsync<T>`   | Execute MySQL function                   | Scalar function calls             |
| `ExecuteSqlRawCommandAsync`       | Execute raw SQL                          | Ad-hoc queries (use with caution) |
| `ProcExecuteBackgroundAsync`      | Fire-and-forget execution                | Logging, notifications            |

### Parameter Types

```csharp
// Input parameter
SqlParamModel.Input("p_UserId", 123)

// Output parameter
SqlParamModel.Output("p_TotalRecord", MySqlDbType.Int64)

// Input/Output parameter
SqlParamModel.InputOutput("p_Counter", 10, MySqlDbType.Int32)
```

### Response Handling

```csharp
var result = await _sqlService.ExecuteProceReturnAsync<User>(model);

if (result.Success)
{
    var users = result.Data; // List<User>
    // Handle success
}
else
{
    var message = result.Message; // User-friendly error
    var code = result.ErrorCode;  // Error classification
    // Handle error
}
```

## 🔧 Column Mapping

### Using CustomDataSet Attribute

```csharp
public class User
{
    [CustomDataSet("user_id")]
    public int UserId { get; set; }

    [CustomDataSet("full_name")]
    public string FullName { get; set; }

    // Auto-mapped: Email -> email or Email
    public string Email { get; set; }

    // Auto-mapped: CreatedAt -> created_at (snake_case)
    public DateTime CreatedAt { get; set; }
}
```

### Mapping Priority

1. **[CustomDataSet("column_name")]** - Explicit column mapping
2. **Property name** - Case-insensitive match
3. **Snake_case conversion** - `UserId` → `user_id`

## 🔒 Security Features

### SQL Injection Prevention

- All parameters are passed via `MySqlParameter`
- Never concatenate user input into SQL strings
- Raw SQL methods still require parameterization

### Error Message Sanitization

- Raw database exceptions are logged but not exposed
- User-facing messages are generic and safe
- Debug info only available in Development environment

### Connection Management

- `using` statements ensure proper disposal
- Connections return to pool immediately after use
- No connection leaks possible

## 📊 Transaction Support

### Batch Operations

```csharp
var batch = SqlExecuteBatchModel.StoredProcedure("sp_InsertRecord");

foreach (var item in items)
{
    batch.AddBatchItem(
        SqlParamModel.Input("p_Name", item.Name),
        SqlParamModel.Input("p_Value", item.Value)
    );
}

var result = await _sqlService.ProcExecuteBatchAsync(batch);

if (result.Data?.AllSucceeded == true)
{
    // All items inserted successfully
}
```

**Transaction Behavior:**

- All operations within same transaction
- Automatic rollback on any failure
- `ContinueOnError` option for partial success

## ⚡ Performance Tips

1. **Use Stored Procedures** - Pre-compiled execution plans
2. **Parameterize Everything** - Enables plan caching
3. **Limit Result Sets** - Use paging for large data
4. **Background for Fire-and-Forget** - Don't block for logging/audit
5. **Connection Pooling** - Already configured in MySqlConnector

## 🔍 Logging

The service logs:

- Errors with procedure/query name (masked for security)
- Background execution status
- Transaction rollback events

Configure logging level in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Core.Sql.Services": "Information"
    }
  }
}
```

## 📝 MySQL Stored Procedure Examples

### Paging Procedure

```sql
DELIMITER //
CREATE PROCEDURE sp_GetUsersPaged(
    IN p_SearchTerm VARCHAR(100),
    IN p_PageIndex INT,
    IN p_PageSize INT,
    OUT p_TotalRecord BIGINT
)
BEGIN
    DECLARE v_Offset INT;
    SET v_Offset = (p_PageIndex - 1) * p_PageSize;

    SELECT COUNT(*) INTO p_TotalRecord
    FROM users
    WHERE p_SearchTerm IS NULL
       OR full_name LIKE CONCAT('%', p_SearchTerm, '%');

    SELECT user_id, full_name, email, is_active, created_at
    FROM users
    WHERE p_SearchTerm IS NULL
       OR full_name LIKE CONCAT('%', p_SearchTerm, '%')
    ORDER BY created_at DESC
    LIMIT p_PageSize OFFSET v_Offset;
END //
DELIMITER ;
```

### CUD Procedure with Response

```sql
DELIMITER //
CREATE PROCEDURE sp_CreateUser(
    IN p_FullName VARCHAR(100),
    IN p_Email VARCHAR(255),
    OUT p_ResponseCode INT,
    OUT p_ResponseMessage VARCHAR(500),
    OUT p_LastInsertId BIGINT
)
BEGIN
    DECLARE EXIT HANDLER FOR 1062
    BEGIN
        SET p_ResponseCode = -4;
        SET p_ResponseMessage = 'Email already exists';
    END;

    INSERT INTO users (full_name, email, created_at)
    VALUES (p_FullName, p_Email, NOW());

    SET p_LastInsertId = LAST_INSERT_ID();
    SET p_ResponseCode = 0;
    SET p_ResponseMessage = 'User created successfully';
END //
DELIMITER ;
```

## ✅ Checklist for Production

- [ ] Connection string uses pooling with appropriate min/max sizes
- [ ] Logging configured appropriately for environment
- [ ] Error messages reviewed for security
- [ ] All stored procedures use parameterized queries
- [ ] Command timeouts set appropriately
- [ ] Transaction isolation level considered for batch operations
- [ ] Background execution has proper error handling
