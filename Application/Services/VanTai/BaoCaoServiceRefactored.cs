using Application.DTOs.VanTai;
using Core.Sql.Config;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services.VanTai;

/// <summary>
/// REFACTORED Service implementation for Report operations.
/// Uses Dynamic SP Resolution pattern with IProcedureConfigProvider.
/// 
/// Architecture Flow:
/// Controller -> BaoCaoServiceRefactored -> GetProcName(FunctionKey) -> IProcedureConfigProvider (cached) -> Execute SQL
/// 
/// NOTE: This is a demonstration of the refactored approach. 
/// The original BaoCaoService is kept for backward compatibility.
/// </summary>
public class BaoCaoServiceRefactored : BaseService, IBaoCaoService
{
    public BaoCaoServiceRefactored(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<BaoCaoServiceRefactored> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    // =========================================================================
    // proc_chi_phi_co_ban - Get Basic Cost for All Trips
    // REFACTORED: Using dynamic SP resolution
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<ChiPhiCoBanDto>>> GetChiPhiCoBanAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Executing report: Chi Phi Co Ban (using dynamic SP resolution)");

        try
        {
            // 1. Dynamic SP Resolution - No hardcoded "proc_chi_phi_co_ban" !
            string spName = await GetProcNameAsync(FunctionKeys.Report.CHI_PHI_CO_BAN);

            // 2. Build model with resolved SP name
            var sqlModel = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true
            };

            // 3. Execute
            var result = await SqlService.ExecuteProceReturnAsync<ChiPhiCoBanDto>(
                sqlModel, cancellationToken);

            if (result.Success)
            {
                Logger.LogInformation(
                    "Report CHI_PHI_CO_BAN (SP: {SpName}) returned {Count} records",
                    spName, result.Data?.Count ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            return HandleException<List<ChiPhiCoBanDto>>(ex, "GetChiPhiCoBan");
        }
    }

    // =========================================================================
    // proc_doanh_thu_xe_bus_ngoi_thang - Get Sitting Bus Revenue by Month
    // REFACTORED: Using dynamic SP resolution
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<DoanhThuXeBusNgoiThangDto>>> GetDoanhThuXeBusNgoiThangAsync(
        DateRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate
        var validation = ValidateDateRange<List<DoanhThuXeBusNgoiThangDto>>(request.TuNgay, request.DenNgay);
        if (validation != null) return validation;

        Logger.LogInformation(
            "Executing report: Doanh Thu Xe Bus Ngoi Thang - From: {From}, To: {To}",
            request.TuNgay, request.DenNgay);

        try
        {
            // 1. Dynamic SP Resolution
            string spName = await GetProcNameAsync(FunctionKeys.Report.DOANH_THU_XE_BUS_NGOI_THANG);

            // 2. Build model
            var sqlModel = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_tu", request.TuNgay),
                    SqlParamModel.Input("p_den", request.DenNgay)
                ]
            };

            // 3. Execute
            var result = await SqlService.ExecuteProceReturnAsync<DoanhThuXeBusNgoiThangDto>(
                sqlModel, cancellationToken);

            if (result.Success)
            {
                Logger.LogInformation(
                    "Report DOANH_THU (SP: {SpName}) returned {Count} records",
                    spName, result.Data?.Count ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            return HandleException<List<DoanhThuXeBusNgoiThangDto>>(ex, "GetDoanhThuXeBusNgoiThang");
        }
    }

    // =========================================================================
    // proc_gia_ve_xe_bus_ngoi - Get Sitting Bus Ticket Prices
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<GiaVeXeBusNgoiDto>>> GetGiaVeXeBusNgoiAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Executing report: Gia Ve Xe Bus Ngoi");

        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Report.GIA_VE_XE_BUS_NGOI);

            var sqlModel = new SqlExecuteModel(spName) { IsStoredProcedure = true };

            return await SqlService.ExecuteProceReturnAsync<GiaVeXeBusNgoiDto>(
                sqlModel, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<GiaVeXeBusNgoiDto>>(ex, "GetGiaVeXeBusNgoi");
        }
    }

    // =========================================================================
    // proc_liet_ke_tuyen_duong_xe_giuong_nam - List Sleeper Bus Routes
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<TuyenDuongXeGiuongNamDto>>> GetTuyenDuongXeGiuongNamAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Report.TUYEN_DUONG_XE_GIUONG_NAM);

            var sqlModel = new SqlExecuteModel(spName) { IsStoredProcedure = true };

            return await SqlService.ExecuteProceReturnAsync<TuyenDuongXeGiuongNamDto>(
                sqlModel, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<TuyenDuongXeGiuongNamDto>>(ex, "GetTuyenDuongXeGiuongNam");
        }
    }

    // =========================================================================
    // proc_liet_ke_khach_hang_quay_lai - List Returning Customers
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<KhachHangQuayLaiDto>>> GetKhachHangQuayLaiAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Report.KHACH_HANG_QUAY_LAI);

            var sqlModel = new SqlExecuteModel(spName) { IsStoredProcedure = true };

            return await SqlService.ExecuteProceReturnAsync<KhachHangQuayLaiDto>(
                sqlModel, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<KhachHangQuayLaiDto>>(ex, "GetKhachHangQuayLai");
        }
    }

    // =========================================================================
    // proc_tinh_doanh_thu_tuyen_duong_theo_thang - Route Revenue by Month
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<DoanhThuTuyenDuongThangDto>>> GetDoanhThuTuyenDuongThangAsync(
        DateRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Report.DOANH_THU_TUYEN_DUONG_THANG);

            var sqlModel = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_tu", request.TuNgay),
                    SqlParamModel.Input("p_den", request.DenNgay)
                ]
            };

            return await SqlService.ExecuteProceReturnAsync<DoanhThuTuyenDuongThangDto>(
                sqlModel, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<DoanhThuTuyenDuongThangDto>>(ex, "GetDoanhThuTuyenDuongThang");
        }
    }

    // =========================================================================
    // proc_tinh_luong_thang - Driver Monthly Salary
    // =========================================================================

    /// <inheritdoc/>
    public async Task<BaseResponse<List<LuongThangTaiXeDto>>> GetLuongThangTaiXeAsync(
        DateRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Report.LUONG_THANG_TAI_XE);

            var sqlModel = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_tu", request.TuNgay),
                    SqlParamModel.Input("p_den", request.DenNgay)
                ]
            };

            return await SqlService.ExecuteProceReturnAsync<LuongThangTaiXeDto>(
                sqlModel, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<LuongThangTaiXeDto>>(ex, "GetLuongThangTaiXe");
        }
    }
}
