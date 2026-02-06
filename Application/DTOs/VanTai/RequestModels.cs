using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTOs.VanTai;

// =============================================================================
// REQUEST MODELS - For Stored Procedures with Parameters
// =============================================================================

/// <summary>
/// Request model for date range queries.
/// Used by: proc_doanh_thu_xe_bus_ngoi_thang, proc_tinh_doanh_thu_tuyen_duong_theo_thang, proc_tinh_luong_thang
/// </summary>
public class DateRangeRequest
{
    /// <summary>
    /// Ngày bắt đầu (From date)
    /// </summary>
    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
    [JsonPropertyName("tuNgay")]
    public DateTime TuNgay { get; set; }

    /// <summary>
    /// Ngày kết thúc (To date)
    /// </summary>
    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
    [JsonPropertyName("denNgay")]
    public DateTime DenNgay { get; set; }
}

// =============================================================================
// CREATE/UPDATE REQUEST MODELS FOR ENTITIES
// =============================================================================

/// <summary>
/// Request model for creating/updating TaiXe (Driver)
/// </summary>
public class TaiXeRequest
{
    [Required(ErrorMessage = "Mã tài xế là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã tài xế tối đa 20 ký tự")]
    [JsonPropertyName("maTaiXe")]
    public string MaTaiXe { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Tên tài xế tối đa 100 ký tự")]
    [JsonPropertyName("tenTaiXe")]
    public string? TenTaiXe { get; set; }

    [JsonPropertyName("ngaySinh")]
    public DateTime? NgaySinh { get; set; }

    [StringLength(10, ErrorMessage = "Giới tính tối đa 10 ký tự")]
    [JsonPropertyName("gioiTinh")]
    public string? GioiTinh { get; set; }

    [StringLength(100, ErrorMessage = "Quê quán tối đa 100 ký tự")]
    [JsonPropertyName("queQuan")]
    public string? QueQuan { get; set; }

    [StringLength(50, ErrorMessage = "Tôn giáo tối đa 50 ký tự")]
    [JsonPropertyName("tonGiao")]
    public string? TonGiao { get; set; }

    [StringLength(20, ErrorMessage = "Số CCCD tối đa 20 ký tự")]
    [JsonPropertyName("soCccd")]
    public string? SoCccd { get; set; }

    [JsonPropertyName("ngayKyHopDong")]
    public DateTime? NgayKyHopDong { get; set; }

    [Range(0, 200, ErrorMessage = "Tuổi phải từ 0 đến 200")]
    [JsonPropertyName("tuoi")]
    public int? Tuoi { get; set; }

    [Range(0, 100, ErrorMessage = "Hệ số lương phải từ 0 đến 100")]
    [JsonPropertyName("heSoLuong")]
    public decimal? HeSoLuong { get; set; }
}

/// <summary>
/// Request model for creating/updating Xe (Vehicle)
/// </summary>
public class XeRequest
{
    [Required(ErrorMessage = "Mã xe là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã xe tối đa 20 ký tự")]
    [JsonPropertyName("maXe")]
    public string MaXe { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Tên xe tối đa 50 ký tự")]
    [JsonPropertyName("tenXe")]
    public string? TenXe { get; set; }

    [StringLength(20, ErrorMessage = "Biển số tối đa 20 ký tự")]
    [JsonPropertyName("bienSo")]
    public string? BienSo { get; set; }

    [StringLength(50, ErrorMessage = "Hãng sản xuất tối đa 50 ký tự")]
    [JsonPropertyName("hangSanXuat")]
    public string? HangSanXuat { get; set; }

    [Range(1900, 2100, ErrorMessage = "Năm sản xuất không hợp lệ")]
    [JsonPropertyName("namSanXuat")]
    public int? NamSanXuat { get; set; }

    [JsonPropertyName("ngayDangKiem")]
    public DateTime? NgayDangKiem { get; set; }

    [StringLength(30, ErrorMessage = "Trạng thái tối đa 30 ký tự")]
    [JsonPropertyName("trangThai")]
    public string? TrangThai { get; set; }

    [Range(0, 1000, ErrorMessage = "Mức tiêu hao phải từ 0 đến 1000")]
    [JsonPropertyName("mucTieuHao")]
    public float? MucTieuHao { get; set; }

    [JsonPropertyName("phuThuPhiVanHanh")]
    public decimal? PhuThuPhiVanHanh { get; set; }
}

/// <summary>
/// Request model for creating/updating ChuyenXe (Trip)
/// </summary>
public class ChuyenXeRequest
{
    [Required(ErrorMessage = "Mã chuyến là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã chuyến tối đa 20 ký tự")]
    [JsonPropertyName("maChuyen")]
    public string MaChuyen { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Tên chuyến tối đa 100 ký tự")]
    [JsonPropertyName("tenChuyen")]
    public string? TenChuyen { get; set; }

    [JsonPropertyName("thoiGianKhoiHanh")]
    public DateTime? ThoiGianKhoiHanh { get; set; }

    [JsonPropertyName("thoiGianDen")]
    public DateTime? ThoiGianDen { get; set; }

    [StringLength(20, ErrorMessage = "Mã xe tối đa 20 ký tự")]
    [JsonPropertyName("maXe")]
    public string? MaXe { get; set; }

    [StringLength(20, ErrorMessage = "Mã tuyến tối đa 20 ký tự")]
    [JsonPropertyName("maTuyen")]
    public string? MaTuyen { get; set; }
}

/// <summary>
/// Request model for creating/updating KhachHang (Customer)
/// </summary>
public class KhachHangRequest
{
    [Required(ErrorMessage = "Mã khách là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã khách tối đa 20 ký tự")]
    [JsonPropertyName("maKhach")]
    public string MaKhach { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
    [JsonPropertyName("ten")]
    public string? Ten { get; set; }

    [JsonPropertyName("ngaySinh")]
    public DateTime? NgaySinh { get; set; }

    [StringLength(20, ErrorMessage = "Điện thoại tối đa 20 ký tự")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [JsonPropertyName("dienThoai")]
    public string? DienThoai { get; set; }

    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Số CCCD tối đa 20 ký tự")]
    [JsonPropertyName("soCccd")]
    public string? SoCccd { get; set; }

    [StringLength(20, ErrorMessage = "Mã giám hộ tối đa 20 ký tự")]
    [JsonPropertyName("maGiamHo")]
    public string? MaGiamHo { get; set; }
}

/// <summary>
/// Request model for creating/updating TuyenDuong (Route)
/// </summary>
public class TuyenDuongRequest
{
    [Required(ErrorMessage = "Mã tuyến là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã tuyến tối đa 20 ký tự")]
    [JsonPropertyName("maTuyen")]
    public string MaTuyen { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Điểm đi tối đa 100 ký tự")]
    [JsonPropertyName("diemDi")]
    public string? DiemDi { get; set; }

    [StringLength(100, ErrorMessage = "Điểm đến tối đa 100 ký tự")]
    [JsonPropertyName("diemDen")]
    public string? DiemDen { get; set; }

    [Range(0, 10000, ErrorMessage = "Khoảng cách phải từ 0 đến 10000")]
    [JsonPropertyName("khoangCach")]
    public float? KhoangCach { get; set; }

    [StringLength(100, ErrorMessage = "Tên tuyến tối đa 100 ký tự")]
    [JsonPropertyName("tenTuyen")]
    public string? TenTuyen { get; set; }

    [StringLength(20, ErrorMessage = "Mã độ phức tạp tối đa 20 ký tự")]
    [JsonPropertyName("maDoPhucTap")]
    public string? MaDoPhucTap { get; set; }
}

/// <summary>
/// Request model for creating/updating Ve (Ticket)
/// </summary>
public class VeRequest
{
    [StringLength(20, ErrorMessage = "Mã khách tối đa 20 ký tự")]
    [JsonPropertyName("maKhach")]
    public string? MaKhach { get; set; }

    [StringLength(20, ErrorMessage = "Mã chuyến tối đa 20 ký tự")]
    [JsonPropertyName("maChuyen")]
    public string? MaChuyen { get; set; }

    [StringLength(50, ErrorMessage = "Phương thức thanh toán tối đa 50 ký tự")]
    [JsonPropertyName("phuongThucTT")]
    public string? PhuongThucTT { get; set; }

    [JsonPropertyName("thoiGianDat")]
    public DateTime? ThoiGianDat { get; set; }

    [StringLength(20, ErrorMessage = "Vị trí tối đa 20 ký tự")]
    [JsonPropertyName("viTri")]
    public string? ViTri { get; set; }

    [StringLength(30, ErrorMessage = "Trạng thái thanh toán tối đa 30 ký tự")]
    [JsonPropertyName("trangThaiTT")]
    public string? TrangThaiTT { get; set; }

    [JsonPropertyName("maGhe")]
    public int? MaGhe { get; set; }

    [JsonPropertyName("maGiuong")]
    public int? MaGiuong { get; set; }
}

/// <summary>
/// Request model for creating/updating BaoTri (Maintenance)
/// </summary>
public class BaoTriRequest
{
    [Required(ErrorMessage = "Mã bảo trì là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã bảo trì tối đa 20 ký tự")]
    [JsonPropertyName("maBaoTri")]
    public string MaBaoTri { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Mã xe tối đa 20 ký tự")]
    [JsonPropertyName("maXe")]
    public string? MaXe { get; set; }

    [StringLength(100, ErrorMessage = "Đơn vị tối đa 100 ký tự")]
    [JsonPropertyName("donVi")]
    public string? DonVi { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Chi phí phải là số dương")]
    [JsonPropertyName("chiPhi")]
    public decimal? ChiPhi { get; set; }

    [JsonPropertyName("ngay")]
    public DateTime? Ngay { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Số km phải là số dương")]
    [JsonPropertyName("soKm")]
    public int? SoKm { get; set; }
}
