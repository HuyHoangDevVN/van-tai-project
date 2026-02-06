using Application.DTOs.VanTai;
using Application.Models;
using Application.Services.VanTai;
using Core.Sql.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// API Controller for TaiXe (Driver) management.
/// Route: /api/tai-xe
/// </summary>
[ApiController]
[Route("api/tai-xe")]
[Produces("application/json")]
public class TaiXeController : ControllerBase
{
    private readonly ITaiXeService _service;
    private readonly ILogger<TaiXeController> _logger;

    public TaiXeController(ITaiXeService service, ILogger<TaiXeController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// T?m ki?m tài x? v?i phân trang và l?c.
    /// </summary>
    [HttpGet("tim-kiem")]
    [ProducesResponseType(typeof(BaseResponse<TPaging<DriverSearchResultDto>>), 200)]
    public async Task<IActionResult> Search([FromQuery] DriverSearchReq request, CancellationToken cancellationToken)
    {
        var result = await _service.SearchAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{maTaiXe}")]
    public async Task<IActionResult> GetById(string maTaiXe, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(maTaiXe, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaiXeRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return result.Success ? CreatedAtAction(nameof(GetById), new { maTaiXe = request.MaTaiXe }, result) : ToActionResult(result);
    }

    [HttpPut("{maTaiXe}")]
    public async Task<IActionResult> Update(string maTaiXe, [FromBody] TaiXeRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(maTaiXe, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{maTaiXe}")]
    public async Task<IActionResult> Delete(string maTaiXe, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(maTaiXe, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(BaseResponse<T> response)
    {
        if (response.Success) return Ok(response);
        return response.ErrorCode switch
        {
            SqlConstants.ResponseCode_NotFound => NotFound(response),
            SqlConstants.ResponseCode_ValidationError => BadRequest(response),
            SqlConstants.ResponseCode_Duplicate => BadRequest(response),
            _ => StatusCode(500, response)
        };
    }
}

/// <summary>
/// API Controller for Xe (Vehicle) management.
/// Route: /api/xe
/// </summary>
[ApiController]
[Route("api/xe")]
[Produces("application/json")]
public class XeController : ControllerBase
{
    private readonly IXeService _service;

    public XeController(IXeService service) => _service = service;

    /// <summary>
    /// T?m ki?m xe v?i phân trang và l?c.
    /// </summary>
    [HttpGet("tim-kiem")]
    [ProducesResponseType(typeof(BaseResponse<TPaging<BusSearchResultDto>>), 200)]
    public async Task<IActionResult> Search([FromQuery] BusSearchReq request, CancellationToken ct)
        => ToActionResult(await _service.SearchAsync(request, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ToActionResult(await _service.GetAllAsync(ct));

    [HttpGet("{maXe}")]
    public async Task<IActionResult> GetById(string maXe, CancellationToken ct) => ToActionResult(await _service.GetByIdAsync(maXe, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] XeRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return result.Success ? CreatedAtAction(nameof(GetById), new { maXe = request.MaXe }, result) : ToActionResult(result);
    }

    [HttpPut("{maXe}")]
    public async Task<IActionResult> Update(string maXe, [FromBody] XeRequest request, CancellationToken ct) => ToActionResult(await _service.UpdateAsync(maXe, request, ct));

    [HttpDelete("{maXe}")]
    public async Task<IActionResult> Delete(string maXe, CancellationToken ct) => ToActionResult(await _service.DeleteAsync(maXe, ct));

    private IActionResult ToActionResult<T>(BaseResponse<T> r) => r.Success ? Ok(r) : r.ErrorCode == SqlConstants.ResponseCode_NotFound ? NotFound(r) : BadRequest(r);
}

/// <summary>
/// API Controller for ChuyenXe (Trip) management.
/// Route: /api/chuyen-xe
/// 
/// Includes business operations: Hoàn thành chuy?n, H?y chuy?n.
/// </summary>
[ApiController]
[Route("api/chuyen-xe")]
[Produces("application/json")]
public class ChuyenXeController : ControllerBase
{
    private readonly IChuyenXeService _service;
    private readonly ILogger<ChuyenXeController> _logger;

    public ChuyenXeController(IChuyenXeService service, ILogger<ChuyenXeController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// T?m ki?m chuy?n xe v?i phân trang và l?c.
    /// </summary>
    [HttpGet("tim-kiem")]
    [ProducesResponseType(typeof(BaseResponse<TPaging<TripSearchResultDto>>), 200)]
    public async Task<IActionResult> Search([FromQuery] TripSearchReq request, CancellationToken ct)
        => ToActionResult(await _service.SearchAsync(request, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ToActionResult(await _service.GetAllAsync(ct));

    [HttpGet("{maChuyen}")]
    public async Task<IActionResult> GetById(string maChuyen, CancellationToken ct) => ToActionResult(await _service.GetByIdAsync(maChuyen, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ChuyenXeRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return result.Success ? CreatedAtAction(nameof(GetById), new { maChuyen = request.MaChuyen }, result) : ToActionResult(result);
    }

    [HttpPut("{maChuyen}")]
    public async Task<IActionResult> Update(string maChuyen, [FromBody] ChuyenXeRequest request, CancellationToken ct) => ToActionResult(await _service.UpdateAsync(maChuyen, request, ct));

    [HttpDelete("{maChuyen}")]
    public async Task<IActionResult> Delete(string maChuyen, CancellationToken ct) => ToActionResult(await _service.DeleteAsync(maChuyen, ct));

    /// <summary>
    /// Hoàn thành chuy?n xe - C?p nh?t km b?o tr?.
    /// </summary>
    /// <remarks>
    /// **CRITICAL OPERATION** - Triggers maintenance data update:
    /// 1. C?p nh?t tr?ng thái chuy?n -> Completed
    /// 2. Tính toán km v?n hành: KM tãng = QuangDuong × HeSoKhoKhan
    /// 3. C?p nh?t xe.tong_km_van_hanh += KM tãng
    /// 4. C?p nh?t tai_xe.tong_so_chuyen += 1
    /// </remarks>
    [HttpPut("{maChuyen}/hoan-thanh")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> HoanThanh(
        [FromRoute] string maChuyen,
        CancellationToken ct = default)
    {
        _logger.LogInformation("API: Hoàn thành chuy?n {MaChuyen}", maChuyen);

        var result = await _service.HoanThanhAsync(maChuyen, ct);

        return result.Success ? Ok(result)
            : result.ErrorCode == SqlConstants.ResponseCode_NotFound ? NotFound(result)
            : BadRequest(result);
    }

    /// <summary>
    /// H?y chuy?n xe.
    /// </summary>
    /// <remarks>
    /// Ch? có th? h?y chuy?n có tr?ng thái "Scheduled".
    /// </remarks>
    [HttpPut("{maChuyen}/huy")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> HuyChuyen(
        [FromRoute] string maChuyen,
        CancellationToken ct = default)
    {
        _logger.LogInformation("API: H?y chuy?n {MaChuyen}", maChuyen);

        var result = await _service.HuyChuyen(maChuyen, ct);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    private IActionResult ToActionResult<T>(BaseResponse<T> r) => r.Success ? Ok(r) : r.ErrorCode == SqlConstants.ResponseCode_NotFound ? NotFound(r) : BadRequest(r);
}

/// <summary>
/// API Controller for KhachHang (Customer) management.
/// Route: /api/khach-hang
/// </summary>
[ApiController]
[Route("api/khach-hang")]
[Produces("application/json")]
public class KhachHangController : ControllerBase
{
    private readonly IKhachHangService _service;

    public KhachHangController(IKhachHangService service) => _service = service;

    /// <summary>
    /// T?m ki?m khách hàng v?i phân trang và l?c.
    /// </summary>
    [HttpGet("tim-kiem")]
    [ProducesResponseType(typeof(BaseResponse<TPaging<CustomerSearchResultDto>>), 200)]
    public async Task<IActionResult> Search([FromQuery] CustomerSearchReq request, CancellationToken ct)
        => ToActionResult(await _service.SearchAsync(request, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ToActionResult(await _service.GetAllAsync(ct));

    [HttpGet("{maKhach}")]
    public async Task<IActionResult> GetById(string maKhach, CancellationToken ct) => ToActionResult(await _service.GetByIdAsync(maKhach, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] KhachHangRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return result.Success ? CreatedAtAction(nameof(GetById), new { maKhach = request.MaKhach }, result) : ToActionResult(result);
    }

    [HttpPut("{maKhach}")]
    public async Task<IActionResult> Update(string maKhach, [FromBody] KhachHangRequest request, CancellationToken ct) => ToActionResult(await _service.UpdateAsync(maKhach, request, ct));

    [HttpDelete("{maKhach}")]
    public async Task<IActionResult> Delete(string maKhach, CancellationToken ct) => ToActionResult(await _service.DeleteAsync(maKhach, ct));

    private IActionResult ToActionResult<T>(BaseResponse<T> r) => r.Success ? Ok(r) : r.ErrorCode == SqlConstants.ResponseCode_NotFound ? NotFound(r) : BadRequest(r);
}

/// <summary>
/// API Controller for TuyenDuong (Route) management.
/// Route: /api/tuyen-duong
/// </summary>
[ApiController]
[Route("api/tuyen-duong")]
[Produces("application/json")]
public class TuyenDuongController : ControllerBase
{
    private readonly ITuyenDuongService _service;

    public TuyenDuongController(ITuyenDuongService service) => _service = service;

    /// <summary>
    /// T?m ki?m tuy?n ðý?ng v?i phân trang và l?c.
    /// </summary>
    [HttpGet("tim-kiem")]
    [ProducesResponseType(typeof(BaseResponse<TPaging<RouteSearchResultDto>>), 200)]
    public async Task<IActionResult> Search([FromQuery] RouteSearchReq request, CancellationToken ct)
        => ToActionResult(await _service.SearchAsync(request, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ToActionResult(await _service.GetAllAsync(ct));

    [HttpGet("{maTuyen}")]
    public async Task<IActionResult> GetById(string maTuyen, CancellationToken ct) => ToActionResult(await _service.GetByIdAsync(maTuyen, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TuyenDuongRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return result.Success ? CreatedAtAction(nameof(GetById), new { maTuyen = request.MaTuyen }, result) : ToActionResult(result);
    }

    [HttpPut("{maTuyen}")]
    public async Task<IActionResult> Update(string maTuyen, [FromBody] TuyenDuongRequest request, CancellationToken ct) => ToActionResult(await _service.UpdateAsync(maTuyen, request, ct));

    [HttpDelete("{maTuyen}")]
    public async Task<IActionResult> Delete(string maTuyen, CancellationToken ct) => ToActionResult(await _service.DeleteAsync(maTuyen, ct));

    private IActionResult ToActionResult<T>(BaseResponse<T> r) => r.Success ? Ok(r) : r.ErrorCode == SqlConstants.ResponseCode_NotFound ? NotFound(r) : BadRequest(r);
}

/// <summary>
/// API Controller for Ve (Ticket) management.
/// Route: /api/ve
/// 
/// Includes business operations: Ð?t vé, H?y vé.
/// </summary>
[ApiController]
[Route("api/ve")]
[Produces("application/json")]
public class VeController : ControllerBase
{
    private readonly IVeService _service;
    private readonly ILogger<VeController> _logger;

    public VeController(IVeService service, ILogger<VeController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// T?m ki?m vé v?i phân trang và l?c.
    /// </summary>
    [HttpGet("tim-kiem")]
    [ProducesResponseType(typeof(BaseResponse<TPaging<TicketSearchResultDto>>), 200)]
    public async Task<IActionResult> Search([FromQuery] TicketSearchReq request, CancellationToken ct)
        => ToActionResult(await _service.SearchAsync(request, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ToActionResult(await _service.GetAllAsync(ct));

    [HttpGet("{stt:int}")]
    public async Task<IActionResult> GetById(int stt, CancellationToken ct) => ToActionResult(await _service.GetByIdAsync(stt, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VeRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return result.Success ? Ok(result) : ToActionResult(result);
    }

    [HttpPut("{stt:int}")]
    public async Task<IActionResult> Update(int stt, [FromBody] VeRequest request, CancellationToken ct) => ToActionResult(await _service.UpdateAsync(stt, request, ct));

    [HttpDelete("{stt:int}")]
    public async Task<IActionResult> Delete(int stt, CancellationToken ct) => ToActionResult(await _service.DeleteAsync(stt, ct));

    /// <summary>
    /// Ð?t vé cho khách hàng.
    /// </summary>
    /// <remarks>
    /// **Constraint Check**: Stored procedure ki?m tra:
    /// - Chuy?n xe t?n t?i và ðang ? tr?ng thái có th? ð?t (Scheduled)
    /// - S? vé ð? bán &lt; S? gh? c?a xe (capacity check)
    /// - Gh?/giý?ng c? th? chýa ðý?c ð?t (n?u có ch? ð?nh)
    /// </remarks>
    [HttpPost("dat-ve")]
    [ProducesResponseType(typeof(BaseResponse<int>), 200)]
    [ProducesResponseType(typeof(BaseResponse<int>), 400)]
    public async Task<IActionResult> DatVe(
        [FromBody] TicketBookingRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("API: Ð?t vé cho khách {MaKhach}, chuy?n {MaChuyen}",
            request.MaKhach, request.MaChuyen);

        var result = await _service.DatVeAsync(request, ct);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// H?y vé.
    /// </summary>
    /// <remarks>
    /// Ch? có th? h?y vé khi:
    /// - Vé chýa ðý?c h?y trý?c ðó
    /// - Chuy?n xe chýa hoàn thành
    /// </remarks>
    [HttpPut("{stt:int}/huy")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> HuyVe(
        [FromRoute] int stt,
        CancellationToken ct = default)
    {
        _logger.LogInformation("API: H?y vé {Stt}", stt);

        var result = await _service.HuyVeAsync(stt, ct);

        return result.Success ? Ok(result)
            : result.ErrorCode == SqlConstants.ResponseCode_NotFound ? NotFound(result)
            : BadRequest(result);
    }

    /// <summary>
    /// L?y danh sách vé theo chuy?n xe.
    /// </summary>
    [HttpGet("theo-chuyen/{maChuyen}")]
    [ProducesResponseType(typeof(BaseResponse<List<VeDto>>), 200)]
    public async Task<IActionResult> GetByChuyen(
        [FromRoute] string maChuyen,
        CancellationToken ct = default)
    {
        var result = await _service.GetByChuyenAsync(maChuyen, ct);
        return Ok(result);
    }

    private IActionResult ToActionResult<T>(BaseResponse<T> r) => r.Success ? Ok(r) : r.ErrorCode == SqlConstants.ResponseCode_NotFound ? NotFound(r) : BadRequest(r);
}
