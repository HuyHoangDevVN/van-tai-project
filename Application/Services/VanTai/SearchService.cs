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
/// Pattern: All search SPs accept nullable parameters with paging support.
/// SP Parameters typically include:
///   - p_keyword: search term
///   - p_offset, p_limit: paging
///   - p_sort_by, p_sort_desc: sorting
///   - OUT p_total_record: total count
/// </summary>
public class SearchService : BaseService, ISearchService
{
    // Default search limits
    private const int DefaultOffset = 0;
    private const int DefaultLimit = 1000;

    public SearchService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<SearchService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// proc_tim_kiem_xe parameters:
    ///   IN p_keyword, p_status, p_hang_san_xuat
    ///   IN p_offset, p_limit, p_sort_by, p_sort_desc
    ///   OUT p_total_record
    /// </remarks>
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
                    // Search filters (nullable)
                    SqlParamModel.Input("p_keyword", NullIfEmpty(request.Keyword)),
                    SqlParamModel.Input("p_status", NullIfEmpty(request.Status)),
                    SqlParamModel.Input("p_hang_san_xuat", NullIfEmpty(request.HangSanXuat)),
                    // Paging parameters
                    SqlParamModel.Input("p_offset", DefaultOffset),
                    SqlParamModel.Input("p_limit", DefaultLimit),
                    SqlParamModel.Input("p_sort_by", null),
                    SqlParamModel.Input("p_sort_desc", 0),
                    // Output
                    SqlParamModel.Output("p_total_record", MySqlConnector.MySqlDbType.Int64)
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
    /// <remarks>
    /// proc_tim_kiem_tai_xe parameters:
    ///   IN p_keyword, p_gioi_tinh, p_que_quan
    ///   IN p_offset, p_limit, p_sort_by, p_sort_desc
    ///   OUT p_total_record
    /// </remarks>
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
                    // Search filters (nullable)
                    SqlParamModel.Input("p_keyword", NullIfEmpty(request.Keyword)),
                    SqlParamModel.Input("p_gioi_tinh", NullIfEmpty(request.GioiTinh)),
                    SqlParamModel.Input("p_que_quan", NullIfEmpty(request.QueQuan)),
                    // Paging parameters
                    SqlParamModel.Input("p_offset", DefaultOffset),
                    SqlParamModel.Input("p_limit", DefaultLimit),
                    SqlParamModel.Input("p_sort_by", null),
                    SqlParamModel.Input("p_sort_desc", 0),
                    // Output
                    SqlParamModel.Output("p_total_record", MySqlConnector.MySqlDbType.Int64)
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
    /// <remarks>
    /// proc_tim_kiem_tuyen_duong parameters:
    ///   IN p_keyword, p_diem_di, p_diem_den
    ///   IN p_offset, p_limit, p_sort_by, p_sort_desc
    ///   OUT p_total_record
    /// </remarks>
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
                    // Search filters (nullable)
                    SqlParamModel.Input("p_keyword", NullIfEmpty(request.Keyword)),
                    SqlParamModel.Input("p_diem_di", NullIfEmpty(request.DiemDi)),
                    SqlParamModel.Input("p_diem_den", NullIfEmpty(request.DiemDen)),
                    // Paging parameters
                    SqlParamModel.Input("p_offset", DefaultOffset),
                    SqlParamModel.Input("p_limit", DefaultLimit),
                    SqlParamModel.Input("p_sort_by", null),
                    SqlParamModel.Input("p_sort_desc", 0),
                    // Output
                    SqlParamModel.Output("p_total_record", MySqlConnector.MySqlDbType.Int64)
                ]
            };

            return await SqlService.ExecuteProceReturnAsync<RouteSearchResultDto>(model, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<RouteSearchResultDto>>(ex, "SearchRoutes");
        }
    }
}
