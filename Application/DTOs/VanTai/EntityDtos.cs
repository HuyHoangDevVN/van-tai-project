using Core.Sql.Models;

namespace Application.DTOs.VanTai;

// =============================================================================
// BẢNG LÁI - Driver's License
// =============================================================================

/// <summary>
/// DTO cho bảng lái (Driver's License).
/// Maps to table: bang_lai
/// </summary>
public class BangLaiDto
{
    [CustomDataSet("ma_bang_lai")]
    public string MaBangLai { get; set; } = string.Empty;

    [CustomDataSet("ten_bang")]
    public string? TenBang { get; set; }

    [CustomDataSet("hang_bang")]
    public string? HangBang { get; set; }

    [CustomDataSet("so_giay_phep")]
    public string? SoGiayPhep { get; set; }

    [CustomDataSet("ngay_cap")]
    public DateTime? NgayCap { get; set; }

    [CustomDataSet("ngay_het_han")]
    public DateTime? NgayHetHan { get; set; }

    [CustomDataSet("co_quan_cap")]
    public string? CoQuanCap { get; set; }

    [CustomDataSet("ma_tai_xe")]
    public string? MaTaiXe { get; set; }
}

// =============================================================================
// BẢO TRÌ - Vehicle Maintenance
// =============================================================================

/// <summary>
/// DTO cho bảo trì xe (Vehicle Maintenance).
/// Maps to table: bao_tri
/// </summary>
public class BaoTriDto
{
    [CustomDataSet("ma_bao_tri")]
    public string MaBaoTri { get; set; } = string.Empty;

    [CustomDataSet("ma_xe")]
    public string? MaXe { get; set; }

    [CustomDataSet("don_vi")]
    public string? DonVi { get; set; }

    [CustomDataSet("chi_phi")]
    public decimal? ChiPhi { get; set; }

    [CustomDataSet("ngay")]
    public DateTime? Ngay { get; set; }

    [CustomDataSet("so_km")]
    public int? SoKm { get; set; }
}

// =============================================================================
// CHỈ ĐỊNH - Driver Assignment
// =============================================================================

/// <summary>
/// DTO cho chỉ định tài xế (Driver Assignment).
/// Maps to table: chi_dinh
/// </summary>
public class ChiDinhDto
{
    [CustomDataSet("ma_chuyen")]
    public string MaChuyen { get; set; } = string.Empty;

    [CustomDataSet("ma_tai_xe")]
    public string MaTaiXe { get; set; } = string.Empty;

    [CustomDataSet("ma_vi_tri")]
    public string MaViTri { get; set; } = string.Empty;
}

// =============================================================================
// CHUYẾN XE - Trip/Journey
// =============================================================================

/// <summary>
/// DTO cho chuyến xe (Trip/Journey).
/// Maps to table: chuyen_xe
/// </summary>
public class ChuyenXeDto
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

    [CustomDataSet("ma_tuyen")]
    public string? MaTuyen { get; set; }
}

// =============================================================================
// ĐỘ PHỨC TẠP - Complexity Level
// =============================================================================

/// <summary>
/// DTO cho độ phức tạp tuyến đường (Route Complexity).
/// Maps to table: do_phuc_tap
/// </summary>
public class DoPhucTapDto
{
    [CustomDataSet("ma_do_phuc_tap")]
    public string MaDoPhucTap { get; set; } = string.Empty;

    [CustomDataSet("ten_muc")]
    public string? TenMuc { get; set; }

    [CustomDataSet("phu_cap_tai_xe")]
    public decimal? PhuCapTaiXe { get; set; }

    [CustomDataSet("ty_le_chi_phi_van_hanh_tang_them")]
    public decimal? TyLeChiPhiVanHanhTangThem { get; set; }
}

// =============================================================================
// GHẾ - Seat (Bus)
// =============================================================================

/// <summary>
/// DTO cho ghế xe bus (Bus Seat).
/// Maps to table: ghe
/// </summary>
public class GheDto
{
    [CustomDataSet("ma_ghe")]
    public int MaGhe { get; set; }

    [CustomDataSet("ma_xe_bus")]
    public string MaXeBus { get; set; } = string.Empty;

    [CustomDataSet("vi_tri")]
    public string ViTri { get; set; } = string.Empty;

    [CustomDataSet("ke_chan")]
    public bool? KeChan { get; set; }

    [CustomDataSet("tua_dau")]
    public bool? TuaDau { get; set; }

    [CustomDataSet("ban_an")]
    public bool? BanAn { get; set; }

    [CustomDataSet("trang_thai")]
    public string? TrangThai { get; set; }

    [CustomDataSet("phu_thu_tien_ich")]
    public decimal? PhuThuTienIch { get; set; }
}

// =============================================================================
// GIƯỜNG - Bed (Sleeper Bus)
// =============================================================================

/// <summary>
/// DTO cho giường xe giường nằm (Sleeper Bed).
/// Maps to table: giuong
/// </summary>
public class GiuongDto
{
    [CustomDataSet("ma_giuong")]
    public int MaGiuong { get; set; }

    [CustomDataSet("ma_xe_giuong")]
    public string MaXeGiuong { get; set; } = string.Empty;

    [CustomDataSet("vi_tri")]
    public string ViTri { get; set; } = string.Empty;

    [CustomDataSet("co_den")]
    public bool? CoDen { get; set; }

    [CustomDataSet("co_rem")]
    public bool? CoRem { get; set; }

    [CustomDataSet("co_usb")]
    public bool? CoUsb { get; set; }

    [CustomDataSet("co_tv")]
    public bool? CoTv { get; set; }

    [CustomDataSet("trang_thai")]
    public string? TrangThai { get; set; }

