using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Core.Sql.Models;

namespace Application.Models;

/// <summary>
/// Base abstract class for all search/paging request DTOs.
/// 
/// Architecture Pattern:
/// - All search endpoints should inherit from this class
/// - Provides standardized paging, sorting, and filtering parameters
/// - Automatically maps to SP parameters via BuildBaseSearchParams()
/// 
/// Usage:
/// public class BusSearchReq : BaseSearchReq
/// {
///     public string? Status { get; set; }  // Additional filter
///     public string? BusType { get; set; } // Additional filter
/// }
/// </summary>
public abstract class BaseSearchReq
{
    /// <summary>
    /// Universal keyword search across multiple columns.
    /// SP Pattern: WHERE (@p_keyword IS NULL OR col LIKE CONCAT('%', @p_keyword, '%'))
    /// </summary>
    [JsonPropertyName("keyword")]
    [StringLength(200, ErrorMessage = "T? khóa t?m ki?m t?i ða 200 k? t?")]
    public string? Keyword { get; set; }

    /// <summary>
    /// Current page index (1-based).
    /// Default: 1
    /// </summary>
    [JsonPropertyName("pageIndex")]
    [Range(1, int.MaxValue, ErrorMessage = "Trang ph?i l?n hõn 0")]
    public int PageIndex { get; set; } = SqlConstants.DefaultPageIndex;

    /// <summary>
    /// Number of items per page.
    /// Default: 20, Max: 100
    /// </summary>
    [JsonPropertyName("pageSize")]
    [Range(1, SqlConstants.MaxPageSize, ErrorMessage = "S? lý?ng m?i trang ph?i t? 1 ð?n 100")]
    public int PageSize { get; set; } = SqlConstants.DefaultPageSize;

    /// <summary>
    /// Column name to sort by.
    /// Must match a valid column name in the SP's ORDER BY logic.
    /// Examples: "CreatedDate", "Name", "Price"
    /// </summary>
    [JsonPropertyName("sortBy")]
    [StringLength(50, ErrorMessage = "Tên c?t s?p x?p t?i ða 50 k? t?")]
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction. True = Descending, False = Ascending.
    /// Default: true (newest first)
    /// </summary>
    [JsonPropertyName("sortDesc")]
    public bool SortDesc { get; set; } = true;

    /// <summary>
    /// Filter: From date (inclusive).
    /// Used for date range filtering.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter: To date (inclusive).
    /// Used for date range filtering.
    /// </summary>
    [JsonPropertyName("toDate")]
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Calculates the OFFSET value for MySQL LIMIT/OFFSET.
    /// Formula: (PageIndex - 1) * PageSize
    /// </summary>
    [JsonIgnore]
    public int Offset => (NormalizedPageIndex - 1) * NormalizedPageSize;

    /// <summary>
    /// Returns PageSize with validation (capped at MaxPageSize).
    /// </summary>
    [JsonIgnore]
    public int NormalizedPageSize => Math.Min(
        PageSize > 0 ? PageSize : SqlConstants.DefaultPageSize,
        SqlConstants.MaxPageSize);

    /// <summary>
    /// Returns PageIndex with validation (minimum 1).
    /// </summary>
    [JsonIgnore]
    public int NormalizedPageIndex => PageIndex > 0 ? PageIndex : SqlConstants.DefaultPageIndex;

    /// <summary>
    /// Returns trimmed keyword or null if empty.
    /// </summary>
    [JsonIgnore]
    public string? NormalizedKeyword => string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim();

    /// <summary>
    /// Returns trimmed SortBy or null if empty.
    /// </summary>
    [JsonIgnore]
    public string? NormalizedSortBy => string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim();

    /// <summary>
    /// Validates date range (FromDate <= ToDate).
    /// </summary>
    public bool IsDateRangeValid()
    {
        if (FromDate.HasValue && ToDate.HasValue)
        {
            return FromDate.Value <= ToDate.Value;
        }
        return true;
    }
}

/// <summary>
/// Search request for Bus (Xe) entity with specific filters.
/// </summary>
public class BusSearchReq : BaseSearchReq
{
    /// <summary>
    /// Filter by vehicle status.
    /// Examples: "Ho?t ð?ng", "B?o tr?", "Ng?ng ho?t ð?ng"
    /// </summary>
    [JsonPropertyName("status")]
    [StringLength(30, ErrorMessage = "Tr?ng thái t?i ða 30 k? t?")]
    public string? Status { get; set; }

    /// <summary>
    /// Filter by vehicle manufacturer.
    /// Examples: "Toyota", "Hyundai"
    /// </summary>
    [JsonPropertyName("hangSanXuat")]
    [StringLength(50, ErrorMessage = "H?ng s?n xu?t t?i ða 50 k? t?")]
    public string? HangSanXuat { get; set; }

    /// <summary>
    /// Filter by bus type.
    /// Examples: "Xe gh? ng?i", "Xe giý?ng n?m"
    /// </summary>
    [JsonPropertyName("busType")]
    [StringLength(50, ErrorMessage = "Lo?i xe t?i ða 50 k? t?")]
    public string? BusType { get; set; }
}

