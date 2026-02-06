using Application.DTOs.VanTai;
using Application.Services.VanTai;
using Core.Sql.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Universal Search Controller.
/// 
/// Pattern: All search endpoints accept optional parameters.
/// NULL parameters are handled by SP with OR logic.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ITripService _tripService;

    public SearchController(ISearchService searchService, ITripService tripService)
    {
        _searchService = searchService;
        _tripService = tripService;
    }

    /// <summary>
    /// Tìm kiếm xe buýt.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/Search/buses?keyword=Toyota&amp;status=Active
    /// </remarks>
    [HttpGet("buses")]
    [ProducesResponseType(typeof(BaseResponse<List<BusSearchResultDto>>), 200)]
    public async Task<IActionResult> SearchBuses(
        [FromQuery] BusSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _searchService.SearchBusesAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tìm kiếm tài xế.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/Search/drivers?keyword=Nguyen&amp;gioiTinh=Nam
    /// </remarks>
    [HttpGet("drivers")]
    [ProducesResponseType(typeof(BaseResponse<List<DriverSearchResultDto>>), 200)]
    public async Task<IActionResult> SearchDrivers(
        [FromQuery] DriverSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _searchService.SearchDriversAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tìm kiếm tuyến đường.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/Search/routes?diemDi=Ha%20Noi&amp;diemDen=Sai%20Gon
    /// </remarks>
    [HttpGet("routes")]
    [ProducesResponseType(typeof(BaseResponse<List<RouteSearchResultDto>>), 200)]
    public async Task<IActionResult> SearchRoutes(
        [FromQuery] RouteSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _searchService.SearchRoutesAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tìm kiếm chuyến xe.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/Search/trips?keyword=CX001&amp;trangThai=Scheduled&amp;dateFrom=2024-01-01
    /// </remarks>
    [HttpGet("trips")]
    [ProducesResponseType(typeof(BaseResponse<List<TripSearchResultDto>>), 200)]
    public async Task<IActionResult> SearchTrips(
        [FromQuery] TripSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        // Delegate to TripService which also has search
        var result = await _tripService.SearchTripsAsync(request, cancellationToken);
        return Ok(result);
    }
}
