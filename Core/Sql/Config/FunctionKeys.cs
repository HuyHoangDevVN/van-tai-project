namespace Core.Sql.Config;

/// <summary>
/// Static class containing all function keys used in the application.
/// Use these constants instead of hardcoded strings for type safety and IntelliSense support.
/// 
/// Naming Convention: MODULE_ACTION
/// </summary>
public static class FunctionKeys
{
    // =========================================================================
    // REPORT Module (Báo cáo)
    // =========================================================================
    public static class Report
    {
        public const string CHI_PHI_CO_BAN = "REPORT_CHI_PHI_CO_BAN";
        public const string DOANH_THU_XE_BUS_NGOI = "REPORT_DOANH_THU_XE_BUS_NGOI";
        public const string DOANH_THU_XE_BUS_NGOI_THANG = "REPORT_DOANH_THU_XE_BUS_NGOI_THANG";
        public const string GIA_VE_XE_BUS_NGOI = "REPORT_GIA_VE_XE_BUS_NGOI";
        public const string DOANH_THU_TUYEN_DUONG = "REPORT_DOANH_THU_TUYEN_DUONG";
        public const string DOANH_THU_TUYEN_DUONG_THANG = "REPORT_DOANH_THU_TUYEN_DUONG_THANG";
        public const string LUONG_THANG_TAI_XE = "REPORT_LUONG_THANG_TAI_XE";
        public const string TUYEN_DUONG_XE_GIUONG_NAM = "REPORT_TUYEN_DUONG_XE_GIUONG_NAM";
        public const string KHACH_HANG_QUAY_LAI = "REPORT_KHACH_HANG_QUAY_LAI";
    }

    // =========================================================================
    // MAINTENANCE Module (Bảo trì)
    // =========================================================================
    public static class Maintenance
    {
        public const string STATUS_CHECK = "MAINTENANCE_STATUS_CHECK";
        public const string CREATE = "MAINTENANCE_CREATE";
        public const string GET_HISTORY = "MAINTENANCE_GET_HISTORY";
        public const string UPDATE = "MAINTENANCE_UPDATE";
    }

    // =========================================================================
    // BUS Module (Quản lý xe)
    // =========================================================================
    public static class Bus
    {
        public const string SEARCH = "BUS_SEARCH";
        public const string GET_BY_ID = "BUS_GET_BY_ID";
        public const string CREATE = "BUS_CREATE";
        public const string UPDATE = "BUS_UPDATE";
        public const string DELETE = "BUS_DELETE";
        public const string GET_AVAILABLE = "BUS_GET_AVAILABLE";
    }

    // =========================================================================
    // DRIVER Module (Quản lý tài xế)
    // =========================================================================
    public static class Driver
    {
        public const string SEARCH = "DRIVER_SEARCH";
        public const string GET_BY_ID = "DRIVER_GET_BY_ID";
        public const string CREATE = "DRIVER_CREATE";
        public const string UPDATE = "DRIVER_UPDATE";
        public const string DELETE = "DRIVER_DELETE";
        public const string GET_AVAILABLE = "DRIVER_GET_AVAILABLE";
    }

    // =========================================================================
    // ROUTE Module (Quản lý tuyến đường)
    // =========================================================================
    public static class Route
    {
        public const string SEARCH = "ROUTE_SEARCH";
        public const string GET_BY_ID = "ROUTE_GET_BY_ID";
        public const string CREATE = "ROUTE_CREATE";
        public const string UPDATE = "ROUTE_UPDATE";
        public const string DELETE = "ROUTE_DELETE";
    }

    // =========================================================================
    // TRIP Module (Quản lý chuyến xe)
    // =========================================================================
    public static class Trip
    {
        public const string SEARCH = "TRIP_SEARCH";
        public const string GET_BY_ID = "TRIP_GET_BY_ID";
        public const string CREATE = "TRIP_CREATE";
        public const string UPDATE = "TRIP_UPDATE";
        public const string DELETE = "TRIP_DELETE";
        public const string COMPLETE = "TRIP_COMPLETE";
        public const string GET_BY_STATUS = "TRIP_GET_BY_STATUS";
    }

    // =========================================================================
    // TICKET Module (Quản lý vé)
    // =========================================================================
    public static class Ticket
    {
        public const string SEARCH = "TICKET_SEARCH";
        public const string GET_BY_ID = "TICKET_GET_BY_ID";
        public const string BOOK = "TICKET_BOOK";
        public const string CANCEL = "TICKET_CANCEL";
        public const string GET_BY_TRIP = "TICKET_GET_BY_TRIP";
        public const string UPDATE_STATUS = "TICKET_UPDATE_STATUS";
    }

    // =========================================================================
    // CUSTOMER Module (Quản lý khách hàng)
    // =========================================================================
    public static class Customer
    {
        public const string SEARCH = "CUSTOMER_SEARCH";
        public const string GET_BY_ID = "CUSTOMER_GET_BY_ID";
        public const string CREATE = "CUSTOMER_CREATE";
        public const string UPDATE = "CUSTOMER_UPDATE";
    }
}
