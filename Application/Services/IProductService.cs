using Application.DTOs;
using Core.Sql.Models;

namespace Application.Services;

/// <summary>
/// Interface defining business logic operations for Product management.
/// Abstracts the data access layer from controllers.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a paginated list of products with optional filtering.
    /// </summary>
    /// <param name="filter">Filter criteria including pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of products.</returns>
    Task<BaseResponse<TPaging<ProductDto>>> GetListAsync(
        ProductFilterModel filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single product by its ID.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product details or null if not found.</returns>
    Task<BaseResponse<ProductDto?>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">Product creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response with created product ID.</returns>
    Task<BaseResponse<long>> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple products in a single transaction.
    /// All products are inserted or none (atomic operation).
    /// </summary>
    /// <param name="requests">List of products to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bulk operation result.</returns>
    Task<BaseResponse<BulkOperationResult>> CreateBatchAsync(
        List<CreateProductRequest> requests,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product ID to update.</param>
    /// <param name="request">Updated product data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response indicating success or failure.</returns>
    Task<BaseResponse<object>> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a product (sets IsActive = false).
    /// </summary>
    /// <param name="id">Product ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response indicating success or failure.</returns>
    Task<BaseResponse<object>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
