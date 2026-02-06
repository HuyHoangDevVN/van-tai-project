using Application.DTOs.VanTai;
using Application.Services.VanTai;
using Core.Sql.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for Maintenance Management (Quản lý bảo trì xe).
/// 
/// Maintenance Algorithm:
/// - Xe cần bảo trì nếu > 360 ngày kể từ lần bảo trì cuối
/// - HOẶC tong_km_van_hanh vượt ngưỡng cấu hình
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(
        IMaintenanceService maintenanceService,
        ILogger<MaintenanceController> logger)
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
    [HttpGet("status")]
    [ProducesResponseType(typeof(BaseResponse<List<MaintenanceStatusDto>>), 200)]
    public async Task<IActionResult> GetMaintenanceStatus(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API: Checking maintenance status for all vehicles");

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
    ///     POST /api/Maintenance
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
    public async Task<IActionResult> CreateMaintenance(
        [FromBody] MaintenanceCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API: Creating maintenance for vehicle {MaXe}", request.MaXe);

        var result = await _maintenanceService.CreateMaintenanceAsync(request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Lấy lịch sử bảo trì của xe.
    /// </summary>
    [HttpGet("history/{maXe}")]
    [ProducesResponseType(typeof(BaseResponse<List<BaoTriDto>>), 200)]
    public async Task<IActionResult> GetMaintenanceHistory(
        [FromRoute] string maXe,
        CancellationToken cancellationToken = default)
    {
        var result = await _maintenanceService.GetMaintenanceHistoryAsync(maXe, cancellationToken);
        return Ok(result);
    }
}
