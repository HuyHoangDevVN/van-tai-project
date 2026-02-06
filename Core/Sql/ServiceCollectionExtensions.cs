using Core.Sql.Config;
using Core.Sql.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Sql;

/// <summary>
/// Extension methods for registering SQL execution services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SQL execution service with the dependency injection container.
    /// Service is registered as Scoped to align with typical request lifecycle.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Prerequisites:
    /// - Connection string named "DefaultConnection" must be configured in appsettings.json
    /// - ILogger must be available in the DI container
    /// </remarks>
    public static IServiceCollection AddSqlExecuteService(this IServiceCollection services)
    {
        // Register as Scoped - one instance per request
        // This aligns with the typical HTTP request lifecycle and
        // ensures proper connection management
        services.AddScoped<ISqlExecuteService, SqlExecuteService>();

        return services;
    }

    /// <summary>
    /// Registers the SQL execution service with a custom configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for additional setup.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqlExecuteService(
        this IServiceCollection services,
        Action<SqlExecuteServiceOptions> configure)
    {
        var options = new SqlExecuteServiceOptions();
        configure(options);

        // Register options if needed for injection
        services.AddSingleton(options);

        // Register the service
        services.AddScoped<ISqlExecuteService, SqlExecuteService>();

        return services;
    }

    /// <summary>
    /// Registers the SSH Tunnel service as a hosted service.
    /// The tunnel starts automatically on application startup and stops on shutdown.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration containing SshTunnel section.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Configuration in appsettings.json:
    /// {
    ///   "SshTunnel": {
    ///     "Enabled": true,
    ///     "Host": "ssh-server.com",
    ///     "Port": 22,
    ///     "Username": "user",
    ///     "Password": "pass",  // or PrivateKeyPath
    ///     "LocalPort": 13306,
    ///     "RemoteMySqlHost": "127.0.0.1",
    ///     "RemoteMySqlPort": 3306
    ///   }
    /// }
    /// 
    /// When enabled, MySQL connection string should use LocalPort (e.g., 13306).
    /// </remarks>
    public static IServiceCollection AddSshTunnel(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind SshTunnel configuration section
        services.AddOptions<SshTunnelOptions>()
            .Bind(configuration.GetSection(SshTunnelOptions.SectionName));

        // Register as hosted service (singleton lifetime, starts/stops with app)
        services.AddHostedService<SshTunnelService>();

        // Also register as singleton so it can be injected elsewhere if needed
        services.AddSingleton<SshTunnelService>(sp =>
            (SshTunnelService)sp.GetServices<Microsoft.Extensions.Hosting.IHostedService>()
                .First(s => s is SshTunnelService));

        return services;
    }
}

/// <summary>
/// Configuration options for the SQL execution service.
/// </summary>
public class SqlExecuteServiceOptions
{
    /// <summary>
    /// Default command timeout in seconds.
    /// </summary>
    public int DefaultCommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to enable detailed logging of SQL queries.
    /// Should be false in production.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Maximum retry attempts for transient failures.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts in milliseconds.
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 100;
}
