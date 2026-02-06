using Application.DTOs.VanTai;
using Application.Services.VanTai;
using Core.Sql.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// API Controller for Maintenance Management (Quản lý bảo trì xe).
/// Route: /api/bao-tri
/// 
/// Maintenance Algorithm:
/// - Xe cần bảo trì nếu > 360 ngày kể từ lần bảo trì cuối
/// - HOẶC tong_km_van_hanh vượt ngưỡng cấu hình
/// </summary>
[ApiController]
[Route("api/bao-tri")]
[Produces("application/json")]
public class BaoTriController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ILogger<BaoTriController> _logger;

    public BaoTriController(
        IMaintenanceService maintenanceService,
        ILogger<BaoTriController> logger)
    {
        _maintenanceService = maintenanceService;
        _logger = logger;
    }

    /// <summary>
    /// Kiểm tra trạng thái bảo trì của tất cả xe.
    /// </summary>
    /// <remarks>
    /// **Maintenance Check Algorithm** (in SP):
    /// 
    /// ```sql
    /// can_bao_tri = (DATEDIFF(NOW(), ngay_bao_tri_cuoi) > 360)
    ///               OR (tong_km_van_hanh > nguong_km_bao_tri)
    /// ```
    /// 
    /// Returns:
    /// - MaXe, TenXe, HangSanXuat
    /// - TongKmVanHanh (total km since last maintenance)
    /// - NgayBaoTriCuoi (date of last maintenance)
    /// - SoNgayTuBaoTri (days since maintenance)
    /// - CanBaoTri (true/false - needs maintenance)
    /// </remarks>
    [HttpGet("trang-thai")]
    [ProducesResponseType(typeof(BaseResponse<List<MaintenanceStatusDto>>), 200)]
    public async Task<IActionResult> GetTrangThaiBaoTri(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API: Kiểm tra trạng thái bảo trì tất cả xe");

        var result = await _maintenanceService.GetMaintenanceStatusAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Thêm lịch bảo trì mới cho xe.
    /// </summary>
    /// <remarks>
    /// **Side Effect**: Khi thêm bảo trì:
    /// - Reset xe.tong_km_van_hanh = 0
    /// - Set xe.ngay_bao_tri_cuoi = ngay_bao_tri
    /// 
    /// Sample request:
    /// 
    ///     POST /api/bao-tri
    ///     {
    ///         "maBaoTri": "BT001",
    ///         "maXe": "XE001",
    ///         "donVi": "Garage A",
    ///         "chiPhi": 5000000,
    ///         "ngay": "2024-01-15",
    ///         "soKm": 10000
    ///     }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> ThemBaoTri(
        [FromBody] MaintenanceCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API: Thêm bảo trì cho xe {MaXe}", request.MaXe);

        var result = await _maintenanceService.CreateMaintenanceAsync(request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Lấy lịch sử bảo trì của xe.
    /// </summary>
    [HttpGet("lich-su/{maXe}")]
    [ProducesResponseType(typeof(BaseResponse<List<BaoTriDto>>), 200)]
    public async Task<IActionResult> GetLichSuBaoTri(
        [FromRoute] string maXe,
        CancellationToken cancellationToken = default)
    {
        var result = await _maintenanceService.GetMaintenanceHistoryAsync(maXe, cancellationToken);
        return Ok(result);
    }
}
