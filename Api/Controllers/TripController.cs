using Application.DTOs.VanTai;
using Application.Services.VanTai;
using Core.Sql.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for Trip Management (Quản lý chuyến xe).
/// 
/// CRITICAL: CompleteTripAsync affects maintenance tracking data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TripController : ControllerBase
{
    private readonly ITripService _tripService;
    private readonly ILogger<TripController> _logger;

    public TripController(ITripService tripService, ILogger<TripController> logger)
    {
        _tripService = tripService;
        _logger = logger;
    }

    /// <summary>
    /// Tìm kiếm chuyến xe theo các tiêu chí.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/Trip/search?keyword=Ha%20Noi&amp;trangThai=Scheduled
    ///     
    /// Tất cả tham số đều không bắt buộc - trả về toàn bộ nếu để trống.
    /// </remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(BaseResponse<List<TripSearchResultDto>>), 200)]
    public async Task<IActionResult> SearchTrips(
        [FromQuery] TripSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _tripService.SearchTripsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tạo chuyến xe mới.
    /// </summary>
    /// <remarks>
    /// Stored procedure sẽ kiểm tra:
    /// - Xe đang ở trạng thái Active
    /// - Xe không có chuyến khác trong cùng khung giờ
    /// - Tuyến đường tồn tại
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CreateTrip(
        [FromBody] TripCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _tripService.CreateTripAsync(request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Hoàn thành chuyến xe - Cập nhật km bảo trì.
    /// </summary>
    /// <remarks>
    /// **CRITICAL OPERATION** - Triggers maintenance data update:
    /// 1. Cập nhật trạng thái chuyến -> Completed
    /// 2. Tính toán km vận hành: KM tăng = QuangDuong × HeSoKhoKhan
    /// 3. Cập nhật xe.tong_km_van_hanh += KM tăng
    /// 4. Cập nhật tai_xe.tong_so_chuyen += 1
    /// 
    /// Dữ liệu này ảnh hưởng đến kiểm tra bảo trì (360 ngày/km threshold).
    /// </remarks>
    [HttpPut("{maChuyen}/complete")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> CompleteTrip(
        [FromRoute] string maChuyen,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API: Completing trip {MaChuyen}", maChuyen);

        var result = await _tripService.CompleteTripAsync(maChuyen, cancellationToken);

        return result.Success ? Ok(result)
            : result.ErrorCode == -3 ? NotFound(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Hủy chuyến xe.
    /// </summary>
    /// <remarks>
    /// Chỉ có thể hủy chuyến có trạng thái "Scheduled".
    /// </remarks>
    [HttpDelete("{maChuyen}")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CancelTrip(
        [FromRoute] string maChuyen,
        CancellationToken cancellationToken = default)
    {
        var result = await _tripService.CancelTripAsync(maChuyen, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
