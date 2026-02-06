using Core.Sql.Services;
using Microsoft.AspNetCore.Mvc;

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
    private readonly ILogger<SshTunnelController> _logger;

    public SshTunnelController(
        SshTunnelService tunnelService,
        ILogger<SshTunnelController> logger)
    {
        _tunnelService = tunnelService;
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
