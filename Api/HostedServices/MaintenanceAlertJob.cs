using Application.Services.VanTai;

namespace Api.HostedServices;

public class MaintenanceAlertJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MaintenanceAlertJob> _logger;

    public MaintenanceAlertJob(
        IServiceProvider serviceProvider,
        ILogger<MaintenanceAlertJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunScanAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            _logger.LogInformation("Maintenance alert job sleeping for {Delay}", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await RunScanAsync(stoppingToken);
        }
    }

    private async Task RunScanAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();

        try
        {
            var result = await service.ScanAlertsAsync(cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Maintenance alert scan completed. Evaluated={Evaluated}, Active={Active}, Resolved={Resolved}",
                    result.Data?.TotalEvaluated ?? 0,
                    result.Data?.ActiveAlerts ?? 0,
                    result.Data?.ResolvedAlerts ?? 0);
            }
            else
            {
                _logger.LogWarning("Maintenance alert scan failed: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while scanning maintenance alerts");
        }
    }

    private static TimeSpan GetDelayUntilNextRun()
    {
        var now = DateTime.Now;
        var next = now.Date.AddDays(1).AddHours(6);
        return next - now;
    }
}
