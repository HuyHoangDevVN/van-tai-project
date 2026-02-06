namespace Core.Sql.Config;

/// <summary>
/// Interface for Dynamic Stored Procedure Resolution.
/// Provides a way to lookup SP names by FunctionKey from database configuration.
/// Implementations should use caching to minimize database hits.
/// </summary>
public interface IProcedureConfigProvider
{
    /// <summary>
    /// Gets the stored procedure name for a given function key.
    /// </summary>
    /// <param name="functionKey">The unique key identifying the feature (e.g., "BUS_SEARCH")</param>
    /// <returns>The actual stored procedure name in MySQL</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the function key is not found in configuration</exception>
    Task<string> GetSpNameAsync(string functionKey);

    /// <summary>
    /// Gets the stored procedure name, returning null if not found (no exception).
    /// </summary>
    Task<string?> GetSpNameOrDefaultAsync(string functionKey);

    /// <summary>
    /// Gets all procedure configurations, optionally filtered by module.
    /// </summary>
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(string? moduleName = null);

    /// <summary>
    /// Refreshes the cached configuration from database.
    /// Call this after updating Sys_ProcedureConfig table.
    /// </summary>
    Task RefreshCacheAsync();

    /// <summary>
    /// Checks if a function key exists in configuration.
    /// </summary>
    Task<bool> ExistsAsync(string functionKey);
}
