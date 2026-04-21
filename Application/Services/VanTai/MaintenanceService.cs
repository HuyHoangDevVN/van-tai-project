using Application.DTOs.VanTai;
using Core.Sql.Config;
using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services.VanTai;

public interface IMaintenanceService
{
    Task<BaseResponse<List<MaintenanceStatusDto>>> GetMaintenanceStatusAsync(
        CancellationToken cancellationToken = default);

    Task<BaseResponse<object>> CreateMaintenanceAsync(
        MaintenanceCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<BaseResponse<List<BaoTriDto>>> GetMaintenanceHistoryAsync(
        string maXe,
        CancellationToken cancellationToken = default);

    Task<BaseResponse<List<MaintenanceAlertDto>>> GetAlertsAsync(
        MaintenanceAlertQuery query,
        CancellationToken cancellationToken = default);

    Task<BaseResponse<object>> ResolveAlertAsync(
        long id,
        string? resolvedBy,
        CancellationToken cancellationToken = default);

    Task<BaseResponse<MaintenanceAlertScanResultDto>> ScanAlertsAsync(
        CancellationToken cancellationToken = default);
}

public class MaintenanceService : BaseService, IMaintenanceService
{
    private const int MaintenanceCycleDays = 360;
    private readonly IConfiguration _configuration;

    public MaintenanceService(
        ISqlExecuteService sqlService,
        IProcedureConfigProvider procProvider,
        IConfiguration configuration,
        ILogger<MaintenanceService> logger)
        : base(sqlService, procProvider, logger)
    {
        _configuration = configuration;
    }

    public async Task<BaseResponse<List<MaintenanceStatusDto>>> GetMaintenanceStatusAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Checking maintenance status for all vehicles");

        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Maintenance.STATUS_CHECK);

            var model = new SqlExecuteModel(spName)
            {
                IsStoredProcedure = true
            };

            var result = await SqlService.ExecuteProceReturnAsync<MaintenanceStatusDto>(
                model, cancellationToken);

            if (!result.Success)
            {
                return result;
            }

            var registrationDates = await GetVehicleRegistrationDatesAsync(cancellationToken);
            var warningDays = GetAlertThresholdDays();
            var today = DateTime.Today;

