using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Core.Sql.Models;

namespace Application.DTOs;

// =============================================================================
// PRODUCT DTO - Maps database columns to C# properties
// Uses [CustomDataSet] attribute for explicit column mapping
// =============================================================================

/// <summary>
/// Data Transfer Object for Product entity.
/// Maps MySQL column names (snake_case) to C# properties (PascalCase).
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Product unique identifier.
    /// Maps to: products.id
    /// </summary>
    [CustomDataSet("id")]
    public int Id { get; set; }

    /// <summary>
    /// Product code (unique business identifier).
    /// Maps to: products.product_code
    /// </summary>
    [CustomDataSet("product_code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Product name/title.
    /// Maps to: products.product_name
    /// </summary>
    [CustomDataSet("product_name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product price.
    /// Maps to: products.price
    /// </summary>
    [CustomDataSet("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Foreign key to categories table.
    /// Maps to: products.category_id
    /// </summary>
    [CustomDataSet("category_id")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// Category name (joined from categories table).
    /// Maps to: categories.category_name
    /// </summary>
    [CustomDataSet("category_name")]
    public string? CategoryName { get; set; }

    /// <summary>
    /// Product description.
    /// Maps to: products.description
    /// </summary>
    [CustomDataSet("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Current stock quantity.
    /// Maps to: products.stock_quantity
    /// </summary>
    [CustomDataSet("stock_quantity")]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Whether the product is active.
    /// Maps to: products.is_active (MySQL TINYINT(1) -> C# bool)
    /// </summary>
    [CustomDataSet("is_active")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// Maps to: products.created_date
    /// </summary>
    [CustomDataSet("created_date")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Last update timestamp.
    /// Maps to: products.updated_date
    /// </summary>
    [CustomDataSet("updated_date")]
    public DateTime? UpdatedDate { get; set; }
}

// =============================================================================
// REQUEST MODELS - For API input validation
// =============================================================================

/// <summary>
/// Request model for creating a new product.
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// Product code (unique identifier). Required, max 50 characters.
    /// </summary>
    [Required(ErrorMessage = "Product code is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Product code must be between 2 and 50 characters")]
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Product name. Required, max 200 characters.
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product price. Must be non-negative.
    /// </summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Category ID. Optional.
    /// </summary>
    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// Product description. Optional, max 2000 characters.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Initial stock quantity. Defaults to 0.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    [JsonPropertyName("stockQuantity")]
    public int StockQuantity { get; set; } = 0;
}

/// <summary>
/// Request model for updating an existing product.
/// </summary>
public class UpdateProductRequest
{
    /// <summary>
    /// Product code. Optional for updates.
    /// </summary>
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Product code must be between 2 and 50 characters")]
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Product name. Optional for updates.
    /// </summary>
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Product price. Optional for updates.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Category ID. Optional for updates. Use 0 to clear category.
    /// </summary>
    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// Product description. Optional for updates.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Stock quantity. Optional for updates.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    [JsonPropertyName("stockQuantity")]
    public int? StockQuantity { get; set; }

    /// <summary>
    /// Active status. Optional for updates.
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}

// =============================================================================
// FILTER MODELS - For query parameters
// =============================================================================

/// <summary>
/// Filter model for product listing with pagination.
/// </summary>
public class ProductFilterModel
{
    /// <summary>
    /// Search keyword (searches in product code and name).
    /// </summary>
    [JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    /// <summary>
    /// Filter by category ID.
    /// </summary>
    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// Filter by active status. Null = all, true = active only, false = inactive only.
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    /// <summary>
    /// Page index (1-based). Defaults to 1.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size. Defaults to 20, max 100.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Size must be between 1 and 100")]
    [JsonPropertyName("size")]
    public int Size { get; set; } = 20;
}

// =============================================================================
// BATCH OPERATION MODELS
// =============================================================================

/// <summary>
/// Request model for bulk creating products.
/// </summary>
public class BulkCreateProductRequest
{
    /// <summary>
    /// List of products to create.
    /// </summary>
    [Required(ErrorMessage = "Products list is required")]
    [MinLength(1, ErrorMessage = "At least one product is required")]
    [MaxLength(100, ErrorMessage = "Cannot create more than 100 products at once")]
    [JsonPropertyName("products")]
    public List<CreateProductRequest> Products { get; set; } = [];
}

/// <summary>
/// Response model for bulk operations showing individual results.
/// </summary>
public class BulkOperationResult
{
    /// <summary>
    /// Total number of items processed.
    /// </summary>
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of successful operations.
    /// </summary>
    [JsonPropertyName("successCount")]
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed operations.
    /// </summary>
    [JsonPropertyName("failedCount")]
    public int FailedCount { get; set; }

    /// <summary>
    /// Whether all operations succeeded.
    /// </summary>
    [JsonPropertyName("allSucceeded")]
    public bool AllSucceeded => FailedCount == 0;

    /// <summary>
    /// Error message if any operation failed.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}
