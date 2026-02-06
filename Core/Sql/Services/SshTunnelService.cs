using Core.Sql.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;

namespace Core.Sql.Services;

/// <summary>
/// SSH Tunnel Service - Creates and maintains SSH port forwarding for MySQL connection.
/// 
/// Architecture:
/// [App] --localhost:13306--> [SSH Tunnel] --SSH--> [Remote Server] --127.0.0.1:3306--> [MySQL]
/// 
/// Features:
/// - Singleton lifetime (runs throughout app lifecycle)
/// - Auto-reconnect on disconnect
/// - Health check monitoring
/// - Clean shutdown
/// - Comprehensive logging
/// </summary>
public class SshTunnelService : IHostedService, IDisposable
{
    private readonly SshTunnelOptions _options;
    private readonly ILogger<SshTunnelService> _logger;

    private SshClient? _sshClient;
    private ForwardedPortLocal? _forwardedPort;
    private Timer? _healthCheckTimer;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;
    private int _retryCount;

    public SshTunnelService(
        IOptions<SshTunnelOptions> options,
        ILogger<SshTunnelService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Indicates whether the SSH tunnel is currently active and forwarding.
    /// </summary>
    public bool IsConnected => _sshClient?.IsConnected == true && _forwardedPort?.IsStarted == true;

    /// <summary>
    /// The local port being forwarded (use this in MySQL connection string).
    /// </summary>
    public uint LocalPort => _options.LocalPort;

    /// <summary>
    /// Starts the SSH tunnel service.
    /// Called automatically by ASP.NET Core host on application startup.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("SSH Tunnel is disabled. Connecting directly to MySQL.");
            return;
        }

