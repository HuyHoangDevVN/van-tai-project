using Application.DTOs.VanTai;
using Core.Sql.Config;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services.VanTai;

/// <summary>
/// Interface for Ticket Service (Quản lý vé).
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// Đặt vé cho khách hàng (kiểm tra số ghế trống).
    /// </summary>
    Task<BaseResponse<int>> BookTicketAsync(
        TicketBookingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy vé.
    /// </summary>
    Task<BaseResponse<object>> CancelTicketAsync(
        int stt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách vé theo chuyến xe.
    /// </summary>
    Task<BaseResponse<List<VeDto>>> GetTicketsByTripAsync(
        string maChuyen,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Ticket Service implementation using Dynamic SP Resolution.
/// 
/// Key Constraint: Ticket booking checks seat capacity.
/// If SoldTickets >= Bus.SeatCount, booking is rejected.
/// </summary>
public class TicketService : BaseService, ITicketService
{
    public TicketService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<TicketService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Constraint Handling:
    /// The SP sp_ticket_booking checks:
    /// 1. Trip exists and is in bookable status (Scheduled/InProgress)
    /// 2. SoldTickets < Bus.SeatCount (capacity check)
    /// 3. Specific seat not already taken (if ma_ghe specified)
    /// 
    /// Returns p_response_code = -2 with appropriate message if constraint violated.
    /// </remarks>
    public async Task<BaseResponse<int>> BookTicketAsync(
        TicketBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate
        var validation = ValidateRequired<int>(request.MaKhach, "Mã khách hàng");
        if (validation != null) return validation;

        validation = ValidateRequired<int>(request.MaChuyen, "Mã chuyến");
        if (validation != null) return validation;

        Logger.LogInformation("Booking ticket for customer: {MaKhach}, trip: {MaChuyen}",
            request.MaKhach, request.MaChuyen);

        try
        {
            // 1. Dynamic SP resolution
            string spName = await GetProcNameAsync(FunctionKeys.Ticket.BOOK);

            // 2. Build model - SP will check seat capacity constraint
            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_ma_khach", request.MaKhach),
                    SqlParamModel.Input("p_ma_chuyen", request.MaChuyen),
                    SqlParamModel.Input("p_phuong_thuc_tt", request.PhuongThucTT),
                    SqlParamModel.Input("p_vi_tri", request.ViTri),
                    SqlParamModel.Input("p_ma_ghe", request.MaGhe),
                    SqlParamModel.Input("p_ma_giuong", request.MaGiuong),
                    SqlParamModel.Output(SqlConstants.P_ResponseCode, MySqlConnector.MySqlDbType.Int32),
                    SqlParamModel.Output(SqlConstants.P_ResponseMessage, MySqlConnector.MySqlDbType.VarChar, 500),
                    SqlParamModel.Output("p_stt_ve", MySqlConnector.MySqlDbType.Int32) // Returns ticket ID
                ]
            };

            // 3. Execute
            var result = await SqlService.ProcExecuteNonQueryAsync(model, cancellationToken);

            if (result.Success)
            {
                // Get the ticket ID from output parameter
                // The SP sets p_stt_ve with LAST_INSERT_ID()
                Logger.LogInformation("Ticket booked successfully for trip {MaChuyen}", request.MaChuyen);

                // Return the ticket ID (would need to be extracted from output params)
                return BaseResponse<int>.Ok(0, result.Message);
            }

            // Map SP error codes to user-friendly response
            return BaseResponse<int>.Error(result.Message, result.ErrorCode);
        }
        catch (Exception ex)
        {
            return HandleException<int>(ex, "BookTicket");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<object>> CancelTicketAsync(
        int stt,
        CancellationToken cancellationToken = default)
    {
        if (stt <= 0)
        {
            return BaseResponse<object>.ValidationError("Số thứ tự vé không hợp lệ.");
        }

        Logger.LogInformation("Cancelling ticket: {Stt}", stt);

        try
        {
            // 1. Dynamic SP resolution
            string spName = await GetProcNameAsync(FunctionKeys.Ticket.CANCEL);

            // 2. Build model - SP validates:
            //    - Ticket exists
            //    - Ticket not already cancelled
            //    - Trip not completed/in-progress
            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_stt", stt),
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
            return HandleException<object>(ex, "CancelTicket");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<List<VeDto>>> GetTicketsByTripAsync(
        string maChuyen,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequired<List<VeDto>>(maChuyen, "Mã chuyến");
        if (validation != null) return validation;

        Logger.LogInformation("Getting tickets for trip: {MaChuyen}", maChuyen);

        try
        {
            var spName = await GetProcNameOrDefaultAsync(FunctionKeys.Ticket.GET_BY_TRIP);

            if (spName != null)
            {
                var model = new SqlExecuteModel(spName)
                {
                    IsStoredProcedure = true,
                    Params = [SqlParamModel.Input("p_ma_chuyen", maChuyen)]
                };

                return await SqlService.ExecuteProceReturnAsync<VeDto>(model, cancellationToken);
            }
            else
            {
                // Fallback to raw query
                var model = SqlExecuteModel.RawQuery(
                    "SELECT * FROM ve WHERE ma_chuyen = @ma_chuyen ORDER BY thoi_gian_dat DESC")
                    .AddInput("ma_chuyen", maChuyen);

                return await SqlService.ExecuteSqlRawCommandAsync<VeDto>(model, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            return HandleException<List<VeDto>>(ex, "GetTicketsByTrip");
        }
    }
}
