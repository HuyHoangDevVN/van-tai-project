using Application.DTOs;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Application.Services;

/// <summary>
/// Business logic implementation for Product management.
/// Demonstrates proper usage of the ISqlExecuteService for various scenarios:
/// - Paged queries with filtering
/// - Single record retrieval
/// - CUD operations with output parameters
/// - Batch/transactional inserts
/// </summary>
public class ProductService : IProductService
{
    private readonly ISqlExecuteService _sqlService;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes ProductService with required dependencies.
    /// </summary>
    /// <param name="sqlService">SQL execution service from Core.Sql library.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    public ProductService(
        ISqlExecuteService sqlService,
        ILogger<ProductService> logger)
    {
        _sqlService = sqlService;
        _logger = logger;
    }

    // =========================================================================
    // GET PAGINATED LIST
    // Demonstrates: ExecuteProcReturnPagingAsync with multiple filter parameters
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<TPaging<ProductDto>>> GetListAsync(
        ProductFilterModel filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting products list - Page: {Page}, Size: {Size}, Keyword: {Keyword}",
            filter.Page, filter.Size, filter.Keyword ?? "(none)");

        try
        {
            // Build the SQL execution model for the stored procedure
            // The procedure sp_Product_GetPaging handles:
            //   - Filtering by keyword (product_code, product_name)
            //   - Filtering by category
            //   - Filtering by active status
            //   - Pagination with offset/limit
            //   - Output parameter for total record count
            var sqlModel = new SqlExecuteModel("sp_Product_GetPaging")
            {
                Params =
                [
                    // Input parameters for filtering
                    SqlParamModel.Input("p_Keyword",
                        string.IsNullOrWhiteSpace(filter.Keyword) ? null : filter.Keyword),

                    SqlParamModel.Input("p_CategoryId",
                        filter.CategoryId ?? 0),  // 0 = no filter
                    
                    SqlParamModel.Input("p_IsActive",
                        filter.IsActive.HasValue ? (filter.IsActive.Value ? 1 : 0) : (object?)null),
                ]
            };

            // Execute the paging stored procedure
            // - The service automatically adds p_PageIndex, p_PageSize parameters
            // - It also adds p_TotalRecord output parameter (uses SqlConstants.P_TotalRecord)
            // - Maps result set to ProductDto using [CustomDataSet] attributes
            var result = await _sqlService.ExecuteProcReturnPagingAsync<ProductDto>(
                sqlModel,
                filter.Page,
                filter.Size,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Retrieved {Count} products (Total: {Total})",
                    result.Data?.Items.Count() ?? 0,
                    result.Data?.TotalRecords ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products list");
            return BaseResponse<TPaging<ProductDto>>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }

    // =========================================================================
    // GET BY ID
    // Demonstrates: ExecuteProcReturnSingleAsync for single record retrieval
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<ProductDto?>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product by ID: {Id}", id);

        try
        {
            // Simple stored procedure call with single input parameter
            var sqlModel = SqlExecuteModel.StoredProcedure("sp_Product_GetById")
                .AddInput("p_Id", id);

            // ExecuteProcReturnSingleAsync returns the first row mapped to ProductDto
            // Returns null if no matching record found
            var result = await _sqlService.ExecuteProcReturnSingleAsync<ProductDto>(
                sqlModel,
                cancellationToken);

            if (result.Success && result.Data == null)
            {
                return BaseResponse<ProductDto?>.NotFound($"Product with ID {id} not found.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {Id}", id);
            return BaseResponse<ProductDto?>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }

    // =========================================================================
    // CREATE SINGLE
    // Demonstrates: ProcExecuteNonQueryAsync with output parameters
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<long>> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating product: {Code} - {Name}", request.Code, request.Name);

        try
        {
            // Build execution model with both input and output parameters
            // The stored procedure sp_Product_Create returns:
            //   - p_ResponseCode: 0 for success, negative for errors
            //   - p_ResponseMessage: Human-readable result message
            //   - p_LastInsertId: The auto-generated ID of the new product
            var sqlModel = new SqlExecuteModel("sp_Product_Create")
            {
                Params =
                [
                    // Input parameters
                    SqlParamModel.Input("p_ProductCode", request.Code),
                    SqlParamModel.Input("p_ProductName", request.Name),
                    SqlParamModel.Input("p_Price", request.Price),
                    SqlParamModel.Input("p_CategoryId", request.CategoryId ?? 0),
                    SqlParamModel.Input("p_Description", request.Description),
                    SqlParamModel.Input("p_StockQuantity", request.StockQuantity),
                    
                    // Output parameters - using SqlConstants for consistent naming
                    SqlParamModel.Output(SqlConstants.P_ResponseCode, MySqlDbType.Int32),
                    SqlParamModel.Output(SqlConstants.P_ResponseMessage, MySqlDbType.VarChar, 500),
                    SqlParamModel.Output(SqlConstants.P_LastInsertId, MySqlDbType.Int64),
                ]
            };

            // Execute the stored procedure
            // ProcExecuteNonQueryAsync handles CUD operations and captures output parameters
            var result = await _sqlService.ProcExecuteNonQueryAsync(sqlModel, cancellationToken);

            // Extract output parameter values from the result
            if (result.Success && result.Data is Dictionary<string, object?> outputs)
            {
                var responseCode = outputs.TryGetValue(SqlConstants.P_ResponseCode, out var code)
                    ? Convert.ToInt32(code)
                    : SqlConstants.ResponseCode_Error;

                var responseMessage = outputs.TryGetValue(SqlConstants.P_ResponseMessage, out var msg)
                    ? msg?.ToString() ?? SqlConstants.Message_Error
                    : SqlConstants.Message_Error;

                var lastInsertId = outputs.TryGetValue(SqlConstants.P_LastInsertId, out var id)
                    ? Convert.ToInt64(id)
                    : 0L;

                // Return appropriate response based on stored procedure result
                if (responseCode == SqlConstants.ResponseCode_Success)
                {
                    _logger.LogInformation("Product created successfully with ID: {Id}", lastInsertId);
                    return BaseResponse<long>.Ok(lastInsertId, responseMessage);
                }
                else
                {
                    _logger.LogWarning("Product creation failed: {Message}", responseMessage);
                    return BaseResponse<long>.Error(responseMessage, responseCode);
                }
            }

            return BaseResponse<long>.Error(result.Message, result.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {Code}", request.Code);
            return BaseResponse<long>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }

    // =========================================================================
    // CREATE BATCH (Transactional)
    // Demonstrates: ProcExecuteBatchAsync for atomic bulk operations
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<BulkOperationResult>> CreateBatchAsync(
        List<CreateProductRequest> requests,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating {Count} products in batch", requests.Count);

        if (requests.Count == 0)
        {
            return BaseResponse<BulkOperationResult>.Ok(
                new BulkOperationResult { TotalItems = 0, SuccessCount = 0 },
                "No products to create.");
        }

        try
        {
            // Build batch execution model
            // ProcExecuteBatchAsync will:
            //   1. Start a MySQL transaction
            //   2. Execute sp_Product_Create for each item in BatchParams
            //   3. Commit if all succeed, rollback if any fails
            var batchModel = new SqlExecuteBatchModel("sp_Product_Create")
            {
                IsStoredProcedure = true,
                // ContinueOnError = false means transaction rolls back on first failure
                // This ensures atomic operation - all or nothing
                ContinueOnError = false
            };

            // Add each product as a batch item with its own set of parameters
            foreach (var request in requests)
            {
                var batchItem = new SqlParamBatchModel()
                    .AddInput("p_ProductCode", request.Code)
                    .AddInput("p_ProductName", request.Name)
                    .AddInput("p_Price", request.Price)
                    .AddInput("p_CategoryId", request.CategoryId ?? 0)
                    .AddInput("p_Description", request.Description)
                    .AddInput("p_StockQuantity", request.StockQuantity);

                // Note: Output parameters are typically not used in batch mode
                // as we're focused on the bulk insert success/failure

                batchModel.AddBatchItem(batchItem);
            }

            // Execute the batch operation within a transaction
            var result = await _sqlService.ProcExecuteBatchAsync(batchModel, cancellationToken);

            // Map the batch result to our response model
            var bulkResult = new BulkOperationResult
            {
                TotalItems = requests.Count,
                SuccessCount = result.Data?.SuccessCount ?? 0,
                FailedCount = result.Data?.FailedCount ?? requests.Count,
                ErrorMessage = result.Data?.FirstErrorMessage
            };

            if (result.Success && bulkResult.AllSucceeded)
            {
                _logger.LogInformation("Batch insert completed: {Success}/{Total} products created",
                    bulkResult.SuccessCount, bulkResult.TotalItems);

                return BaseResponse<BulkOperationResult>.Ok(
                    bulkResult,
                    $"Successfully created {bulkResult.SuccessCount} products.");
            }
            else
            {
                _logger.LogWarning("Batch insert failed: {Failed}/{Total} products failed. Error: {Error}",
                    bulkResult.FailedCount, bulkResult.TotalItems, bulkResult.ErrorMessage);

                var errorResponse = BaseResponse<BulkOperationResult>.Error(
                    bulkResult.ErrorMessage ?? "Batch operation failed. All changes have been rolled back.",
                    SqlConstants.ResponseCode_Error);
                errorResponse.Data = bulkResult;
                return errorResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch product creation");
            return BaseResponse<BulkOperationResult>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }

    // =========================================================================
    // UPDATE
    // Demonstrates: ProcExecuteNonQueryAsync for update operation
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<object>> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product ID: {Id}", id);

        try
        {
            var sqlModel = new SqlExecuteModel("sp_Product_Update")
            {
                Params =
                [
                    SqlParamModel.Input("p_Id", id),
                    SqlParamModel.Input("p_ProductCode", request.Code),
                    SqlParamModel.Input("p_ProductName", request.Name),
                    SqlParamModel.Input("p_Price", request.Price),
                    SqlParamModel.Input("p_CategoryId", request.CategoryId),
                    SqlParamModel.Input("p_Description", request.Description),
                    SqlParamModel.Input("p_StockQuantity", request.StockQuantity),
                    SqlParamModel.Input("p_IsActive", request.IsActive.HasValue
                        ? (request.IsActive.Value ? 1 : 0)
                        : (object?)null),
                    SqlParamModel.Output(SqlConstants.P_ResponseCode, MySqlDbType.Int32),
                    SqlParamModel.Output(SqlConstants.P_ResponseMessage, MySqlDbType.VarChar, 500),
                ]
            };

            var result = await _sqlService.ProcExecuteNonQueryAsync(sqlModel, cancellationToken);

            // The service returns output parameters in the Data dictionary
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return BaseResponse<object>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }

    // =========================================================================
    // DELETE (Soft Delete)
    // Demonstrates: Simple CUD operation
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<object>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting (soft) product ID: {Id}", id);

        try
        {
            var sqlModel = SqlExecuteModel.StoredProcedure("sp_Product_Delete")
                .AddInput("p_Id", id)
                .AddOutput(SqlConstants.P_ResponseCode, MySqlDbType.Int32)
                .AddOutput(SqlConstants.P_ResponseMessage, MySqlDbType.VarChar, 500);

            var result = await _sqlService.ProcExecuteNonQueryAsync(sqlModel, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return BaseResponse<object>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }
}
