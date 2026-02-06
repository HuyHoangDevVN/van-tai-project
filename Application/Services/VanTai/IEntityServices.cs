using Application.DTOs.VanTai;
using Application.Models;
using Core.Sql.Models;

namespace Application.Services.VanTai;

/// <summary>
/// Interface for TaiXe (Driver) Service.
/// CRUD operations for tai_xe table.
/// </summary>
public interface ITaiXeService
{
    /// <summary>
    /// Tìm kiếm tài xế với phân trang.
    /// </summary>
    Task<BaseResponse<TPaging<DriverSearchResultDto>>> SearchAsync(
        DriverSearchReq request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách tài xế.
    /// </summary>
    Task<BaseResponse<List<TaiXeDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin tài xế theo mã.
    /// </summary>
    Task<BaseResponse<TaiXeDto?>> GetByIdAsync(string maTaiXe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới tài xế.
    /// </summary>
    Task<BaseResponse<object>> CreateAsync(TaiXeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin tài xế.
    /// </summary>
    Task<BaseResponse<object>> UpdateAsync(string maTaiXe, TaiXeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa tài xế.
    /// </summary>
    Task<BaseResponse<object>> DeleteAsync(string maTaiXe, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for Xe (Vehicle) Service.
/// CRUD operations for xe table.
/// </summary>
public interface IXeService
{
    /// <summary>
    /// Tìm kiếm xe với phân trang.
    /// </summary>
    Task<BaseResponse<TPaging<BusSearchResultDto>>> SearchAsync(
        BusSearchReq request, CancellationToken cancellationToken = default);

    Task<BaseResponse<List<XeDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<XeDto?>> GetByIdAsync(string maXe, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> CreateAsync(XeRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> UpdateAsync(string maXe, XeRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> DeleteAsync(string maXe, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for ChuyenXe (Trip) Service.
/// CRUD operations for chuyen_xe table.
/// </summary>
public interface IChuyenXeService
{
    /// <summary>
    /// Tìm kiếm chuyến xe với phân trang.
    /// </summary>
    Task<BaseResponse<TPaging<TripSearchResultDto>>> SearchAsync(
        TripSearchReq request, CancellationToken cancellationToken = default);

    Task<BaseResponse<List<ChuyenXeDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<ChuyenXeDto?>> GetByIdAsync(string maChuyen, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> CreateAsync(ChuyenXeRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> UpdateAsync(string maChuyen, ChuyenXeRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> DeleteAsync(string maChuyen, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for KhachHang (Customer) Service.
/// CRUD operations for khach_hang table.
/// </summary>
public interface IKhachHangService
{
    /// <summary>
    /// Tìm kiếm khách hàng với phân trang.
    /// </summary>
    Task<BaseResponse<TPaging<CustomerSearchResultDto>>> SearchAsync(
        CustomerSearchReq request, CancellationToken cancellationToken = default);

    Task<BaseResponse<List<KhachHangDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<KhachHangDto?>> GetByIdAsync(string maKhach, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> CreateAsync(KhachHangRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> UpdateAsync(string maKhach, KhachHangRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> DeleteAsync(string maKhach, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for TuyenDuong (Route) Service.
/// CRUD operations for tuyen_duong table.
/// </summary>
public interface ITuyenDuongService
{
    /// <summary>
    /// Tìm kiếm tuyến đường với phân trang.
    /// </summary>
    Task<BaseResponse<TPaging<RouteSearchResultDto>>> SearchAsync(
        RouteSearchReq request, CancellationToken cancellationToken = default);

    Task<BaseResponse<List<TuyenDuongDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<TuyenDuongDto?>> GetByIdAsync(string maTuyen, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> CreateAsync(TuyenDuongRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> UpdateAsync(string maTuyen, TuyenDuongRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> DeleteAsync(string maTuyen, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for Ve (Ticket) Service.
/// CRUD operations for ve table.
/// </summary>
public interface IVeService
{
    /// <summary>
    /// Tìm kiếm vé với phân trang.
    /// </summary>
    Task<BaseResponse<TPaging<TicketSearchResultDto>>> SearchAsync(
        TicketSearchReq request, CancellationToken cancellationToken = default);

    Task<BaseResponse<List<VeDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<VeDto?>> GetByIdAsync(int stt, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> CreateAsync(VeRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> UpdateAsync(int stt, VeRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<object>> DeleteAsync(int stt, CancellationToken cancellationToken = default);
}