/// <summary>
/// Search request for Driver (TaiXe) entity with specific filters.
/// </summary>
public class DriverSearchReq : BaseSearchReq
{
    /// <summary>
    /// Filter by gender.
    /// Examples: "Nam", "N?"
    /// </summary>
    [JsonPropertyName("gioiTinh")]
    [StringLength(10, ErrorMessage = "Gi?i tính t?i ða 10 k? t?")]
    public string? GioiTinh { get; set; }

    /// <summary>
    /// Filter by hometown/province.
    /// Examples: "Hà N?i", "TP.HCM"
    /// </summary>
    [JsonPropertyName("queQuan")]
    [StringLength(100, ErrorMessage = "Quê quán t?i ða 100 k? t?")]
    public string? QueQuan { get; set; }

    /// <summary>
    /// Filter by contract status (active/expired based on NgayKyHopDong).
    /// </summary>
    [JsonPropertyName("contractStatus")]
    [StringLength(20, ErrorMessage = "Tr?ng thái h?p ð?ng t?i ða 20 k? t?")]
    public string? ContractStatus { get; set; }
}

/// <summary>
/// Search request for Route (TuyenDuong) entity with specific filters.
/// </summary>
public class RouteSearchReq : BaseSearchReq
{
    /// <summary>
    /// Filter by departure point.
    /// </summary>
    [JsonPropertyName("diemDi")]
    [StringLength(100, ErrorMessage = "Ði?m ði t?i ða 100 k? t?")]
    public string? DiemDi { get; set; }

    /// <summary>
    /// Filter by destination point.
    /// </summary>
    [JsonPropertyName("diemDen")]
    [StringLength(100, ErrorMessage = "Ði?m ð?n t?i ða 100 k? t?")]
    public string? DiemDen { get; set; }

    /// <summary>
    /// Filter by complexity level.
    /// </summary>
    [JsonPropertyName("maDoPhucTap")]
    [StringLength(20, ErrorMessage = "M? ð? ph?c t?p t?i ða 20 k? t?")]
    public string? MaDoPhucTap { get; set; }
}

/// <summary>
/// Search request for Trip (ChuyenXe) entity with specific filters.
/// </summary>
public class TripSearchReq : BaseSearchReq
{
    /// <summary>
    /// Filter by route ID.
    /// </summary>
    [JsonPropertyName("maTuyen")]
    [StringLength(20, ErrorMessage = "M? tuy?n t?i ða 20 k? t?")]
    public string? MaTuyen { get; set; }

    /// <summary>
    /// Filter by vehicle ID.
    /// </summary>
    [JsonPropertyName("maXe")]
    [StringLength(20, ErrorMessage = "M? xe t?i ða 20 k? t?")]
    public string? MaXe { get; set; }

    /// <summary>
    /// Filter by trip status.
    /// Examples: "Scheduled", "InProgress", "Completed", "Cancelled"
    /// </summary>
    [JsonPropertyName("status")]
    [StringLength(30, ErrorMessage = "Tr?ng thái t?i ða 30 k? t?")]
    public string? Status { get; set; }
}

/// <summary>
/// Search request for Ticket (Ve) entity with specific filters.
/// </summary>
public class TicketSearchReq : BaseSearchReq
{
    /// <summary>
    /// Filter by customer ID.
    /// </summary>
    [JsonPropertyName("maKhach")]
    [StringLength(20, ErrorMessage = "M? khách t?i ða 20 k? t?")]
    public string? MaKhach { get; set; }

    /// <summary>
    /// Filter by trip ID.
    /// </summary>
    [JsonPropertyName("maChuyen")]
    [StringLength(20, ErrorMessage = "M? chuy?n t?i ða 20 k? t?")]
    public string? MaChuyen { get; set; }

    /// <summary>
    /// Filter by payment status.
    /// Examples: "Ð? thanh toán", "Chýa thanh toán"
    /// </summary>
    [JsonPropertyName("trangThaiTT")]
    [StringLength(30, ErrorMessage = "Tr?ng thái thanh toán t?i ða 30 k? t?")]
    public string? TrangThaiTT { get; set; }

    /// <summary>
    /// Filter by payment method.
    /// Examples: "Ti?n m?t", "Chuy?n kho?n", "Th?"
    /// </summary>
    [JsonPropertyName("phuongThucTT")]
    [StringLength(50, ErrorMessage = "Phýõng th?c thanh toán t?i ða 50 k? t?")]
    public string? PhuongThucTT { get; set; }
}

/// <summary>
/// Search request for Customer (KhachHang) entity with specific filters.
/// </summary>
public class CustomerSearchReq : BaseSearchReq
{
    /// <summary>
    /// Filter by guardian ID (for minors).
    /// </summary>
    [JsonPropertyName("maGiamHo")]
    [StringLength(20, ErrorMessage = "M? giám h? t?i ða 20 k? t?")]
    public string? MaGiamHo { get; set; }

    /// <summary>
    /// Filter by customer type.
    /// Examples: "Thý?ng xuyên", "M?i"
    /// </summary>
    [JsonPropertyName("customerType")]
    [StringLength(30, ErrorMessage = "Lo?i khách hàng t?i ða 30 k? t?")]
    public string? CustomerType { get; set; }
}
