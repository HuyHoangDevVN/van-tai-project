using Core.Sql.Models;

namespace Application.DTOs.VanTai;

// =============================================================================
// DTOs for Maintenance Module
// =============================================================================

/// <summary>
/// DTO cho kết quả kiểm tra trạng thái bảo trì xe.
/// </summary>
public class MaintenanceStatusDto
{
    [CustomDataSet("ma_xe")]
    public string MaXe { get; set; } = string.Empty;

    [CustomDataSet("ten_xe")]
    public string? TenXe { get; set; }

    [CustomDataSet("bien_so")]
    public string? BienSo { get; set; }

    [CustomDataSet("trang_thai")]
    public string? TrangThai { get; set; }

    [CustomDataSet("tong_km_van_hanh")]
    public float? TongKmVanHanh { get; set; }

    [CustomDataSet("ngay_bao_tri_cuoi")]
    public DateTime? NgayBaoTriCuoi { get; set; }

    [CustomDataSet("ngay_dang_kiem")]
    public DateTime? NgayDangKiem { get; set; }

    [CustomDataSet("so_ngay_tu_bao_tri")]
    public int SoNgayTuBaoTri { get; set; }

    [CustomDataSet("trang_thai_bao_tri")]
    public string? TrangThaiBaoTri { get; set; }

    [CustomDataSet("can_bao_tri")]
    public bool CanBaoTri { get; set; }

    public DateTime? NgayBaoTriTiepTheo { get; set; }

    public int? SoNgayConLai { get; set; }

    public int? SoNgayDenDangKiem { get; set; }

    public string? TrangThaiDangKiem { get; set; }
}

public class MaintenanceAlertDto
{
    [CustomDataSet("id")]
    public long Id { get; set; }

    [CustomDataSet("ma_xe")]
    public string MaXe { get; set; } = string.Empty;

    [CustomDataSet("ten_xe")]
    public string? TenXe { get; set; }

    [CustomDataSet("bien_so")]
    public string? BienSo { get; set; }

    [CustomDataSet("alert_type")]
    public string AlertType { get; set; } = string.Empty;

    [CustomDataSet("status")]
    public string Status { get; set; } = string.Empty;

    [CustomDataSet("severity")]
    public string Severity { get; set; } = string.Empty;

    [CustomDataSet("title")]
    public string Title { get; set; } = string.Empty;

    [CustomDataSet("message_snapshot")]
    public string MessageSnapshot { get; set; } = string.Empty;

    [CustomDataSet("due_date")]
    public DateTime? DueDate { get; set; }

    [CustomDataSet("triggered_at")]
    public DateTime TriggeredAt { get; set; }

    [CustomDataSet("last_seen_at")]
    public DateTime LastSeenAt { get; set; }

