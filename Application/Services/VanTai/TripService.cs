using Application.DTOs.VanTai;
using Core.Sql.Config;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services.VanTai;

/// <summary>
/// Interface for Trip Service (Quản lý chuyến xe).
/// </summary>
public interface ITripService
{
    /// <summary>
    /// Tìm kiếm chuyến xe theo các tiêu chí.
    /// </summary>
    Task<BaseResponse<List<TripSearchResultDto>>> SearchTripsAsync(
        TripSearchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo chuyến xe mới (kiểm tra xe & tài xế khả dụng).
    /// </summary>
    Task<BaseResponse<object>> CreateTripAsync(
        TripCreateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hoàn thành chuyến xe - CRITICAL: Cập nhật km vận hành cho xe.
    /// </summary>
    Task<BaseResponse<object>> CompleteTripAsync(
        string maChuyen,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy chuyến xe.
    /// </summary>
    Task<BaseResponse<object>> CancelTripAsync(
        string maChuyen,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Trip Service implementation using Dynamic SP Resolution.
/// 
/// CRITICAL: CompleteTripAsync triggers maintenance algorithm update.
/// When a trip is completed:
/// 1. Trip status -> Completed
/// 2. Bus.TotalWorkKm += RouteDistance * DifficultyCoef
/// 3. Driver.TotalTrips++
/// </summary>
public class TripService : BaseService, ITripService
{
    public TripService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<TripService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<List<TripSearchResultDto>>> SearchTripsAsync(
        TripSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Searching trips with keyword: {Keyword}, status: {Status}",
            request.Keyword, request.TrangThai);

        try
        {
            // 1. Dynamic SP resolution
            string spName = await GetProcNameAsync(FunctionKeys.Trip.SEARCH);

            // 2. Build model with nullable parameters for universal search
            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    // Nullable parameters - SP handles NULL with WHERE logic
                    SqlParamModel.Input("p_keyword", string.IsNullOrEmpty(request.Keyword) ? null : request.Keyword),
                    SqlParamModel.Input("p_trang_thai", string.IsNullOrEmpty(request.TrangThai) ? null : request.TrangThai),
                    SqlParamModel.Input("p_date_from", request.DateFrom),
                    SqlParamModel.Input("p_date_to", request.DateTo)
                ]
            };

            // 3. Execute search
            var result = await SqlService.ExecuteProceReturnAsync<TripSearchResultDto>(
                model, cancellationToken);

            Logger.LogInformation("Trip search returned {Count} results", result.Data?.Count ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            return HandleException<List<TripSearchResultDto>>(ex, "SearchTrips");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<object>> CreateTripAsync(
        TripCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate required fields
        var validation = ValidateRequired<object>(request.MaChuyen, "Mã chuyến");
        if (validation != null) return validation;

        validation = ValidateRequired<object>(request.MaXe, "Mã xe");
        if (validation != null) return validation;

        validation = ValidateRequired<object>(request.MaTuyen, "Mã tuyến");
        if (validation != null) return validation;

        // Validate time range
        if (request.ThoiGianKhoiHanh >= request.ThoiGianDen)
        {
            return BaseResponse<object>.ValidationError(
                "Thời gian khởi hành phải trước thời gian đến.");
        }

        Logger.LogInformation("Creating trip: {MaChuyen}, Bus: {MaXe}, Route: {MaTuyen}",
            request.MaChuyen, request.MaXe, request.MaTuyen);

        try
        {
            // 1. Dynamic SP resolution
            string spName = await GetProcNameAsync(FunctionKeys.Trip.CREATE);

            // 2. Build model - SP will validate:
            //    - Bus is Active
            //    - Bus not on another trip at same time
            //    - Route exists
            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_ma_chuyen", request.MaChuyen),
                    SqlParamModel.Input("p_ten_chuyen", request.TenChuyen),
                    SqlParamModel.Input("p_thoi_gian_khoi_hanh", request.ThoiGianKhoiHanh),
                    SqlParamModel.Input("p_thoi_gian_den", request.ThoiGianDen),
                    SqlParamModel.Input("p_ma_xe", request.MaXe),
                    SqlParamModel.Input("p_ma_tuyen", request.MaTuyen),
                    SqlParamModel.Output(SqlConstants.P_ResponseCode, MySqlConnector.MySqlDbType.Int32),
                    SqlParamModel.Output(SqlConstants.P_ResponseMessage, MySqlConnector.MySqlDbType.VarChar, 500)
                ]
            };

            // 3. Execute
            var result = await SqlService.ProcExecuteNonQueryAsync(model, cancellationToken);

            return MapSpResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<object>(ex, "CreateTrip");
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// CRITICAL OPERATION:
    /// This method triggers the maintenance tracking algorithm.
    /// 
    /// SQL Logic inside sp_trip_complete_update_stats:
    /// 1. Get RouteDistance and DifficultyCoef from Route
    /// 2. Calculate: AddedWorkKm = RouteDistance * DifficultyCoef
    /// 3. UPDATE xe SET tong_km_van_hanh = tong_km_van_hanh + AddedWorkKm
    /// 4. UPDATE tai_xe SET tong_so_chuyen = tong_so_chuyen + 1
    /// 
    /// The accumulated km affects sp_maintenance_status_check results.
    /// </remarks>
    public async Task<BaseResponse<object>> CompleteTripAsync(
        string maChuyen,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequired<object>(maChuyen, "Mã chuyến");
        if (validation != null) return validation;

        Logger.LogInformation("Completing trip: {MaChuyen} - This will update bus maintenance km", maChuyen);

        try
        {
            // 1. Dynamic SP resolution - This is the critical SP
            string spName = await GetProcNameAsync(FunctionKeys.Trip.COMPLETE);

            // 2. Build model
            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_ma_chuyen", maChuyen),
                    SqlParamModel.Output(SqlConstants.P_ResponseCode, MySqlConnector.MySqlDbType.Int32),
                    SqlParamModel.Output(SqlConstants.P_ResponseMessage, MySqlConnector.MySqlDbType.VarChar, 500)
                ]
            };

            // 3. Execute - SP handles:
            //    - Status update to 'Completed'
            //    - Bus km calculation
            //    - Driver trip count increment
            var result = await SqlService.ProcExecuteNonQueryAsync(model, cancellationToken);

            if (result.Success)
            {
                Logger.LogInformation(
                    "Trip {MaChuyen} completed successfully. Maintenance data updated.", maChuyen);
            }

            return MapSpResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<object>(ex, "CompleteTrip");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<object>> CancelTripAsync(
        string maChuyen,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequired<object>(maChuyen, "Mã chuyến");
        if (validation != null) return validation;

        Logger.LogInformation("Cancelling trip: {MaChuyen}", maChuyen);

        try
        {
            // Use DELETE key or fallback to raw query
            var spName = await GetProcNameOrDefaultAsync(FunctionKeys.Trip.DELETE);

            if (spName != null)
            {
                var model = new SqlExecuteModel(spName)
                {
                    IsStoredProcedure = true,
                    Params =
                    [
                        SqlParamModel.Input("p_ma_chuyen", maChuyen),
                        SqlParamModel.Output(SqlConstants.P_ResponseCode, MySqlConnector.MySqlDbType.Int32),
                        SqlParamModel.Output(SqlConstants.P_ResponseMessage, MySqlConnector.MySqlDbType.VarChar, 500)
                    ]
                };

                return await SqlService.ProcExecuteNonQueryAsync(model, cancellationToken);
            }
            else
            {
                // Fallback: Simple status update
                var model = SqlExecuteModel.RawQuery(
                    "UPDATE chuyen_xe SET trang_thai = 'Cancelled' WHERE ma_chuyen = @ma_chuyen AND trang_thai = 'Scheduled'")
                    .AddInput("ma_chuyen", maChuyen);

                var result = await SqlService.ExecuteSqlRawNonQueryAsync(model, cancellationToken);

                if (result.Success && result.Data == 0)
                {
                    return BaseResponse<object>.NotFound(
                        "Không thể hủy chuyến. Chuyến không tồn tại hoặc đã ở trạng thái khác.");
                }

                return result.Success
                    ? BaseResponse<object>.Ok("Hủy chuyến thành công")
                    : BaseResponse<object>.Error(result.Message);
            }
        }
        catch (Exception ex)
        {
            return HandleException<object>(ex, "CancelTrip");
        }
    }
}
