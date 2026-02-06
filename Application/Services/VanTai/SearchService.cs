using Application.DTOs.VanTai;
using Core.Sql.Config;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services.VanTai;

/// <summary>
/// Interface for Universal Search Service.
/// Provides standardized search across all entities.
/// </summary>
public interface ISearchService
{
    Task<BaseResponse<List<BusSearchResultDto>>> SearchBusesAsync(
        BusSearchRequest request, CancellationToken cancellationToken = default);

    Task<BaseResponse<List<DriverSearchResultDto>>> SearchDriversAsync(
        DriverSearchRequest request, CancellationToken cancellationToken = default);

    Task<BaseResponse<List<RouteSearchResultDto>>> SearchRoutesAsync(
        RouteSearchRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Universal Search Service using Dynamic SP Resolution.
/// 
/// Pattern: All search SPs accept nullable parameters.
/// WHERE (@keyword IS NULL OR name LIKE CONCAT('%', @keyword, '%'))
/// </summary>
public class SearchService : BaseService, ISearchService
{
    public SearchService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<SearchService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<List<BusSearchResultDto>>> SearchBusesAsync(
        BusSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Searching buses - Keyword: {Keyword}, Status: {Status}",
            request.Keyword, request.Status);

        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Bus.SEARCH);

            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    // Nullable parameters for flexible search
                    SqlParamModel.Input("p_keyword", NullIfEmpty(request.Keyword)),
                    SqlParamModel.Input("p_status", NullIfEmpty(request.Status)),
                    SqlParamModel.Input("p_hang_san_xuat", NullIfEmpty(request.HangSanXuat))
                ]
            };

            return await SqlService.ExecuteProceReturnAsync<BusSearchResultDto>(model, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<BusSearchResultDto>>(ex, "SearchBuses");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<List<DriverSearchResultDto>>> SearchDriversAsync(
        DriverSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Searching drivers - Keyword: {Keyword}", request.Keyword);

        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Driver.SEARCH);

            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_keyword", NullIfEmpty(request.Keyword)),
                    SqlParamModel.Input("p_gioi_tinh", NullIfEmpty(request.GioiTinh)),
                    SqlParamModel.Input("p_que_quan", NullIfEmpty(request.QueQuan))
                ]
            };

            return await SqlService.ExecuteProceReturnAsync<DriverSearchResultDto>(model, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<DriverSearchResultDto>>(ex, "SearchDrivers");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<List<RouteSearchResultDto>>> SearchRoutesAsync(
        RouteSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Searching routes - Keyword: {Keyword}", request.Keyword);

        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Route.SEARCH);

            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_keyword", NullIfEmpty(request.Keyword)),
                    SqlParamModel.Input("p_diem_di", NullIfEmpty(request.DiemDi)),
                    SqlParamModel.Input("p_diem_den", NullIfEmpty(request.DiemDen))
                ]
            };

            return await SqlService.ExecuteProceReturnAsync<RouteSearchResultDto>(model, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<RouteSearchResultDto>>(ex, "SearchRoutes");
        }
    }

    /// <summary>
    /// Helper to convert empty string to null for nullable SP parameters.
    /// </summary>
    private static object? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
