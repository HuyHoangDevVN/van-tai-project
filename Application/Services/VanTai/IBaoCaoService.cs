using Application.DTOs.VanTai;
using Core.Sql.Models;

namespace Application.Services.VanTai;

/// <summary>
/// Interface for Report Service - handles stored procedure-based reports.
/// Các báo cáo từ stored procedures.
/// </summary>
public interface IBaoCaoService
{
    /// <summary>
    /// Lấy chi phí cơ bản của tất cả chuyến xe.
    /// Procedure: proc_chi_phi_co_ban (no parameters)
    /// </summary>
    Task<BaseResponse<List<ChiPhiCoBanDto>>> GetChiPhiCoBanAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy doanh thu xe bus ngồi theo tháng.
    /// Procedure: proc_doanh_thu_xe_bus_ngoi_thang (p_tu, p_den)
    /// </summary>
    Task<BaseResponse<List<DoanhThuXeBusNgoiThangDto>>> GetDoanhThuXeBusNgoiThangAsync(
        DateRangeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy giá vé xe bus ngồi.
    /// Procedure: proc_gia_ve_xe_bus_ngoi (no parameters)
    /// </summary>
    Task<BaseResponse<List<GiaVeXeBusNgoiDto>>> GetGiaVeXeBusNgoiAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy doanh thu tuyến đường theo tháng.
    /// Procedure: proc_tinh_doanh_thu_tuyen_duong_theo_thang (p_tu, p_den)
    /// </summary>
    Task<BaseResponse<List<DoanhThuTuyenDuongThangDto>>> GetDoanhThuTuyenDuongThangAsync(
        DateRangeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tính lương tháng tài xế.
    /// Procedure: proc_tinh_luong_thang (p_tu, p_den)
    /// </summary>
    Task<BaseResponse<List<LuongThangTaiXeDto>>> GetLuongThangTaiXeAsync(
        DateRangeRequest request,
        CancellationToken cancellationToken = default);
}