    [CustomDataSet("phu_thu_tien_ich")]
    public decimal? PhuThuTienIch { get; set; }
}

// =============================================================================
// KHÁCH HÀNG - Customer
// =============================================================================

/// <summary>
/// DTO cho khách hàng (Customer).
/// Maps to table: khach_hang
/// </summary>
public class KhachHangDto
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
}

// =============================================================================
// LỄ - Holiday
// =============================================================================

/// <summary>
/// DTO cho ngày lễ (Holiday).
/// Maps to table: le
/// </summary>
public class LeDto
{
    [CustomDataSet("ma_le")]
    public string MaLe { get; set; } = string.Empty;

    [CustomDataSet("ten_le")]
    public string? TenLe { get; set; }

    [CustomDataSet("tu_ngay")]
    public DateTime? TuNgay { get; set; }

    [CustomDataSet("den_ngay")]
    public DateTime? DenNgay { get; set; }

    [CustomDataSet("phi_phu_thu")]
    public decimal? PhiPhuThu { get; set; }
}

// =============================================================================
// PHÍ LỄ TẾT - Holiday Surcharge
// =============================================================================

/// <summary>
/// DTO cho phí lễ tết (Holiday Surcharge).
/// Maps to table: phi_le_tet
/// </summary>
public class PhiLeTetDto
{
    [CustomDataSet("ma_le")]
    public string MaLe { get; set; } = string.Empty;

    [CustomDataSet("ma_chuyen")]
    public string MaChuyen { get; set; } = string.Empty;
}

// =============================================================================
// SỐ ĐIỆN THOẠI TÀI XẾ - Driver Phone Number
// =============================================================================

/// <summary>
/// DTO cho số điện thoại tài xế (Driver Phone).
/// Maps to table: so_dien_thoai_tai_xe
/// </summary>
public class SoDienThoaiTaiXeDto
{
    [CustomDataSet("ma_tai_xe")]
    public string MaTaiXe { get; set; } = string.Empty;

    [CustomDataSet("so")]
    public string So { get; set; } = string.Empty;
}

// =============================================================================
// TÀI XẾ - Driver
// =============================================================================

/// <summary>
/// DTO cho tài xế (Driver).
/// Maps to table: tai_xe
/// </summary>
public class TaiXeDto
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

    [CustomDataSet("ton_giao")]
    public string? TonGiao { get; set; }

    [CustomDataSet("so_cccd")]
    public string? SoCccd { get; set; }

    [CustomDataSet("ngay_ky_hop_dong")]
    public DateTime? NgayKyHopDong { get; set; }

    [CustomDataSet("tuoi")]
    public int? Tuoi { get; set; }

    [CustomDataSet("he_so_luong")]
    public decimal? HeSoLuong { get; set; }
}

// =============================================================================
// TUYẾN ĐƯỜNG - Route
// =============================================================================

/// <summary>
/// DTO cho tuyến đường (Route).
/// Maps to table: tuyen_duong
/// </summary>
public class TuyenDuongDto
{
    [CustomDataSet("ma_tuyen")]
    public string MaTuyen { get; set; } = string.Empty;

    [CustomDataSet("diem_di")]
    public string? DiemDi { get; set; }

    [CustomDataSet("diem_den")]
    public string? DiemDen { get; set; }

    [CustomDataSet("khoang_cach")]
    public float? KhoangCach { get; set; }

    [CustomDataSet("ten_tuyen")]
    public string? TenTuyen { get; set; }

    [CustomDataSet("ma_do_phuc_tap")]
    public string? MaDoPhucTap { get; set; }
}

// =============================================================================
// VÉ - Ticket
// =============================================================================

/// <summary>
/// DTO cho vé (Ticket).
/// Maps to table: ve
/// </summary>
public class VeDto
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
}

// =============================================================================
// VỊ TRÍ - Position
// =============================================================================

/// <summary>
/// DTO cho vị trí (Position/Role on trip).
/// Maps to table: vi_tri
/// </summary>
public class ViTriDto
{
    [CustomDataSet("ma_vi_tri")]
    public string MaViTri { get; set; } = string.Empty;

    [CustomDataSet("vai_tro")]
    public string? VaiTro { get; set; }

    [CustomDataSet("phu_cap")]
    public decimal? PhuCap { get; set; }
}

// =============================================================================
// XE - Vehicle
// =============================================================================

/// <summary>
/// DTO cho xe (Vehicle).
/// Maps to table: xe
/// </summary>
public class XeDto
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

    [CustomDataSet("muc_tieu_hao")]
    public float? MucTieuHao { get; set; }

    [CustomDataSet("phu_thu_phi_van_hanh")]
    public decimal? PhuThuPhiVanHanh { get; set; }
}

// =============================================================================
// XE BUS NGỒI - Sitting Bus
// =============================================================================

/// <summary>
/// DTO cho xe bus ngồi (Sitting Bus).
/// Maps to table: xe_bus_ngoi
/// </summary>
public class XeBusNgoiDto
{
    [CustomDataSet("ma_xe_bus")]
    public string MaXeBus { get; set; } = string.Empty;

    [CustomDataSet("tong_tang")]
    public int TongTang { get; set; }
}

// =============================================================================
// XE GIƯỜNG NẰM - Sleeper Bus
// =============================================================================

/// <summary>
/// DTO cho xe giường nằm (Sleeper Bus).
/// Maps to table: xe_giuong_nam
/// </summary>
public class XeGiuongNamDto
{
    [CustomDataSet("ma_xe_giuong")]
    public string MaXeGiuong { get; set; } = string.Empty;

    [CustomDataSet("chieu_dai")]
    public float? ChieuDai { get; set; }

    [CustomDataSet("chieu_rong")]
    public float? ChieuRong { get; set; }
}
