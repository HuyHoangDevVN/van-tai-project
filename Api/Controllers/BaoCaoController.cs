using Application.DTOs.VanTai;
using Application.Services.VanTai;
using Core.Sql.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// API Controller for Reports (Báo Cáo).
/// Exposes stored procedure-based reports as REST endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BaoCaoController : ControllerBase
{
    private readonly IBaoCaoService _baoCaoService;
    private readonly ILogger<BaoCaoController> _logger;

    public BaoCaoController(
        IBaoCaoService baoCaoService,
        ILogger<BaoCaoController> logger)
    {
        _baoCaoService = baoCaoService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy chi phí cơ bản của tất cả chuyến xe.
    /// Calls: proc_chi_phi_co_ban
    /// </summary>
    /// <returns>Danh sách chi phí cơ bản.</returns>
    [HttpGet("chi-phi-co-ban")]
    [ProducesResponseType(typeof(BaseResponse<List<ChiPhiCoBanDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChiPhiCoBan(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/baocao/chi-phi-co-ban");

        var result = await _baoCaoService.GetChiPhiCoBanAsync(cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Lấy doanh thu xe bus ngồi theo tháng.
    /// Calls: proc_doanh_thu_xe_bus_ngoi_thang(p_tu, p_den)
    /// </summary>
    /// <param name="tuNgay">Ngày bắt đầu (yyyy-MM-dd)</param>
    /// <param name="denNgay">Ngày kết thúc (yyyy-MM-dd)</param>
    /// <returns>Doanh thu theo tháng.</returns>
    [HttpGet("doanh-thu-xe-bus-ngoi")]
    [ProducesResponseType(typeof(BaseResponse<List<DoanhThuXeBusNgoiThangDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDoanhThuXeBusNgoiThang(
        [FromQuery] DateTime tuNgay,
        [FromQuery] DateTime denNgay,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/baocao/doanh-thu-xe-bus-ngoi - From: {From}, To: {To}", tuNgay, denNgay);

        if (tuNgay > denNgay)
        {
            return BadRequest(BaseResponse<object>.ValidationError("Ngày bắt đầu phải nhỏ hơn ngày kết thúc"));
        }

        var request = new DateRangeRequest { TuNgay = tuNgay, DenNgay = denNgay };
        var result = await _baoCaoService.GetDoanhThuXeBusNgoiThangAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Lấy giá vé xe bus ngồi.
    /// Calls: proc_gia_ve_xe_bus_ngoi
    /// </summary>
    /// <returns>Danh sách giá vé.</returns>
    [HttpGet("gia-ve-xe-bus-ngoi")]
    [ProducesResponseType(typeof(BaseResponse<List<GiaVeXeBusNgoiDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGiaVeXeBusNgoi(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/baocao/gia-ve-xe-bus-ngoi");

        var result = await _baoCaoService.GetGiaVeXeBusNgoiAsync(cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Lấy doanh thu tuyến đường theo tháng.
    /// Calls: proc_tinh_doanh_thu_tuyen_duong_theo_thang(p_tu, p_den)
    /// </summary>
    /// <param name="tuNgay">Ngày bắt đầu</param>
    /// <param name="denNgay">Ngày kết thúc</param>
    /// <returns>Doanh thu tuyến đường theo tháng.</returns>
    [HttpGet("doanh-thu-tuyen-duong")]
    [ProducesResponseType(typeof(BaseResponse<List<DoanhThuTuyenDuongThangDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDoanhThuTuyenDuongThang(
        [FromQuery] DateTime tuNgay,
        [FromQuery] DateTime denNgay,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/baocao/doanh-thu-tuyen-duong - From: {From}, To: {To}", tuNgay, denNgay);

        if (tuNgay > denNgay)
        {
            return BadRequest(BaseResponse<object>.ValidationError("Ngày bắt đầu phải nhỏ hơn ngày kết thúc"));
        }

        var request = new DateRangeRequest { TuNgay = tuNgay, DenNgay = denNgay };
        var result = await _baoCaoService.GetDoanhThuTuyenDuongThangAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Tính lương tháng tài xế.
    /// Calls: proc_tinh_luong_thang(p_tu, p_den)
    /// </summary>
    /// <param name="tuNgay">Ngày bắt đầu</param>
    /// <param name="denNgay">Ngày kết thúc</param>
    /// <returns>Bảng lương tài xế.</returns>
    [HttpGet("luong-thang-tai-xe")]
    [ProducesResponseType(typeof(BaseResponse<List<LuongThangTaiXeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLuongThangTaiXe(
        [FromQuery] DateTime tuNgay,
        [FromQuery] DateTime denNgay,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/baocao/luong-thang-tai-xe - From: {From}, To: {To}", tuNgay, denNgay);

        if (tuNgay > denNgay)
        {
            return BadRequest(BaseResponse<object>.ValidationError("Ngày bắt đầu phải nhỏ hơn ngày kết thúc"));
        }

        var request = new DateRangeRequest { TuNgay = tuNgay, DenNgay = denNgay };
        var result = await _baoCaoService.GetLuongThangTaiXeAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(BaseResponse<T> response)
    {
        if (response.Success)
        {
            return Ok(response);
        }

        return response.ErrorCode switch
        {
            SqlConstants.ResponseCode_NotFound => NotFound(response),
            SqlConstants.ResponseCode_ValidationError => BadRequest(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}
