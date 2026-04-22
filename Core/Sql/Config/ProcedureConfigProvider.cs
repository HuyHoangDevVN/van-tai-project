using Core.Sql.Models;
using Core.Sql.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Sql.Config;

/// <summary>
/// Implementation of IProcedureConfigProvider with Memory Cache.
/// Loads SP mappings from Sys_ProcedureConfig table and caches them.
/// 
/// Caching Strategy:
/// - Full cache on first access (Lazy Loading)
/// - Configurable expiration time (default: 30 minutes)
/// - Manual refresh available via RefreshCacheAsync()
/// 
/// NOTE: Uses IServiceScopeFactory to create scoped ISqlExecuteService instances
/// because this provider is registered as Singleton.
/// </summary>
public class ProcedureConfigProvider : IProcedureConfigProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProcedureConfigProvider> _logger;

    // Cache key for the full dictionary
    private const string CacheKey = "Sys_ProcedureConfig_All";

    // Cache options - configurable via constructor in production
    private readonly MemoryCacheEntryOptions _cacheOptions;

    // Lock object for thread-safe cache population
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);

    /// <summary>
    /// Creates a new ProcedureConfigProvider with specified cache duration.
    /// </summary>
    /// <param name="scopeFactory">Service scope factory for creating scoped services</param>
    /// <param name="cache">Memory cache instance (Singleton)</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="cacheExpirationMinutes">Cache expiration in minutes (default: 30)</param>
    public ProcedureConfigProvider(
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache,
        ILogger<ProcedureConfigProvider> logger,
        int cacheExpirationMinutes = 30)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;

        _cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheExpirationMinutes))
            .SetPriority(CacheItemPriority.High)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                _logger.LogInformation(
                    "Procedure config cache evicted. Reason: {Reason}", reason);
            });
    }

    /// <inheritdoc/>
    public async Task<string> GetSpNameAsync(string functionKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionKey);

        var configs = await GetCachedConfigAsync();

        if (configs.TryGetValue(functionKey, out var spName))
        {
            _logger.LogDebug("Resolved SP for {Key}: {SpName}", functionKey, spName);
            return spName;
        }

        _logger.LogWarning("Function key not found in configuration: {Key}", functionKey);
        throw new KeyNotFoundException(
            $"Không tìm thấy cấu hình cho chức năng: '{functionKey}'. " +
            $"Vui lòng kiểm tra bảng Sys_ProcedureConfig.");
    }

    /// <inheritdoc/>
    public async Task<string?> GetSpNameOrDefaultAsync(string functionKey)
    {
        if (string.IsNullOrWhiteSpace(functionKey))
            return null;

        var configs = await GetCachedConfigAsync();
        return configs.TryGetValue(functionKey, out var spName) ? spName : null;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(string? moduleName = null)
    {
        var configs = await GetCachedConfigAsync();

        if (string.IsNullOrWhiteSpace(moduleName))
            return configs;

        // If module filter specified, we need the full config with module info
        // For simplicity, return filtered from DB if module specified
        var filtered = await LoadFromDatabaseAsync(moduleName);
        return filtered;
    }

    /// <inheritdoc/>
    public async Task RefreshCacheAsync()
    {
        _logger.LogInformation("Manually refreshing procedure config cache...");

        await _cacheLock.WaitAsync();
        try
        {
            _cache.Remove(CacheKey);
            await GetCachedConfigAsync(); // Force reload
            _logger.LogInformation("Procedure config cache refreshed successfully");
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string functionKey)
    {
        if (string.IsNullOrWhiteSpace(functionKey))
            return false;

        var configs = await GetCachedConfigAsync();
        return configs.ContainsKey(functionKey);
    }

    /// <summary>
    /// Gets the cached configuration dictionary, loading from DB if needed.
    /// Thread-safe implementation with double-check locking.
    /// </summary>
    private async Task<IReadOnlyDictionary<string, string>> GetCachedConfigAsync()
    {
        // Try get from cache first (fast path)
        if (_cache.TryGetValue(CacheKey, out IReadOnlyDictionary<string, string>? configs) && configs != null)
        {
            return configs;
        }

        // Cache miss - need to load from database (slow path)
        await _cacheLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cache.TryGetValue(CacheKey, out configs) && configs != null)
            {
                return configs;
            }

            _logger.LogInformation("Loading procedure configurations from database...");

            configs = await LoadFromDatabaseAsync();

            if (configs.Count == 0)
            {
                _logger.LogWarning(
                    "Sys_ProcedureConfig query succeeded but returned 0 active mappings. Empty result will not be cached.");
                return configs;
            }

            _cache.Set(CacheKey, configs, _cacheOptions);

            _logger.LogInformation(
                "Loaded {Count} procedure configurations into cache", configs.Count);

            return configs;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Loads procedure configurations from database.
    /// Creates a new scope to get ISqlExecuteService instance.
    /// </summary>
    private async Task<IReadOnlyDictionary<string, string>> LoadFromDatabaseAsync(string? moduleName = null)
    {
        try
        {
            // Create a scope to resolve scoped service
            using var scope = _scopeFactory.CreateScope();
            var sqlService = scope.ServiceProvider.GetRequiredService<ISqlExecuteService>();

            var query = moduleName == null
                ? "SELECT FunctionKey, ProcedureName FROM Sys_ProcedureConfig WHERE IsActive = 1"
                : "SELECT FunctionKey, ProcedureName FROM Sys_ProcedureConfig WHERE IsActive = 1 AND ModuleName = @ModuleName";

            var sqlModel = SqlExecuteModel.RawQuery(query);

            if (moduleName != null)
            {
                sqlModel.AddInput("ModuleName", moduleName);
            }

            var result = await sqlService.ExecuteSqlRawCommandAsync<ProcedureConfigDto>(sqlModel);

            if (!result.Success)
            {
                throw new InvalidOperationException(
                    $"Failed to load procedure configs from database: {result.Message}");
            }

            var data = result.Data ?? [];

            var dictionary = data.ToDictionary(
                x => x.FunctionKey,
                x => x.ProcedureName,
                StringComparer.OrdinalIgnoreCase);

            return dictionary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading procedure configurations from database");
            throw;
        }
    }
}