            foreach (var item in result.Data ?? [])
            {
                if (registrationDates.TryGetValue(item.MaXe, out var ngayDangKiem))
                {
                    item.NgayDangKiem = ngayDangKiem;
                }

                if (item.NgayBaoTriCuoi.HasValue)
                {
                    item.NgayBaoTriTiepTheo = item.NgayBaoTriCuoi.Value.Date.AddDays(MaintenanceCycleDays);
                    item.SoNgayConLai = (item.NgayBaoTriTiepTheo.Value.Date - today).Days;
                }
                else
                {
                    item.NgayBaoTriTiepTheo = null;
                    item.SoNgayConLai = -1;
                }

                if (item.NgayDangKiem.HasValue)
                {
                    item.SoNgayDenDangKiem = (item.NgayDangKiem.Value.Date - today).Days;
                    item.TrangThaiDangKiem = item.SoNgayDenDangKiem switch
                    {
                        <= 0 => "Quá hạn",
                        int days when days <= warningDays => "Sắp đến hạn",
                        _ => "Bình thường"
                    };
                }
                else
                {
                    item.SoNgayDenDangKiem = null;
                    item.TrangThaiDangKiem = "Chưa có dữ liệu";
                }

                item.TrangThaiBaoTri = ResolveMaintenanceStatus(item, warningDays);
                item.CanBaoTri = string.Equals(item.TrangThaiBaoTri, "Quá hạn", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(item.TrangThaiBaoTri, "Sắp đến hạn", StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }
        catch (Exception ex)
        {
            return HandleException<List<MaintenanceStatusDto>>(ex, "GetMaintenanceStatus");
        }
    }

    public async Task<BaseResponse<object>> CreateMaintenanceAsync(
        MaintenanceCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequired<object>(request.MaXe, "Mã xe");
        if (validation != null) return validation;

        validation = ValidateRequired<object>(request.MaBaoTri, "Mã bảo trì");
        if (validation != null) return validation;

        if (request.Ngay.Date > DateTime.Today)
        {
            return BaseResponse<object>.ValidationError("Ngày bảo trì không được lớn hơn ngày hiện tại.");
        }

        if (request.SoKm < 0 || request.ChiPhi < 0)
        {
            return BaseResponse<object>.ValidationError("Số km và chi phí phải lớn hơn hoặc bằng 0.");
        }

        if (!await RecordExistsAsync("xe", "ma_xe", request.MaXe, cancellationToken))
        {
            return BaseResponse<object>.NotFound($"Không tìm thấy xe với mã {request.MaXe}.");
        }

        Logger.LogInformation("Creating maintenance record for vehicle: {MaXe}", request.MaXe);

        try
        {
            string spName = await GetProcNameAsync(FunctionKeys.Maintenance.CREATE);

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

            var result = await SqlService.ProcExecuteNonQueryAsync(model, cancellationToken);
            var response = MapSpResponse(result);

            if (response.Success)
            {
                await EnsureAlertTableAsync(cancellationToken);
                await ResolvePendingAlertsForVehicleAsync(request.MaXe, cancellationToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            return HandleException<object>(ex, "CreateMaintenance");
        }
    }

    public async Task<BaseResponse<List<BaoTriDto>>> GetMaintenanceHistoryAsync(
        string maXe,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequired<List<BaoTriDto>>(maXe, "Mã xe");
        if (validation != null) return validation;

        Logger.LogInformation("Getting maintenance history for vehicle: {MaXe}", maXe);

        try
        {
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

            var query = SqlExecuteModel.RawQuery(
                    "SELECT * FROM bao_tri WHERE ma_xe = @ma_xe ORDER BY ngay DESC")
                .AddInput("ma_xe", maXe);

            return await SqlService.ExecuteSqlRawCommandAsync<BaoTriDto>(query, cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<List<BaoTriDto>>(ex, "GetMaintenanceHistory");
        }
    }

    public async Task<BaseResponse<List<MaintenanceAlertDto>>> GetAlertsAsync(
        MaintenanceAlertQuery query,
        CancellationToken cancellationToken = default)
    {
        await EnsureAlertTableAsync(cancellationToken);

        var sql = @"
            SELECT 
                a.id,
                a.ma_xe,
                x.ten_xe,
                x.bien_so,
                a.alert_type,
                a.status,
                a.severity,
                a.title,
                a.message_snapshot,
                a.due_date,
                a.triggered_at,
                a.last_seen_at,
                a.resolved_at,
                a.resolved_by
            FROM maintenance_alerts a
            LEFT JOIN xe x ON x.ma_xe = a.ma_xe
            WHERE (@status IS NULL OR a.status = @status)
              AND (@severity IS NULL OR a.severity = @severity)
              AND (@ma_xe IS NULL OR a.ma_xe = @ma_xe)
            ORDER BY 
                CASE a.severity WHEN 'critical' THEN 0 ELSE 1 END,
                a.status,
                a.due_date,
                a.last_seen_at DESC";

        var model = SqlExecuteModel.RawQuery(sql)
            .AddInput("status", NullIfEmpty(query.Status))
            .AddInput("severity", NullIfEmpty(query.Severity))
            .AddInput("ma_xe", NullIfEmpty(query.MaXe));

        return await SqlService.ExecuteSqlRawCommandAsync<MaintenanceAlertDto>(model, cancellationToken);
    }

    public async Task<BaseResponse<object>> ResolveAlertAsync(
        long id,
        string? resolvedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureAlertTableAsync(cancellationToken);

        if (id <= 0)
        {
            return BaseResponse<object>.ValidationError("Mã cảnh báo không hợp lệ.");
        }

        var model = SqlExecuteModel.RawQuery(@"
                UPDATE maintenance_alerts
                SET status = 'resolved',
                    resolved_at = NOW(),
                    resolved_by = @resolved_by
                WHERE id = @id AND status = 'pending'")
            .AddInput("id", id)
            .AddInput("resolved_by", string.IsNullOrWhiteSpace(resolvedBy) ? "admin" : resolvedBy.Trim());

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(model, cancellationToken);

        if (!result.Success)
        {
            return BaseResponse<object>.Error(result.Message, result.ErrorCode);
        }

        if (result.Data == 0)
        {
            return BaseResponse<object>.NotFound("Không tìm thấy cảnh báo đang chờ xử lý.");
        }

        return BaseResponse<object>.Ok("Đã đánh dấu cảnh báo là đã xử lý.");
    }

    public async Task<BaseResponse<MaintenanceAlertScanResultDto>> ScanAlertsAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureAlertTableAsync(cancellationToken);

        var statusResult = await GetMaintenanceStatusAsync(cancellationToken);
        if (!statusResult.Success)
        {
            return BaseResponse<MaintenanceAlertScanResultDto>.Error(statusResult.Message, statusResult.ErrorCode);
        }

        var warningDays = GetAlertThresholdDays();
        var summary = new MaintenanceAlertScanResultDto
        {
            TotalEvaluated = statusResult.Data?.Count ?? 0
        };

        foreach (var item in statusResult.Data ?? [])
        {
            summary.ResolvedAlerts += await SyncAlertCategoryAsync(
                item,
                "maintenance",
                item.SoNgayConLai,
                item.NgayBaoTriTiepTheo,
                warningDays,
                cancellationToken);

            summary.ResolvedAlerts += await SyncAlertCategoryAsync(
                item,
                "inspection",
                item.SoNgayDenDangKiem,
                item.NgayDangKiem,
                warningDays,
                cancellationToken);
        }

        var activeAlertsResult = await SqlService.ExecuteSqlRawCommandAsync<MaintenanceAlertDto>(
            SqlExecuteModel.RawQuery("SELECT * FROM maintenance_alerts WHERE status = 'pending'"),
            cancellationToken);

        summary.ActiveAlerts = activeAlertsResult.Success ? activeAlertsResult.Data?.Count ?? 0 : 0;
        return BaseResponse<MaintenanceAlertScanResultDto>.Ok(summary, "Quét cảnh báo bảo trì hoàn tất.");
    }

    private async Task<int> SyncAlertCategoryAsync(
        MaintenanceStatusDto item,
        string category,
        int? daysUntilDue,
        DateTime? dueDate,
        int warningDays,
        CancellationToken cancellationToken)
    {
        string? alertType = null;
        string severity = "warning";
        string title = string.Empty;
        string message = string.Empty;

        if (daysUntilDue.HasValue)
        {
            if (daysUntilDue <= 0)
            {
                alertType = $"{category}_overdue";
                severity = "critical";
                title = category == "maintenance" ? "Xe quá hạn bảo dưỡng" : "Xe quá hạn đăng kiểm";
                message = $"{item.TenXe ?? item.MaXe} đã quá hạn {Math.Abs(daysUntilDue.Value)} ngày.";
            }
            else if (daysUntilDue <= warningDays)
            {
                alertType = $"{category}_due";
                severity = "warning";
                title = category == "maintenance" ? "Xe sắp đến hạn bảo dưỡng" : "Xe sắp đến hạn đăng kiểm";
                message = $"{item.TenXe ?? item.MaXe} còn {daysUntilDue.Value} ngày đến hạn.";
            }
        }

        var resolvedCount = await ResolveStaleCategoryAlertsAsync(item.MaXe, category, alertType, cancellationToken);

        if (alertType == null)
        {
            return resolvedCount;
        }

        var upsert = SqlExecuteModel.RawQuery(@"
                INSERT INTO maintenance_alerts (
                    ma_xe,
                    alert_type,
                    status,
                    severity,
                    title,
                    message_snapshot,
                    due_date,
                    triggered_at,
                    last_seen_at
                )
                VALUES (
                    @ma_xe,
                    @alert_type,
                    'pending',
                    @severity,
                    @title,
                    @message_snapshot,
                    @due_date,
                    NOW(),
                    NOW()
                )
                ON DUPLICATE KEY UPDATE
                    severity = VALUES(severity),
                    title = VALUES(title),
                    message_snapshot = VALUES(message_snapshot),
                    due_date = VALUES(due_date),
                    last_seen_at = NOW()")
            .AddInput("ma_xe", item.MaXe)
            .AddInput("alert_type", alertType)
            .AddInput("severity", severity)
            .AddInput("title", title)
            .AddInput("message_snapshot", message)
            .AddInput("due_date", dueDate);

        await SqlService.ExecuteSqlRawNonQueryAsync(upsert, cancellationToken);
        return resolvedCount;
    }

    private async Task<int> ResolveStaleCategoryAlertsAsync(
        string maXe,
        string category,
        string? keepAlertType,
        CancellationToken cancellationToken)
    {
        var types = category == "maintenance"
            ? new[] { "maintenance_due", "maintenance_overdue" }
            : new[] { "inspection_due", "inspection_overdue" };

        var sql = @"
            UPDATE maintenance_alerts
            SET status = 'resolved',
                resolved_at = NOW(),
                resolved_by = 'system',
                last_seen_at = NOW()
            WHERE ma_xe = @ma_xe
              AND status = 'pending'
              AND alert_type IN (@type1, @type2)
              AND (@keep_type IS NULL OR alert_type <> @keep_type)";

        var model = SqlExecuteModel.RawQuery(sql)
            .AddInput("ma_xe", maXe)
            .AddInput("type1", types[0])
            .AddInput("type2", types[1])
            .AddInput("keep_type", keepAlertType);

        var result = await SqlService.ExecuteSqlRawNonQueryAsync(model, cancellationToken);
        return result.Success ? result.Data : 0;
    }

    private async Task ResolvePendingAlertsForVehicleAsync(
        string maXe,
        CancellationToken cancellationToken)
    {
        var query = SqlExecuteModel.RawQuery(@"
                UPDATE maintenance_alerts
                SET status = 'resolved',
                    resolved_at = NOW(),
                    resolved_by = 'system-maintenance'
                WHERE ma_xe = @ma_xe
                  AND status = 'pending'
                  AND alert_type IN ('maintenance_due', 'maintenance_overdue')")
            .AddInput("ma_xe", maXe);

        await SqlService.ExecuteSqlRawNonQueryAsync(query, cancellationToken);
    }

    private async Task EnsureAlertTableAsync(CancellationToken cancellationToken)
    {
        var createTable = SqlExecuteModel.RawQuery(@"
            CREATE TABLE IF NOT EXISTS maintenance_alerts (
                id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                ma_xe VARCHAR(20) NOT NULL,
                alert_type VARCHAR(50) NOT NULL,
                status VARCHAR(20) NOT NULL DEFAULT 'pending',
                severity VARCHAR(20) NOT NULL,
                title VARCHAR(255) NOT NULL,
                message_snapshot VARCHAR(500) NOT NULL,
                due_date DATE NULL,
                triggered_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                last_seen_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                resolved_at DATETIME NULL,
                resolved_by VARCHAR(100) NULL,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                UNIQUE KEY uq_maintenance_alert_active (ma_xe, alert_type, status),
                KEY idx_maintenance_alert_status (status),
                KEY idx_maintenance_alert_vehicle (ma_xe),
                CONSTRAINT fk_maintenance_alert_vehicle FOREIGN KEY (ma_xe) REFERENCES xe(ma_xe)
            )");

        await SqlService.ExecuteSqlRawNonQueryAsync(createTable, cancellationToken);
    }

    private async Task<bool> RecordExistsAsync(
        string tableName,
        string keyColumn,
        string keyValue,
        CancellationToken cancellationToken)
    {
        var query = SqlExecuteModel.RawQuery(
                $"SELECT {keyColumn} FROM {tableName} WHERE {keyColumn} = @value LIMIT 1")
            .AddInput("value", keyValue);

        var result = await SqlService.ExecuteSqlRawCommandAsync<SimpleLookupDto>(query, cancellationToken);
        return result.Success && result.Data?.Count > 0;
    }

    private async Task<Dictionary<string, DateTime?>> GetVehicleRegistrationDatesAsync(
        CancellationToken cancellationToken)
    {
        var query = SqlExecuteModel.RawQuery("SELECT ma_xe, ngay_dang_kiem FROM xe");
        var result = await SqlService.ExecuteSqlRawCommandAsync<VehicleRegistrationDto>(query, cancellationToken);

        return result.Success
            ? result.Data?.ToDictionary(x => x.MaXe, x => x.NgayDangKiem) ?? []
            : [];
    }

    private int GetAlertThresholdDays()
    {
        var configured = _configuration.GetValue<int?>("MaintenanceAlerts:WarningDays");
        var days = configured.GetValueOrDefault(30);
        return days > 0 ? days : 30;
    }

    private static string ResolveMaintenanceStatus(MaintenanceStatusDto item, int warningDays)
    {
        if (!item.NgayBaoTriCuoi.HasValue)
        {
            return "Quá hạn";
        }

        if (item.CanBaoTri)
        {
            return item.SoNgayConLai <= 0 ? "Quá hạn" : "Sắp đến hạn";
        }

        if (item.SoNgayConLai <= 0)
        {
            return "Quá hạn";
        }

        if (item.SoNgayConLai <= warningDays)
        {
            return "Sắp đến hạn";
        }

        return "Bình thường";
    }

    private sealed class VehicleRegistrationDto
    {
        [Core.Sql.Models.CustomDataSet("ma_xe")]
        public string MaXe { get; set; } = string.Empty;

        [Core.Sql.Models.CustomDataSet("ngay_dang_kiem")]
        public DateTime? NgayDangKiem { get; set; }
    }

    private sealed class SimpleLookupDto
    {
        [Core.Sql.Models.CustomDataSet("ma_xe")]
        public string? MaXe { get; set; }
    }
}
