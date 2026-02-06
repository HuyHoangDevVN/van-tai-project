# Product Management API

A .NET 8 Web API demonstrating the usage of the **Core.Sql** library for MySQL database operations.

## 📁 Project Structure

```
project/
├── Api/                           # Web API Layer
│   ├── Controllers/
│   │   └── ProductController.cs   # API endpoints
│   ├── Program.cs                 # Application startup & DI config
│   ├── appsettings.json           # Configuration
│   └── Api.csproj
├── Application/                   # Business Logic Layer
│   ├── DTOs/
│   │   └── ProductModels.cs       # DTOs with [CustomDataSet] mapping
│   ├── Services/
│   │   ├── IProductService.cs     # Service interface
│   │   └── ProductService.cs      # Service implementation
│   └── Application.csproj
├── Core/                          # Data Access Layer
│   └── Sql/                       # Core.Sql library
│       └── ...
├── Database/
│   └── Scripts/
│       └── 001_Create_Products_Schema.sql
└── ProductManagement.sln
```

## 🚀 Quick Start

### 1. Setup Database

```bash
# Connect to MySQL and run the schema script
mysql -u root -p < Database/Scripts/001_Create_Products_Schema.sql
```

### 2. Configure Connection String

Update `Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=product_management;User=root;Password=YOUR_PASSWORD;..."
  }
}
```

### 3. Run the Application

```bash
cd Api
dotnet run
```

### 4. Access Swagger UI

Open: `https://localhost:5001` (or the URL shown in console)

## 📖 API Endpoints

| Method | Endpoint            | Description                      |
| ------ | ------------------- | -------------------------------- |
| GET    | `/api/product`      | Get paginated product list       |
| GET    | `/api/product/{id}` | Get product by ID                |
| POST   | `/api/product`      | Create single product            |
| POST   | `/api/product/bulk` | Create multiple products (batch) |
| PUT    | `/api/product/{id}` | Update product                   |
| DELETE | `/api/product/{id}` | Soft delete product              |

## 📝 Example Requests

### Get Products with Pagination

```http
GET /api/product?page=1&size=10&keyword=wireless&categoryId=1&isActive=true
```

**Response:**

```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": {
    "items": [
      {
        "id": 1,
        "code": "ELEC-001",
        "name": "Wireless Bluetooth Headphones",
        "price": 79.99,
        "categoryId": 1,
        "categoryName": "Electronics",
        "isActive": true,
        "createdDate": "2026-02-06T10:00:00"
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalRecords": 1,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "errorCode": 0
}
```

### Create Single Product

```http
POST /api/product
Content-Type: application/json

{
  "code": "NEW-001",
  "name": "New Product",
  "price": 29.99,
  "categoryId": 1,
  "description": "A brand new product",
  "stockQuantity": 100
}
```

**Response:**

```json
{
  "success": true,
  "message": "Product created successfully",
  "data": 10,
  "errorCode": 0
}
```

### Bulk Create Products

```http
POST /api/product/bulk
Content-Type: application/json

{
  "products": [
    {
      "code": "BULK-001",
      "name": "Bulk Product 1",
      "price": 19.99,
      "categoryId": 1,
      "stockQuantity": 50
    },
    {
      "code": "BULK-002",
      "name": "Bulk Product 2",
      "price": 24.99,
      "categoryId": 2,
      "stockQuantity": 75
    }
  ]
}
```

**Response:**

```json
{
  "success": true,
  "message": "Successfully created 2 products.",
  "data": {
    "totalItems": 2,
    "successCount": 2,
    "failedCount": 0,
    "allSucceeded": true
  },
  "errorCode": 0
}
```

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     ProductController                           │
│         (Routing, Validation, HTTP Response Mapping)            │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     IProductService                              │
│              (Business Logic, Data Transformation)               │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ISqlExecuteService                            │
│    (Core.Sql Library - DB Operations, Mapping, Transactions)     │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     MySQL Database                               │
│              (Stored Procedures, Tables)                         │
└─────────────────────────────────────────────────────────────────┘
```

## 🔧 Key Implementation Details

### 1. Column Mapping with [CustomDataSet]

```csharp
public class ProductDto
{
    [CustomDataSet("product_code")]  // Maps DB column to C# property
    public string Code { get; set; }

    [CustomDataSet("product_name")]
    public string Name { get; set; }
}
```

### 2. Paging with ExecuteProcReturnPagingAsync

```csharp
var sqlModel = new SqlExecuteModel("sp_Product_GetPaging")
{
    Params = [
        SqlParamModel.Input("p_Keyword", filter.Keyword),
        SqlParamModel.Input("p_CategoryId", filter.CategoryId),
    ]
};

// Service auto-adds: p_PageIndex, p_PageSize, p_TotalRecord (output)
var result = await _sqlService.ExecuteProcReturnPagingAsync<ProductDto>(
    sqlModel, filter.Page, filter.Size);
```

### 3. CUD with Output Parameters

```csharp
var sqlModel = new SqlExecuteModel("sp_Product_Create")
{
    Params = [
        SqlParamModel.Input("p_ProductCode", request.Code),
        SqlParamModel.Output(SqlConstants.P_ResponseCode, MySqlDbType.Int32),
        SqlParamModel.Output(SqlConstants.P_LastInsertId, MySqlDbType.Int64),
    ]
};

var result = await _sqlService.ProcExecuteNonQueryAsync(sqlModel);
```

### 4. Batch Insert with Transaction

```csharp
var batchModel = new SqlExecuteBatchModel("sp_Product_Create")
{
    ContinueOnError = false  // Rollback all on first failure
};

foreach (var product in products)
{
    batchModel.AddBatchItem(
        SqlParamModel.Input("p_ProductCode", product.Code),
        SqlParamModel.Input("p_ProductName", product.Name)
    );
}

// Executes all inserts in single transaction
var result = await _sqlService.ProcExecuteBatchAsync(batchModel);
```

## 🔒 Error Handling

All responses use consistent `BaseResponse<T>` wrapper:

```json
{
  "success": false,
  "message": "Product code already exists",
  "data": null,
  "errorCode": -4,
  "timestamp": "2026-02-06T10:00:00Z"
}
```

| Error Code | Meaning          |
| ---------- | ---------------- |
| 0          | Success          |
| -1         | General error    |
| -2         | Validation error |
| -3         | Not found        |
| -4         | Duplicate entry  |
| -5         | Unauthorized     |

## 📊 Dependency Injection

```csharp
// Program.cs
builder.Services.AddSqlExecuteService();                    // Core.Sql (Scoped)
builder.Services.AddScoped<IProductService, ProductService>(); // Application (Scoped)
```

Both services are **Scoped** - one instance per HTTP request, ensuring:

- Proper connection pooling
- Request-isolated transactions
- Consistent state within a request
