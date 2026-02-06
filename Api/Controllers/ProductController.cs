using Application.DTOs;
using Application.Services;
using Core.Sql.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// API Controller for Product management operations.
/// Demonstrates proper separation between API layer (routing, validation)
/// and Business Logic layer (ProductService).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    /// <summary>
    /// Initializes the controller with required services.
    /// </summary>
    /// <param name="productService">Business logic service for products.</param>
    /// <param name="logger">Logger for request tracking.</param>
    public ProductController(
        IProductService productService,
        ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    // =========================================================================
    // GET api/product
    // Retrieves paginated list of products with optional filtering
    // =========================================================================

    /// <summary>
    /// Gets a paginated list of products.
    /// </summary>
    /// <param name="filter">Filter and pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of products.</returns>
    /// <response code="200">Returns the paginated product list.</response>
    /// <response code="400">If the filter parameters are invalid.</response>
    /// <response code="500">If an internal error occurs.</response>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<TPaging<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TPaging<ProductDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<TPaging<ProductDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList(
        [FromQuery] ProductFilterModel filter,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/product - Page: {Page}, Size: {Size}",
            filter.Page, filter.Size);

        // Model validation is automatic via [ApiController] attribute
        // Invalid models return 400 Bad Request automatically

        var result = await _productService.GetListAsync(filter, cancellationToken);

        // Return appropriate HTTP status based on response
        return ToActionResult(result);
    }

    // =========================================================================
    // GET api/product/{id}
    // Retrieves a single product by ID
    // =========================================================================

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product details.</returns>
    /// <response code="200">Returns the product.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="500">If an internal error occurs.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/product/{Id}", id);

        if (id <= 0)
        {
            return BadRequest(BaseResponse<ProductDto?>.ValidationError("Invalid product ID."));
        }

        var result = await _productService.GetByIdAsync(id, cancellationToken);

        return ToActionResult(result);
    }

    // =========================================================================
    // POST api/product
    // Creates a single new product
    // =========================================================================

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">Product creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created product ID.</returns>
    /// <response code="201">Product created successfully.</response>
    /// <response code="400">If validation fails or duplicate code.</response>
    /// <response code="500">If an internal error occurs.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<long>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<long>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/product - Code: {Code}", request.Code);

        var result = await _productService.CreateAsync(request, cancellationToken);

        if (result.Success)
        {
            // Return 201 Created with location header for new resource
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data },
                result);
        }

        return ToActionResult(result);
    }

    // =========================================================================
    // POST api/product/bulk
    // Creates multiple products in a single transaction
    // =========================================================================

    /// <summary>
    /// Creates multiple products in a single transaction (atomic operation).
    /// If any product fails validation, all products are rolled back.
    /// </summary>
    /// <param name="request">List of products to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bulk operation result.</returns>
    /// <response code="200">All products created successfully.</response>
    /// <response code="400">If validation fails or operation partially fails.</response>
    /// <response code="500">If an internal error occurs.</response>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(BaseResponse<BulkOperationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<BulkOperationResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<BulkOperationResult>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBulk(
        [FromBody] BulkCreateProductRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/product/bulk - Count: {Count}", request.Products.Count);

        // Additional validation beyond model attributes
        if (request.Products.Count == 0)
        {
            return BadRequest(BaseResponse<BulkOperationResult>.ValidationError(
                "At least one product is required."));
        }

        // Check for duplicate codes within the request
        var duplicateCodes = request.Products
            .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateCodes.Any())
        {
            return BadRequest(BaseResponse<BulkOperationResult>.ValidationError(
                $"Duplicate product codes in request: {string.Join(", ", duplicateCodes)}"));
        }

        var result = await _productService.CreateBatchAsync(request.Products, cancellationToken);

        return ToActionResult(result);
    }

    // =========================================================================
    // PUT api/product/{id}
    // Updates an existing product
    // =========================================================================

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product ID to update.</param>
    /// <param name="request">Updated product data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Update result.</returns>
    /// <response code="200">Product updated successfully.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="400">If validation fails.</response>
    /// <response code="500">If an internal error occurs.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/product/{Id}", id);

        if (id <= 0)
        {
            return BadRequest(BaseResponse<object>.ValidationError("Invalid product ID."));
        }

        var result = await _productService.UpdateAsync(id, request, cancellationToken);

        return ToActionResult(result);
    }

    // =========================================================================
    // DELETE api/product/{id}
    // Soft-deletes a product
    // =========================================================================

    /// <summary>
    /// Deletes a product (soft delete - sets IsActive to false).
    /// </summary>
    /// <param name="id">Product ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Delete result.</returns>
    /// <response code="200">Product deleted successfully.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="500">If an internal error occurs.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("DELETE /api/product/{Id}", id);

        if (id <= 0)
        {
            return BadRequest(BaseResponse<object>.ValidationError("Invalid product ID."));
        }

        var result = await _productService.DeleteAsync(id, cancellationToken);

        return ToActionResult(result);
    }

    // =========================================================================
    // HELPER: Convert BaseResponse to IActionResult
    // Maps ErrorCode to appropriate HTTP status codes
    // =========================================================================

    /// <summary>
    /// Converts a BaseResponse to an appropriate IActionResult.
    /// Maps response codes to HTTP status codes.
    /// </summary>
    private IActionResult ToActionResult<T>(BaseResponse<T> response)
    {
        // Always wrap response in Ok/BadRequest/NotFound for consistent JSON structure
        if (response.Success)
        {
            return Ok(response);
        }

        // Map error codes to HTTP status codes
        return response.ErrorCode switch
        {
            SqlConstants.ResponseCode_NotFound => NotFound(response),
            SqlConstants.ResponseCode_ValidationError => BadRequest(response),
            SqlConstants.ResponseCode_Duplicate => BadRequest(response),
            SqlConstants.ResponseCode_Unauthorized => Unauthorized(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}
