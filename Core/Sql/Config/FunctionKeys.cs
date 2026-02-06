namespace Core.Sql.Config;

/// <summary>
/// Static class containing all function keys used in the application.
/// Use these constants instead of hardcoded strings for type safety and IntelliSense support.
/// 
/// Naming Convention: proc_hanh_dong_voi_cai_gi
/// Example: proc_tinh_chi_phi_co_ban
/// </summary>
public static class FunctionKeys
{
    // =========================================================================
    // REPORT Module (Báo cáo)
    // =========================================================================
    public static class Report
    {
        public const string CHI_PHI_CO_BAN = "proc_chi_phi_co_ban";
        public const string DOANH_THU_XE_BUS_NGOI = "proc_doanh_thu_xe_bus_ngoi";
        public const string DOANH_THU_XE_BUS_NGOI_THANG = "proc_doanh_thu_xe_bus_ngoi_thang";
        public const string GIA_VE_XE_BUS_NGOI = "proc_gia_ve_xe_bus_ngoi";
        public const string DOANH_THU_TUYEN_DUONG = "proc_tinh_doanh_thu_tuyen_duong";
        public const string DOANH_THU_TUYEN_DUONG_THANG = "proc_tinh_doanh_thu_tuyen_duong_theo_thang";
        public const string LUONG_THANG_TAI_XE = "proc_tinh_luong_thang";
        public const string TUYEN_DUONG_XE_GIUONG_NAM = "proc_lay_tuyen_duong_xe_giuong_nam";
        public const string KHACH_HANG_QUAY_LAI = "proc_lay_khach_hang_quay_lai";
    }

    // =========================================================================
    // MAINTENANCE Module (Bảo trì)
    // =========================================================================
    public static class Maintenance
    {
        public const string STATUS_CHECK = "proc_kiem_tra_trang_thai_bao_tri";
        public const string CREATE = "proc_tao_bao_tri";
        public const string GET_HISTORY = "proc_lay_lich_su_bao_tri";
        public const string UPDATE = "proc_cap_nhat_bao_tri";
    }

    // =========================================================================
    // BUS Module (Quản lý xe)
    // =========================================================================
    public static class Bus
    {
        public const string SEARCH = "proc_tim_kiem_xe";
        public const string GET_BY_ID = "proc_lay_xe_theo_id";
        public const string CREATE = "proc_tao_xe";
        public const string UPDATE = "proc_cap_nhat_xe";
        public const string DELETE = "proc_xoa_xe";
        public const string GET_AVAILABLE = "proc_lay_xe_san_sang";
    }

    // =========================================================================
    // DRIVER Module (Quản lý tài xế)
    // =========================================================================
    public static class Driver
    {
        public const string SEARCH = "proc_tim_kiem_tai_xe";
        public const string GET_BY_ID = "proc_lay_tai_xe_theo_id";
        public const string CREATE = "proc_tao_tai_xe";
        public const string UPDATE = "proc_cap_nhat_tai_xe";
        public const string DELETE = "proc_xoa_tai_xe";
        public const string GET_AVAILABLE = "proc_lay_tai_xe_san_sang";
    }

    // =========================================================================
    // ROUTE Module (Quản lý tuyến đường)
    // =========================================================================
    public static class Route
    {
        public const string SEARCH = "proc_tim_kiem_tuyen_duong";
        public const string GET_BY_ID = "proc_lay_tuyen_duong_theo_id";
        public const string CREATE = "proc_tao_tuyen_duong";
        public const string UPDATE = "proc_cap_nhat_tuyen_duong";
        public const string DELETE = "proc_xoa_tuyen_duong";
    }

    // =========================================================================
    // TRIP Module (Quản lý chuyến xe)
    // =========================================================================
    public static class Trip
    {
        public const string SEARCH = "proc_tim_kiem_chuyen_xe";
        public const string GET_BY_ID = "proc_lay_chuyen_xe_theo_id";
        public const string CREATE = "proc_tao_chuyen_xe";
        public const string UPDATE = "proc_cap_nhat_chuyen_xe";
        public const string DELETE = "proc_xoa_chuyen_xe";
        public const string COMPLETE = "proc_hoan_thanh_chuyen_xe";
        public const string GET_BY_STATUS = "proc_lay_chuyen_xe_theo_trang_thai";
    }

    // =========================================================================
    // TICKET Module (Quản lý vé)
    // =========================================================================
    public static class Ticket
    {
        public const string SEARCH = "proc_tim_kiem_ve";
        public const string GET_BY_ID = "proc_lay_ve_theo_id";
        public const string BOOK = "proc_dat_ve";
        public const string CANCEL = "proc_huy_ve";
        public const string GET_BY_TRIP = "proc_lay_ve_theo_chuyen";
        public const string UPDATE_STATUS = "proc_cap_nhat_trang_thai_ve";
    }

    // =========================================================================
    // CUSTOMER Module (Quản lý khách hàng)
    // =========================================================================
    public static class Customer
    {
        public const string SEARCH = "proc_tim_kiem_khach_hang";
        public const string GET_BY_ID = "proc_lay_khach_hang_theo_id";
        public const string CREATE = "proc_tao_khach_hang";
        public const string UPDATE = "proc_cap_nhat_khach_hang";
    }
}
