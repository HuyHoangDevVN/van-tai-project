using Application.DTOs.VanTai;
using Application.Services.VanTai;
using Core.Sql.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for Ticket Management (Quản lý vé).
/// 
/// Key Constraint: Đặt vé kiểm tra số ghế trống.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketController> _logger;

    public TicketController(ITicketService ticketService, ILogger<TicketController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    /// <summary>
    /// Đặt vé cho khách hàng.
    /// </summary>
    /// <remarks>
    /// **Constraint Check**: Stored procedure kiểm tra:
    /// - Chuyến xe tồn tại và đang ở trạng thái có thể đặt (Scheduled)
    /// - Số vé đã bán &lt; Số ghế của xe (capacity check)
    /// - Ghế/giường cụ thể chưa được đặt (nếu có chỉ định)
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Ticket/book
    ///     {
    ///         "maKhach": "KH001",
    ///         "maChuyen": "CX001",
    ///         "phuongThucTT": "Cash",
    ///         "viTri": "A1"
    ///     }
    /// </remarks>
    [HttpPost("book")]
    [ProducesResponseType(typeof(BaseResponse<int>), 200)]
    [ProducesResponseType(typeof(BaseResponse<int>), 400)]
    public async Task<IActionResult> BookTicket(
        [FromBody] TicketBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API: Booking ticket for customer {MaKhach}, trip {MaChuyen}",
            request.MaKhach, request.MaChuyen);

        var result = await _ticketService.BookTicketAsync(request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Hủy vé.
    /// </summary>
    /// <remarks>
    /// Chỉ có thể hủy vé khi:
    /// - Vé chưa được hủy trước đó
    /// - Chuyến xe chưa hoàn thành
    /// </remarks>
    [HttpDelete("{stt}")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> CancelTicket(
        [FromRoute] int stt,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API: Cancelling ticket {Stt}", stt);

        var result = await _ticketService.CancelTicketAsync(stt, cancellationToken);

        return result.Success ? Ok(result)
            : result.ErrorCode == -3 ? NotFound(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Lấy danh sách vé theo chuyến xe.
    /// </summary>
    [HttpGet("by-trip/{maChuyen}")]
    [ProducesResponseType(typeof(BaseResponse<List<VeDto>>), 200)]
    public async Task<IActionResult> GetTicketsByTrip(
        [FromRoute] string maChuyen,
        CancellationToken cancellationToken = default)
    {
        var result = await _ticketService.GetTicketsByTripAsync(maChuyen, cancellationToken);
        return Ok(result);
    }
}
