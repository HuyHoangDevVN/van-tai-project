using Application.DTOs.VanTai;
using Core.Sql.Config;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services.VanTai;

/// <summary>
/// Interface for Maintenance Service (Bảo trì xe).
/// </summary>
public interface IMaintenanceService
{
    /// <summary>
    /// Kiểm tra trạng thái bảo trì của tất cả xe.
    /// Logic: Xe cần bảo trì nếu > 360 ngày hoặc vượt km ngưỡng.
    /// </summary>
    Task<BaseResponse<List<MaintenanceStatusDto>>> GetMaintenanceStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm lịch bảo trì mới (reset km vận hành).
    /// </summary>
    Task<BaseResponse<object>> CreateMaintenanceAsync(
        MaintenanceCreateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy lịch sử bảo trì của xe.
    /// </summary>
    Task<BaseResponse<List<BaoTriDto>>> GetMaintenanceHistoryAsync(
        string maXe,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Maintenance Service implementation using Dynamic SP Resolution.
/// 
/// Pattern: Service -> GetProcName(FunctionKey) -> IProcedureConfigProvider (cached) -> Execute SP
/// </summary>
public class MaintenanceService : BaseService, IMaintenanceService
{
    public MaintenanceService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        ILogger<MaintenanceService> logger)
        : base(sqlService, procProvider, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<List<MaintenanceStatusDto>>> GetMaintenanceStatusAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Checking maintenance status for all vehicles");

        try
        {
            // 1. Get the actual SP name dynamically from configuration
            string spName = await GetProcNameAsync(FunctionKeys.Maintenance.STATUS_CHECK);

            // 2. Prepare SqlExecuteModel with dynamic SP name
            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true
                // This SP has no parameters - calculates 360 days / km threshold internally
            };

            // 3. Execute and return results
            var result = await SqlService.ExecuteProceReturnAsync<MaintenanceStatusDto>(
                model, cancellationToken);

            if (result.Success)
            {
                var needMaintenance = result.Data?.Count(x => x.CanBaoTri) ?? 0;
                Logger.LogInformation(
                    "Maintenance check completed. {Total} vehicles, {NeedMaintenance} need maintenance",
                    result.Data?.Count ?? 0, needMaintenance);
            }

            return result;
        }
        catch (Exception ex)
        {
            return HandleException<List<MaintenanceStatusDto>>(ex, "GetMaintenanceStatus");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<object>> CreateMaintenanceAsync(
        MaintenanceCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate
        var validation = ValidateRequired<object>(request.MaXe, "Mã xe");
        if (validation != null) return validation;

        validation = ValidateRequired<object>(request.MaBaoTri, "Mã bảo trì");
        if (validation != null) return validation;

        Logger.LogInformation("Creating maintenance record for vehicle: {MaXe}", request.MaXe);

        try
        {
            // 1. Dynamic SP resolution
            string spName = await GetProcNameAsync(FunctionKeys.Maintenance.CREATE);

            // 2. Build model with parameters matching SP signature
            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true,
                Params =
                [
                    SqlParamModel.Input("p_ma_bao_tri", request.MaBaoTri),
                    SqlParamModel.Input("p_ma_xe", request.MaXe),
                    SqlParamModel.Input("p_don_vi", request.DonVi),
                    SqlParamModel.Input("p_chi_phi", request.ChiPhi),
                    SqlParamModel.Input("p_ngay", request.Ngay),
                    SqlParamModel.Input("p_so_km", request.SoKm),
                    SqlParamModel.Output(SqlConstants.P_ResponseCode, MySqlConnector.MySqlDbType.Int32),
                    SqlParamModel.Output(SqlConstants.P_ResponseMessage, MySqlConnector.MySqlDbType.VarChar, 500)
                ]
            };

            // 3. Execute non-query (CUD operation)
            var result = await SqlService.ProcExecuteNonQueryAsync(model, cancellationToken);

            return MapSpResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<object>(ex, "CreateMaintenance");
        }
    }

    /// <inheritdoc/>
    public async Task<BaseResponse<List<BaoTriDto>>> GetMaintenanceHistoryAsync(
        string maXe,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequired<List<BaoTriDto>>(maXe, "Mã xe");
        if (validation != null) return validation;

        Logger.LogInformation("Getting maintenance history for vehicle: {MaXe}", maXe);

        try
        {
            // Check if SP is configured, fallback to raw query if not
            var spName = await GetProcNameOrDefaultAsync(FunctionKeys.Maintenance.GET_HISTORY);

            if (spName != null)
            {
                var model = new SqlExecuteModel(spName)
                {
                    IsStoredProcedure = true,
                    Params = [SqlParamModel.Input("p_ma_xe", maXe)]
                };
                return await SqlService.ExecuteProceReturnAsync<BaoTriDto>(model, cancellationToken);
            }
            else
            {
                // Fallback to raw query if SP not configured
                Logger.LogDebug("SP not configured for {Key}, using raw query", FunctionKeys.Maintenance.GET_HISTORY);
                var model = SqlExecuteModel.RawQuery(
                    "SELECT * FROM bao_tri WHERE ma_xe = @ma_xe ORDER BY ngay DESC")
                    .AddInput("ma_xe", maXe);

                return await SqlService.ExecuteSqlRawCommandAsync<BaoTriDto>(model, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            return HandleException<List<BaoTriDto>>(ex, "GetMaintenanceHistory");
        }
    }
}
