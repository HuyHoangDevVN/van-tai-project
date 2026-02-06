using Application.DTOs.VanTai;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services.VanTai;

/// <summary>
/// Service implementation for Report operations.
/// Handles all stored procedure-based reports from the transport management system.
/// </summary>
public class BaoCaoService : IBaoCaoService
{
    private readonly ISqlExecuteService _sqlService;
    private readonly ILogger<BaoCaoService> _logger;

    public BaoCaoService(
        ISqlExecuteService sqlService,
        ILogger<BaoCaoService> logger)
    {
        _sqlService = sqlService;
        _logger = logger;
    }

    // =========================================================================
    // proc_chi_phi_co_ban - Get Basic Cost for All Trips
    // This procedure has NO parameters, returns list of trip costs
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<ChiPhiCoBanDto>>> GetChiPhiCoBanAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing proc_chi_phi_co_ban");

        try
        {
            // Build SqlExecuteModel for stored procedure with NO parameters
            // proc_chi_phi_co_ban() returns:
            //   ma_chuyen, ten_chuyen, ma_xe, khoang_cach, chi_phi_co_ban
            var sqlModel = new SqlExecuteModel("proc_chi_phi_co_ban")
            {
                IsStoredProcedure = true
                // No parameters for this procedure
            };

            // Execute stored procedure and map results to ChiPhiCoBanDto
            // Using ExecuteProceReturnAsync<T> for list results
            var result = await _sqlService.ExecuteProceReturnAsync<ChiPhiCoBanDto>(
                sqlModel,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "proc_chi_phi_co_ban returned {Count} records",
                    result.Data?.Count ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing proc_chi_phi_co_ban");
            return BaseResponse<List<ChiPhiCoBanDto>>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }

    // =========================================================================
    // proc_doanh_thu_xe_bus_ngoi_thang - Get Sitting Bus Revenue by Month
    // Parameters: p_tu (DATETIME), p_den (DATETIME)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<DoanhThuXeBusNgoiThangDto>>> GetDoanhThuXeBusNgoiThangAsync(
        DateRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Executing proc_doanh_thu_xe_bus_ngoi_thang - From: {From}, To: {To}",
            request.TuNgay, request.DenNgay);

        try
        {
            // Build SqlExecuteModel with exact parameter names from stored procedure
            // CRITICAL: Parameter names MUST match exactly: p_tu, p_den
            var sqlModel = new SqlExecuteModel("proc_doanh_thu_xe_bus_ngoi_thang")
            {
                IsStoredProcedure = true,
                Params =
                [
                    // p_tu IN DATETIME - Start date
                    SqlParamModel.Input("p_tu", request.TuNgay),
                    
                    // p_den IN DATETIME - End date
                    SqlParamModel.Input("p_den", request.DenNgay)
                ]
            };

            // Execute and map to DTO
            // Procedure returns: ma_xe, thang, doanh_thu_thang
            var result = await _sqlService.ExecuteProceReturnAsync<DoanhThuXeBusNgoiThangDto>(
                sqlModel,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "proc_doanh_thu_xe_bus_ngoi_thang returned {Count} records",
                    result.Data?.Count ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing proc_doanh_thu_xe_bus_ngoi_thang");
            return BaseResponse<List<DoanhThuXeBusNgoiThangDto>>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }

    // =========================================================================
    // proc_gia_ve_xe_bus_ngoi - Get Sitting Bus Ticket Prices
    // This procedure has NO parameters
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<GiaVeXeBusNgoiDto>>> GetGiaVeXeBusNgoiAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing proc_gia_ve_xe_bus_ngoi");

        try
        {
            // No parameters for this procedure
            var sqlModel = new SqlExecuteModel("proc_gia_ve_xe_bus_ngoi")
            {
                IsStoredProcedure = true
            };

            // Procedure returns: stt, ma_chuyen, ten_chuyen, vi_tri, ma_xe, 
            //                    chi_phi_co_ban, khoang_cach, gia_ve
            var result = await _sqlService.ExecuteProceReturnAsync<GiaVeXeBusNgoiDto>(
                sqlModel,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "proc_gia_ve_xe_bus_ngoi returned {Count} records",
                    result.Data?.Count ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing proc_gia_ve_xe_bus_ngoi");
            return BaseResponse<List<GiaVeXeBusNgoiDto>>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }

    // =========================================================================
    // proc_tinh_doanh_thu_tuyen_duong_theo_thang - Get Route Revenue by Month
    // Parameters: p_tu (DATETIME), p_den (DATETIME)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<DoanhThuTuyenDuongThangDto>>> GetDoanhThuTuyenDuongThangAsync(
        DateRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Executing proc_tinh_doanh_thu_tuyen_duong_theo_thang - From: {From}, To: {To}",
            request.TuNgay, request.DenNgay);

        try
        {
            // Parameter names MUST match: p_tu, p_den
            var sqlModel = new SqlExecuteModel("proc_tinh_doanh_thu_tuyen_duong_theo_thang")
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_tu", request.TuNgay),
                    SqlParamModel.Input("p_den", request.DenNgay)
                ]
            };

            // Procedure returns: ma_tuyen, ten_tuyen, thang, doanh_thu_thang
            var result = await _sqlService.ExecuteProceReturnAsync<DoanhThuTuyenDuongThangDto>(
                sqlModel,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "proc_tinh_doanh_thu_tuyen_duong_theo_thang returned {Count} records",
                    result.Data?.Count ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing proc_tinh_doanh_thu_tuyen_duong_theo_thang");
            return BaseResponse<List<DoanhThuTuyenDuongThangDto>>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }

    // =========================================================================
    // proc_tinh_luong_thang - Calculate Monthly Driver Salary
    // Parameters: p_tu (DATETIME), p_den (DATETIME)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<LuongThangTaiXeDto>>> GetLuongThangTaiXeAsync(
        DateRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Executing proc_tinh_luong_thang - From: {From}, To: {To}",
            request.TuNgay, request.DenNgay);

        try
        {
            // Parameter names MUST match exactly with stored procedure: p_tu, p_den
            var sqlModel = new SqlExecuteModel("proc_tinh_luong_thang")
            {
                IsStoredProcedure = true,
                Params =
                [
                    // IN p_tu DATETIME
                    SqlParamModel.Input("p_tu", request.TuNgay),
                    
                    // IN p_den DATETIME
                    SqlParamModel.Input("p_den", request.DenNgay)
                ]
            };

            // Procedure returns: ma_tai_xe, ten_tai_xe, tong_km, so_tuyen, luong_thang
            var result = await _sqlService.ExecuteProceReturnAsync<LuongThangTaiXeDto>(
                sqlModel,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "proc_tinh_luong_thang returned {Count} driver salary records",
                    result.Data?.Count ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing proc_tinh_luong_thang");
            return BaseResponse<List<LuongThangTaiXeDto>>.Error(
                SqlConstants.Message_Error,
                SqlConstants.ResponseCode_Error);
        }
    }
}
