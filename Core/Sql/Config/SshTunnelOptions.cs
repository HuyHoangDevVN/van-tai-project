namespace Core.Sql.Config;

/// <summary>
/// SSH Tunnel configuration options.
/// Loaded from appsettings.json "SshTunnel" section.
/// </summary>
public class SshTunnelOptions
{
    public const string SectionName = "SshTunnel";

    /// <summary>
    /// Enable/disable SSH tunnel. Set to false to connect directly to MySQL.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// SSH server hostname or IP address.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SSH server port (default: 22).
    /// </summary>
    public int Port { get; set; } = 22;

    /// <summary>
    /// SSH username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// SSH password (use either Password or PrivateKeyPath, not both).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Path to SSH private key file (e.g., ~/.ssh/id_rsa).
    /// </summary>
    public string? PrivateKeyPath { get; set; }

    /// <summary>
    /// Passphrase for encrypted private key (optional).
    /// </summary>
    public string? PrivateKeyPassphrase { get; set; }

    /// <summary>
    /// Local port to bind for forwarding (default: 13306).
    /// MySQL connection string should use this port.
    /// </summary>
    public uint LocalPort { get; set; } = 13306;

    /// <summary>
    /// Remote MySQL host as seen from SSH server (usually localhost or 127.0.0.1).
    /// </summary>
    public string RemoteMySqlHost { get; set; } = "127.0.0.1";

    /// <summary>
    /// Remote MySQL port on the SSH server (default: 3306).
    /// </summary>
    public uint RemoteMySqlPort { get; set; } = 3306;

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Interval in seconds to check connection health and reconnect if needed.
    /// </summary>
    public int HealthCheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts for reconnection.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;

    /// <summary>
    /// Delay between retry attempts in seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public void Validate()
    {
        if (!Enabled) return;

        if (string.IsNullOrWhiteSpace(Host))
            throw new ArgumentException("SSH Host is required when tunnel is enabled.");

        if (string.IsNullOrWhiteSpace(Username))
            throw new ArgumentException("SSH Username is required when tunnel is enabled.");

        if (string.IsNullOrWhiteSpace(Password) && string.IsNullOrWhiteSpace(PrivateKeyPath))
            throw new ArgumentException("Either SSH Password or PrivateKeyPath is required.");

        if (!string.IsNullOrWhiteSpace(PrivateKeyPath) && !File.Exists(PrivateKeyPath))
            throw new FileNotFoundException($"SSH Private key file not found: {PrivateKeyPath}");
    }
}