        try
        {
            _options.Validate();
            _logger.LogInformation(
                "Starting SSH Tunnel to {Host}:{Port} (forwarding localhost:{LocalPort} -> {RemoteHost}:{RemotePort})",
                _options.Host, _options.Port, _options.LocalPort, _options.RemoteMySqlHost, _options.RemoteMySqlPort);

            await ConnectAsync(cancellationToken);
            StartHealthCheckTimer();

            _logger.LogInformation("SSH Tunnel started successfully. MySQL available at localhost:{LocalPort}", _options.LocalPort);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to start SSH Tunnel. Application may not be able to connect to database.");
            throw; // Let the application fail to start if tunnel is required
        }
    }

    /// <summary>
    /// Stops the SSH tunnel service.
    /// Called automatically by ASP.NET Core host on application shutdown.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return Task.CompletedTask;

        _logger.LogInformation("Stopping SSH Tunnel...");

        try
        {
            _healthCheckTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            Disconnect();
            _logger.LogInformation("SSH Tunnel stopped gracefully.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during SSH Tunnel shutdown.");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Establishes SSH connection and starts port forwarding.
    /// </summary>
    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
            {
                _logger.LogDebug("SSH Tunnel already connected.");
                return;
            }

            // Disconnect existing connection if any
            Disconnect();

            // Create authentication method
            var authMethods = CreateAuthenticationMethods();

            // Create connection info
            var connectionInfo = new ConnectionInfo(
                _options.Host,
                _options.Port,
                _options.Username,
                authMethods)
            {
                Timeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds)
            };

            // Create SSH client
            _sshClient = new SshClient(connectionInfo);
            _sshClient.ErrorOccurred += OnSshError;

            // Connect
            _logger.LogDebug("Connecting to SSH server {Host}:{Port}...", _options.Host, _options.Port);
            _sshClient.Connect();

            if (!_sshClient.IsConnected)
            {
                throw new InvalidOperationException("SSH connection failed - client not connected after Connect()");
            }

            _logger.LogDebug("SSH connected. Setting up port forwarding...");

            // Setup port forwarding
            _forwardedPort = new ForwardedPortLocal(
                "127.0.0.1",
                _options.LocalPort,
                _options.RemoteMySqlHost,
                _options.RemoteMySqlPort);

            _forwardedPort.Exception += OnForwardingException;
            _forwardedPort.RequestReceived += OnForwardingRequestReceived;

            _sshClient.AddForwardedPort(_forwardedPort);
            _forwardedPort.Start();

            if (!_forwardedPort.IsStarted)
            {
                throw new InvalidOperationException("Port forwarding failed to start");
            }

            _retryCount = 0; // Reset retry count on successful connection
            _logger.LogInformation(
                "SSH Tunnel established: localhost:{LocalPort} -> {RemoteHost}:{RemotePort}",
                _options.LocalPort, _options.RemoteMySqlHost, _options.RemoteMySqlPort);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Creates authentication methods based on configuration.
    /// </summary>
    private AuthenticationMethod[] CreateAuthenticationMethods()
    {
        var methods = new List<AuthenticationMethod>();

        // Private key authentication (preferred)
        if (!string.IsNullOrWhiteSpace(_options.PrivateKeyPath))
        {
            _logger.LogDebug("Using private key authentication: {KeyPath}", _options.PrivateKeyPath);

            PrivateKeyFile keyFile;
            if (!string.IsNullOrWhiteSpace(_options.PrivateKeyPassphrase))
            {
                keyFile = new PrivateKeyFile(_options.PrivateKeyPath, _options.PrivateKeyPassphrase);
            }
            else
            {
                keyFile = new PrivateKeyFile(_options.PrivateKeyPath);
            }

            methods.Add(new PrivateKeyAuthenticationMethod(_options.Username, keyFile));
        }

        // Password authentication
        if (!string.IsNullOrWhiteSpace(_options.Password))
        {
            _logger.LogDebug("Using password authentication for user: {Username}", _options.Username);
            methods.Add(new PasswordAuthenticationMethod(_options.Username, _options.Password));
        }

        if (methods.Count == 0)
        {
            throw new InvalidOperationException("No SSH authentication method configured.");
        }

        return methods.ToArray();
    }

    /// <summary>
    /// Disconnects SSH and stops port forwarding.
    /// </summary>
    private void Disconnect()
    {
        try
        {
            if (_forwardedPort != null)
            {
                if (_forwardedPort.IsStarted)
                {
                    _forwardedPort.Stop();
                }
                _forwardedPort.Exception -= OnForwardingException;
                _forwardedPort.RequestReceived -= OnForwardingRequestReceived;
                _forwardedPort.Dispose();
                _forwardedPort = null;
            }

            if (_sshClient != null)
            {
                _sshClient.ErrorOccurred -= OnSshError;
                if (_sshClient.IsConnected)
                {
                    _sshClient.Disconnect();
                }
                _sshClient.Dispose();
                _sshClient = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during SSH disconnect.");
        }
    }

    /// <summary>
    /// Starts periodic health check timer.
    /// </summary>
    private void StartHealthCheckTimer()
    {
        _healthCheckTimer = new Timer(
            async _ => await HealthCheckAsync(),
            null,
            TimeSpan.FromSeconds(_options.HealthCheckIntervalSeconds),
            TimeSpan.FromSeconds(_options.HealthCheckIntervalSeconds));
    }

    /// <summary>
    /// Performs health check and reconnects if necessary.
    /// </summary>
    private async Task HealthCheckAsync()
    {
        if (_disposed) return;

        try
        {
            if (!IsConnected)
            {
                _logger.LogWarning("SSH Tunnel disconnected. Attempting reconnection...");
                await ReconnectAsync();
            }
            else
            {
                _logger.LogDebug("SSH Tunnel health check: OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed.");
        }
    }

    /// <summary>
    /// Attempts to reconnect with retry logic.
    /// </summary>
    private async Task ReconnectAsync()
    {
        if (_retryCount >= _options.MaxRetryAttempts)
        {
            _logger.LogCritical(
                "SSH Tunnel reconnection failed after {MaxAttempts} attempts. Manual intervention required.",
                _options.MaxRetryAttempts);
            return;
        }

        _retryCount++;
        _logger.LogInformation(
            "SSH Tunnel reconnection attempt {Attempt}/{MaxAttempts}...",
            _retryCount, _options.MaxRetryAttempts);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds));
            await ConnectAsync(CancellationToken.None);
            _logger.LogInformation("SSH Tunnel reconnected successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reconnection attempt {Attempt} failed.", _retryCount);
        }
    }

    /// <summary>
    /// Handles SSH client errors.
    /// </summary>
    private void OnSshError(object? sender, Renci.SshNet.Common.ExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "SSH Client error occurred.");
    }

    /// <summary>
    /// Handles port forwarding exceptions.
    /// </summary>
    private void OnForwardingException(object? sender, Renci.SshNet.Common.ExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "SSH Port forwarding error occurred.");
    }

    /// <summary>
    /// Logs forwarding requests (debug only).
    /// </summary>
    private void OnForwardingRequestReceived(object? sender, Renci.SshNet.Common.PortForwardEventArgs e)
    {
        _logger.LogDebug("Port forwarding request: {Host}:{Port}", e.OriginatorHost, e.OriginatorPort);
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _healthCheckTimer?.Dispose();
        Disconnect();
        _connectionLock.Dispose();

        GC.SuppressFinalize(this);
    }
}
