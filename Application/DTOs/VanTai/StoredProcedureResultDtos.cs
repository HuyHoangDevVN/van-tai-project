using Core.Sql.Models;

namespace Application.DTOs.VanTai;

// =============================================================================
// STORED PROCEDURE RESULT DTOs
// DTOs for results returned by stored procedures
// =============================================================================

/// <summary>
/// Kết quả tính chi phí cơ bản chuyến xe.
/// Procedure: proc_chi_phi_co_ban
/// </summary>
public class ChiPhiCoBanDto
{
    [CustomDataSet("ma_chuyen")]
    public string MaChuyen { get; set; } = string.Empty;

    [CustomDataSet("ten_chuyen")]
    public string? TenChuyen { get; set; }

    [CustomDataSet("ma_xe")]
    public string? MaXe { get; set; }

    [CustomDataSet("khoang_cach")]
    public float? KhoangCach { get; set; }

    [CustomDataSet("chi_phi_co_ban")]
    public decimal? ChiPhiCoBan { get; set; }
}

/// <summary>
/// Kết quả doanh thu xe bus ngồi theo tháng.
/// Procedure: proc_doanh_thu_xe_bus_ngoi_thang
/// </summary>
public class DoanhThuXeBusNgoiThangDto
{
    [CustomDataSet("ma_xe")]
    public string MaXe { get; set; } = string.Empty;

    [CustomDataSet("thang")]
    public string Thang { get; set; } = string.Empty;

    [CustomDataSet("doanh_thu_thang")]
    public decimal? DoanhThuThang { get; set; }
}

/// <summary>
/// Kết quả giá vé xe bus ngồi.
/// Procedure: proc_gia_ve_xe_bus_ngoi
/// </summary>
public class GiaVeXeBusNgoiDto
{
    [CustomDataSet("stt")]
    public int Stt { get; set; }

    [CustomDataSet("ma_chuyen")]
    public string MaChuyen { get; set; } = string.Empty;

    [CustomDataSet("ten_chuyen")]
    public string? TenChuyen { get; set; }

    [CustomDataSet("vi_tri")]
    public string? ViTri { get; set; }

    [CustomDataSet("ma_xe")]
    public string? MaXe { get; set; }

    [CustomDataSet("chi_phi_co_ban")]
    public decimal? ChiPhiCoBan { get; set; }

    [CustomDataSet("khoang_cach")]
    public float? KhoangCach { get; set; }

    [CustomDataSet("gia_ve")]
    public decimal? GiaVe { get; set; }
}

/// <summary>
/// Kết quả doanh thu tuyến đường theo tháng.
/// Procedure: proc_tinh_doanh_thu_tuyen_duong_theo_thang
/// </summary>
public class DoanhThuTuyenDuongThangDto
{
    [CustomDataSet("ma_tuyen")]
    public string MaTuyen { get; set; } = string.Empty;

    [CustomDataSet("ten_tuyen")]
    public string? TenTuyen { get; set; }

    [CustomDataSet("thang")]
    public string Thang { get; set; } = string.Empty;

    [CustomDataSet("doanh_thu_thang")]
    public decimal? DoanhThuThang { get; set; }
}

/// <summary>
/// Kết quả tính lương tháng tài xế.
/// Procedure: proc_tinh_luong_thang
/// </summary>
public class LuongThangTaiXeDto
{
    [CustomDataSet("ma_tai_xe")]
    public string MaTaiXe { get; set; } = string.Empty;

    [CustomDataSet("ten_tai_xe")]
    public string? TenTaiXe { get; set; }

    [CustomDataSet("tong_km")]
    public float? TongKm { get; set; }

    [CustomDataSet("so_tuyen")]
    public int? SoTuyen { get; set; }

    [CustomDataSet("luong_thang")]
    public decimal? LuongThang { get; set; }
}

/// <summary>
/// Kết quả liệt kê tuyến đường xe giường nằm.
/// Procedure: proc_liet_ke_tuyen_duong_xe_giuong_nam
/// </summary>
public class TuyenDuongXeGiuongNamDto
{
    [CustomDataSet("ma_tuyen")]
    public string MaTuyen { get; set; } = string.Empty;

    [CustomDataSet("ten_tuyen")]
    public string? TenTuyen { get; set; }

    [CustomDataSet("khoang_cach")]
    public float? KhoangCach { get; set; }

    [CustomDataSet("loai_xe")]
    public string? LoaiXe { get; set; }
}

/// <summary>
/// Kết quả liệt kê khách hàng quay lại.
/// Procedure: proc_liet_ke_khach_hang_quay_lai
/// </summary>
public class KhachHangQuayLaiDto
{
    [CustomDataSet("ma_khach_hang")]
    public string MaKhachHang { get; set; } = string.Empty;

    [CustomDataSet("ten_khach_hang")]
    public string? TenKhachHang { get; set; }

    [CustomDataSet("so_lan_di")]
    public int? SoLanDi { get; set; }

    [CustomDataSet("tong_tien")]
    public decimal? TongTien { get; set; }
}