    [CustomDataSet("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    [CustomDataSet("resolved_by")]
    public string? ResolvedBy { get; set; }
}

public class MaintenanceAlertScanResultDto
{
    public int TotalEvaluated { get; set; }

    public int ActiveAlerts { get; set; }

    public int ResolvedAlerts { get; set; }
}

public class MaintenanceAlertQuery
{
    public string? Status { get; set; }

    public string? Severity { get; set; }

    public string? MaXe { get; set; }
}

public class ResolveMaintenanceAlertRequest
{
    public string? ResolvedBy { get; set; }
}

// =============================================================================
// DTOs for Trip Module
// =============================================================================

/// <summary>
/// DTO cho kết quả tìm kiếm chuyến xe.
/// </summary>
public class TripSearchResultDto
{
    [CustomDataSet("ma_chuyen")]
    public string MaChuyen { get; set; } = string.Empty;

    [CustomDataSet("ten_chuyen")]
    public string? TenChuyen { get; set; }

    [CustomDataSet("thoi_gian_khoi_hanh")]
    public DateTime? ThoiGianKhoiHanh { get; set; }

    [CustomDataSet("thoi_gian_den")]
    public DateTime? ThoiGianDen { get; set; }

    [CustomDataSet("ma_xe")]
    public string? MaXe { get; set; }

    [CustomDataSet("ten_xe")]
    public string? TenXe { get; set; }

    [CustomDataSet("bien_so")]
    public string? BienSo { get; set; }

    [CustomDataSet("ma_tuyen")]
    public string? MaTuyen { get; set; }

    [CustomDataSet("ten_tuyen")]
    public string? TenTuyen { get; set; }

    [CustomDataSet("trang_thai")]
    public string? TrangThai { get; set; }

    [CustomDataSet("so_ve_da_ban")]
    public int SoVeDaBan { get; set; }

    [CustomDataSet("tong_cho")]
    public int TongCho { get; set; }
}

// =============================================================================
// DTOs for Search Module
// =============================================================================

/// <summary>
/// DTO cho kết quả tìm kiếm xe.
/// </summary>
public class BusSearchResultDto
{
    [CustomDataSet("ma_xe")]
    public string MaXe { get; set; } = string.Empty;

    [CustomDataSet("ten_xe")]
    public string? TenXe { get; set; }

    [CustomDataSet("bien_so")]
    public string? BienSo { get; set; }

    [CustomDataSet("hang_san_xuat")]
    public string? HangSanXuat { get; set; }

    [CustomDataSet("nam_san_xuat")]
    public int? NamSanXuat { get; set; }

    [CustomDataSet("ngay_dang_kiem")]
    public DateTime? NgayDangKiem { get; set; }

    [CustomDataSet("trang_thai")]
    public string? TrangThai { get; set; }

    [CustomDataSet("tong_km_van_hanh")]
    public float? TongKmVanHanh { get; set; }

    [CustomDataSet("ngay_bao_tri_cuoi")]
    public DateTime? NgayBaoTriCuoi { get; set; }

    [CustomDataSet("so_cho_ngoi")]
    public int SoChoNgoi { get; set; }

    [CustomDataSet("total_trips")]
    public int TotalTrips { get; set; }

    [CustomDataSet("driver_name")]
    public string? DriverName { get; set; }
}

/// <summary>
/// DTO cho kết quả tìm kiếm tài xế.
/// </summary>
public class DriverSearchResultDto
{
    [CustomDataSet("ma_tai_xe")]
    public string MaTaiXe { get; set; } = string.Empty;

    [CustomDataSet("ten_tai_xe")]
    public string? TenTaiXe { get; set; }

    [CustomDataSet("ngay_sinh")]
    public DateTime? NgaySinh { get; set; }

    [CustomDataSet("gioi_tinh")]
    public string? GioiTinh { get; set; }

    [CustomDataSet("que_quan")]
    public string? QueQuan { get; set; }

    [CustomDataSet("so_cccd")]
    public string? SoCccd { get; set; }

    [CustomDataSet("ngay_ky_hop_dong")]
    public DateTime? NgayKyHopDong { get; set; }

    [CustomDataSet("tuoi")]
    public int? Tuoi { get; set; }

    [CustomDataSet("he_so_luong")]
    public decimal? HeSoLuong { get; set; }

    [CustomDataSet("tong_so_chuyen")]
    public int TongSoChuyen { get; set; }

    [CustomDataSet("total_assignments")]
    public int TotalAssignments { get; set; }

    [CustomDataSet("current_vehicle")]
    public string? CurrentVehicle { get; set; }
}

/// <summary>
/// DTO cho kết quả tìm kiếm tuyến đường.
/// </summary>
public class RouteSearchResultDto
{
    [CustomDataSet("ma_tuyen")]
    public string MaTuyen { get; set; } = string.Empty;

    [CustomDataSet("ten_tuyen")]
    public string? TenTuyen { get; set; }

    [CustomDataSet("diem_di")]
    public string? DiemDi { get; set; }

    [CustomDataSet("diem_den")]
    public string? DiemDen { get; set; }

    [CustomDataSet("khoang_cach")]
    public float? KhoangCach { get; set; }

    [CustomDataSet("ma_do_phuc_tap")]
    public string? MaDoPhucTap { get; set; }

    [CustomDataSet("ten_do_phuc_tap")]
    public string? TenDoPhucTap { get; set; }

    [CustomDataSet("total_trips")]
    public int TotalTrips { get; set; }

    [CustomDataSet("total_revenue")]
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// DTO cho kết quả tìm kiếm vé.
/// </summary>
public class TicketSearchResultDto
{
    [CustomDataSet("stt")]
    public int Stt { get; set; }

    [CustomDataSet("ma_khach")]
    public string? MaKhach { get; set; }

    [CustomDataSet("ma_chuyen")]
    public string? MaChuyen { get; set; }

    [CustomDataSet("phuong_thuc_tt")]
    public string? PhuongThucTT { get; set; }

    [CustomDataSet("thoi_gian_dat")]
    public DateTime? ThoiGianDat { get; set; }

    [CustomDataSet("vi_tri")]
    public string? ViTri { get; set; }

    [CustomDataSet("trang_thai_tt")]
    public string? TrangThaiTT { get; set; }

    [CustomDataSet("ma_ghe")]
    public int? MaGhe { get; set; }

    [CustomDataSet("ma_giuong")]
    public int? MaGiuong { get; set; }

    [CustomDataSet("ten_khach")]
    public string? TenKhach { get; set; }

    [CustomDataSet("dien_thoai")]
    public string? DienThoai { get; set; }

    [CustomDataSet("thoi_gian_khoi_hanh")]
    public DateTime? ThoiGianKhoiHanh { get; set; }

    [CustomDataSet("ten_tuyen")]
    public string? TenTuyen { get; set; }

    [CustomDataSet("gia_ve")]
    public decimal? GiaVe { get; set; }
}

/// <summary>
/// DTO cho kết quả tìm kiếm khách hàng.
/// </summary>
public class CustomerSearchResultDto
{
    [CustomDataSet("ma_khach")]
    public string MaKhach { get; set; } = string.Empty;

    [CustomDataSet("ten")]
    public string? Ten { get; set; }

    [CustomDataSet("ngay_sinh")]
    public DateTime? NgaySinh { get; set; }

    [CustomDataSet("dien_thoai")]
    public string? DienThoai { get; set; }

    [CustomDataSet("email")]
    public string? Email { get; set; }

    [CustomDataSet("so_cccd")]
    public string? SoCccd { get; set; }

    [CustomDataSet("ma_giam_ho")]
    public string? MaGiamHo { get; set; }

    [CustomDataSet("total_tickets")]
    public int TotalTickets { get; set; }

    [CustomDataSet("total_spending")]
    public decimal TotalSpending { get; set; }

    [CustomDataSet("last_booking_date")]
    public DateTime? LastBookingDate { get; set; }

    [CustomDataSet("guardian_name")]
    public string? GuardianName { get; set; }
}

// =============================================================================
// Request Models for New Modules
// =============================================================================

/// <summary>
/// Request tạo chuyến xe mới.
/// </summary>
public class TripCreateRequest
{
    public string MaChuyen { get; set; } = string.Empty;
    public string? TenChuyen { get; set; }
    public DateTime ThoiGianKhoiHanh { get; set; }
    public DateTime ThoiGianDen { get; set; }
    public string MaXe { get; set; } = string.Empty;
    public string MaTuyen { get; set; } = string.Empty;
}

/// <summary>
/// Request đặt vé.
/// </summary>
public class TicketBookingRequest
{
    public string MaKhach { get; set; } = string.Empty;
    public string MaChuyen { get; set; } = string.Empty;
    public string? PhuongThucTT { get; set; }
    public string? ViTri { get; set; }
    public int? MaGhe { get; set; }
    public int? MaGiuong { get; set; }
}

/// <summary>
/// Request tìm kiếm xe.
/// </summary>
public class BusSearchRequest
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public string? HangSanXuat { get; set; }
}

/// <summary>
/// Request tìm kiếm tài xế.
/// </summary>
public class DriverSearchRequest
{
    public string? Keyword { get; set; }
    public string? GioiTinh { get; set; }
    public string? QueQuan { get; set; }
}

/// <summary>
/// Request tìm kiếm tuyến đường.
/// </summary>
public class RouteSearchRequest
{
    public string? Keyword { get; set; }
    public string? DiemDi { get; set; }
    public string? DiemDen { get; set; }
}

/// <summary>
/// Request tìm kiếm chuyến xe.
/// </summary>
public class TripSearchRequest
{
    public string? Keyword { get; set; }
    public string? TrangThai { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

/// <summary>
/// Request tạo bảo trì mới.
/// </summary>
public class MaintenanceCreateRequest
{
    public string MaBaoTri { get; set; } = string.Empty;
    public string MaXe { get; set; } = string.Empty;
    public string? DonVi { get; set; }
    public decimal ChiPhi { get; set; }
    public DateTime Ngay { get; set; }
    public int SoKm { get; set; }
}
