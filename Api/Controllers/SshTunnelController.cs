using Core.Sql.Config;
using Core.Sql.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Api.Controllers;

/// <summary>
/// Controller for SSH Tunnel status and management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SshTunnelController : ControllerBase
{
    private readonly SshTunnelService _tunnelService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly SshTunnelOptions _options;
    private readonly ILogger<SshTunnelController> _logger;

    public SshTunnelController(
        SshTunnelService tunnelService,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IOptions<SshTunnelOptions> options,
        ILogger<SshTunnelController> logger)
    {
        _tunnelService = tunnelService;
        _configuration = configuration;
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current status of the SSH tunnel.
    /// </summary>
    /// <returns>Tunnel status including connection state and forwarded port.</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(SshTunnelStatus), 200)]
    public IActionResult GetStatus()
    {
        var status = new SshTunnelStatus
        {
            IsConnected = _tunnelService.IsConnected,
            LocalPort = _tunnelService.LocalPort,
            Message = _tunnelService.IsConnected
                ? $"SSH Tunnel active. MySQL available at localhost:{_tunnelService.LocalPort}"
                : "SSH Tunnel is not connected."
        };

        return Ok(status);
    }

    /// <summary>
    /// Performs an end-to-end diagnostic of the current SSH tunnel and MySQL connection.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(SshTunnelHealthResponse), 200)]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var response = new SshTunnelHealthResponse
        {
            Environment = _environment.EnvironmentName,
            TunnelEnabled = _options.Enabled,
            TunnelConnected = _tunnelService.IsConnected,
            LocalPort = _tunnelService.LocalPort,
            SshHost = _options.Host,
            SshPort = _options.Port,
            RemoteMySqlHost = _options.RemoteMySqlHost,
            RemoteMySqlPort = _options.RemoteMySqlPort
        };

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            response.Message = "DefaultConnection is not configured.";
            return Ok(response);
        }

        var csb = new MySqlConnectionStringBuilder(connectionString);
        response.DatabaseHost = csb.Server;
        response.DatabasePort = csb.Port;
        response.DatabaseName = csb.Database;
        response.DatabaseUser = csb.UserID;

        try
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            response.CanConnectToDatabase = true;

            await using var command = new MySqlCommand(
                "SELECT DATABASE() AS DbName, @@hostname AS HostName, NOW() AS ServerTime;",
                connection);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                response.ConnectedDatabase = reader["DbName"]?.ToString();
                response.MySqlHostName = reader["HostName"]?.ToString();
                response.ServerTime = reader["ServerTime"] as DateTime?;
            }

            response.Message = "SSH tunnel and MySQL connection are healthy.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SSH tunnel health check failed");
            response.CanConnectToDatabase = false;
            response.Message = "MySQL connection through the current configuration failed.";
            response.Error = ex.Message;
        }

        return Ok(response);
    }
}

/// <summary>
/// SSH Tunnel status response model.
/// </summary>
public class SshTunnelStatus
{
    public bool IsConnected { get; set; }
    public uint LocalPort { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Detailed SSH tunnel and database connectivity diagnostic response.
/// </summary>
public class SshTunnelHealthResponse
{
    public string Environment { get; set; } = string.Empty;
    public bool TunnelEnabled { get; set; }
    public bool TunnelConnected { get; set; }
    public uint LocalPort { get; set; }
    public string SshHost { get; set; } = string.Empty;
    public int SshPort { get; set; }
    public string RemoteMySqlHost { get; set; } = string.Empty;
    public uint RemoteMySqlPort { get; set; }
    public string DatabaseHost { get; set; } = string.Empty;
    public uint DatabasePort { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string DatabaseUser { get; set; } = string.Empty;
    public bool CanConnectToDatabase { get; set; }
    public string? ConnectedDatabase { get; set; }
    public string? MySqlHostName { get; set; }
    public DateTime? ServerTime { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
}
