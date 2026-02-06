using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Core.Sql.Models;

namespace Core.Sql.Extensions;

/// <summary>
/// Extension methods for DataTable operations including mapping to objects and JSON conversion.
/// Uses reflection caching for improved performance in high-traffic scenarios.
/// </summary>
public static class DataTableExtensions
{
    // Cache for property mappings to avoid repeated reflection calls
    private static readonly ConcurrentDictionary<Type, PropertyMappingInfo[]> _propertyMappingCache = new();

    /// <summary>
    /// Converts a DataTable to a strongly-typed List.
    /// Respects [CustomDataSet] attribute for column-to-property mapping.
    /// Falls back to property name matching when attribute is not present.
    /// </summary>
    /// <typeparam name="T">The target type to map rows to.</typeparam>
    /// <param name="table">The source DataTable.</param>
    /// <returns>A list of mapped objects.</returns>
    /// <remarks>
    /// Mapping priority:
    /// 1. [CustomDataSet("column_name")] attribute - maps to specified column
    /// 2. Property name exact match (case-insensitive)
    /// 3. Property name with underscores converted (e.g., UserId maps to user_id)
    /// </remarks>
    public static List<T> DataTableToList<T>(this DataTable? table) where T : class, new()
    {
        if (table == null || table.Rows.Count == 0)
            return [];

        var result = new List<T>(table.Rows.Count);
        var mappings = GetPropertyMappings<T>(table);

        foreach (DataRow row in table.Rows)
        {
            var item = new T();

            foreach (var mapping in mappings)
            {
                if (mapping.ColumnOrdinal < 0)
                    continue;

                var value = row[mapping.ColumnOrdinal];
                TypeConversionHelper.SetValueToObject(
                    mapping.Property,
                    item,
                    value,
                    mapping.DefaultValue,
                    mapping.DateFormat
                );
            }

            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// Converts a DataTable to a single object (first row only).
    /// </summary>
    public static T? DataTableToObject<T>(this DataTable? table) where T : class, new()
    {
        if (table == null || table.Rows.Count == 0)
            return null;

        var mappings = GetPropertyMappings<T>(table);
        var item = new T();
        var row = table.Rows[0];

        foreach (var mapping in mappings)
        {
            if (mapping.ColumnOrdinal < 0)
                continue;

            var value = row[mapping.ColumnOrdinal];
            TypeConversionHelper.SetValueToObject(
                mapping.Property,
                item,
                value,
                mapping.DefaultValue,
                mapping.DateFormat
            );
        }

        return item;
    }

    /// <summary>
    /// Converts a DataTable to a JsonArray using System.Text.Json.Nodes.
    /// </summary>
    /// <param name="table">The source DataTable.</param>
    /// <param name="useCamelCase">If true, converts column names to camelCase.</param>
    /// <returns>A JsonArray containing the table data.</returns>
    public static JsonArray DataTableToJson(this DataTable? table, bool useCamelCase = true)
    {
        var jsonArray = new JsonArray();

        if (table == null || table.Rows.Count == 0)
            return jsonArray;

        var columns = table.Columns.Cast<DataColumn>().ToList();

        foreach (DataRow row in table.Rows)
        {
            var jsonObject = new JsonObject();

            foreach (var column in columns)
            {
                var columnName = useCamelCase
                    ? ToCamelCase(column.ColumnName)
                    : column.ColumnName;

                var value = row[column];
                jsonObject[columnName] = ConvertToJsonNode(value, column.DataType);
            }

            jsonArray.Add(jsonObject);
        }

        return jsonArray;
    }

    /// <summary>
    /// Converts a DataTable to a JSON string.
    /// </summary>
    public static string DataTableToJsonString(this DataTable? table, bool useCamelCase = true,
        bool indented = false)
    {
        var jsonArray = table.DataTableToJson(useCamelCase);
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented
        };
        return jsonArray.ToJsonString(options);
    }

    /// <summary>
    /// Converts a single DataRow to a dictionary.
    /// </summary>
    public static Dictionary<string, object?> DataRowToDictionary(this DataRow row, bool useCamelCase = true)
    {
        var dict = new Dictionary<string, object?>();
        var columns = row.Table.Columns;

        foreach (DataColumn column in columns)
        {
            var key = useCamelCase ? ToCamelCase(column.ColumnName) : column.ColumnName;
            var value = row[column];
            dict[key] = value == DBNull.Value ? null : value;
        }

        return dict;
    }

    /// <summary>
    /// Reads a scalar value from the first cell of a DataTable.
    /// </summary>
    public static T? GetScalarValue<T>(this DataTable? table, T? defaultValue = default)
    {
        if (table == null || table.Rows.Count == 0 || table.Columns.Count == 0)
            return defaultValue;

        var value = table.Rows[0][0];
        return TypeConversionHelper.SafeConvert(value, defaultValue);
    }

    /// <summary>
    /// Gets a single column as a list of values.
    /// </summary>
    public static List<T> GetColumnValues<T>(this DataTable? table, string columnName)
    {
        if (table == null || !table.Columns.Contains(columnName))
            return [];

        var result = new List<T>(table.Rows.Count);
        var ordinal = table.Columns[columnName]!.Ordinal;

        foreach (DataRow row in table.Rows)
        {
            var value = TypeConversionHelper.SafeConvert<T>(row[ordinal]);
            if (value != null)
                result.Add(value);
        }

        return result;
    }

    /// <summary>
    /// Gets a single column as a list of values using column index.
    /// </summary>
    public static List<T> GetColumnValues<T>(this DataTable? table, int columnIndex)
    {
        if (table == null || columnIndex < 0 || columnIndex >= table.Columns.Count)
            return [];

        var result = new List<T>(table.Rows.Count);

        foreach (DataRow row in table.Rows)
        {
            var value = TypeConversionHelper.SafeConvert<T>(row[columnIndex]);
            if (value != null)
                result.Add(value);
        }

        return result;
    }

    #region Private Helper Methods

    /// <summary>
    /// Gets cached property mappings for a type, or creates them if not cached.
    /// </summary>
    private static PropertyMappingInfo[] GetPropertyMappings<T>(DataTable table) where T : class
    {
        var type = typeof(T);

        // We need to include column names in the cache key since the same type
        // might be mapped from different tables with different columns
        // For simplicity in this implementation, we'll create mappings per call
        // but cache the property info extraction

        if (!_propertyMappingCache.TryGetValue(type, out var cachedMappings))
        {
            cachedMappings = CreatePropertyMappings(type);
            _propertyMappingCache.TryAdd(type, cachedMappings);
        }

        // Map column ordinals for this specific table
        return MapColumnOrdinals(cachedMappings, table);
    }

    /// <summary>
    /// Creates property mapping info from reflection (cached).
    /// </summary>
    private static PropertyMappingInfo[] CreatePropertyMappings(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        var mappings = new PropertyMappingInfo[properties.Count];

        for (int i = 0; i < properties.Count; i++)
        {
            var prop = properties[i];
            var attribute = prop.GetCustomAttribute<CustomDataSetAttribute>();

            mappings[i] = new PropertyMappingInfo
            {
                Property = prop,
                AttributeColumnName = attribute?.ColumnName,
                IsIgnored = attribute?.Ignore ?? false,
                DefaultValue = attribute?.DefaultValue,
                DateFormat = attribute?.DateFormat,
                PropertyName = prop.Name,
                PropertyNameLower = prop.Name.ToLowerInvariant(),
                PropertyNameSnakeCase = ToSnakeCase(prop.Name).ToLowerInvariant()
            };
        }

        return mappings;
    }

    /// <summary>
    /// Maps column ordinals for a specific DataTable.
    /// </summary>
    private static PropertyMappingInfo[] MapColumnOrdinals(PropertyMappingInfo[] mappings, DataTable table)
    {
        // Build column name lookup (case-insensitive)
        var columnLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < table.Columns.Count; i++)
        {
            columnLookup[table.Columns[i].ColumnName] = i;
        }

        var result = new PropertyMappingInfo[mappings.Length];

        for (int i = 0; i < mappings.Length; i++)
        {
            result[i] = mappings[i] with { ColumnOrdinal = -1 };

            if (result[i].IsIgnored)
                continue;

            // Priority 1: CustomDataSet attribute column name
            if (!string.IsNullOrEmpty(result[i].AttributeColumnName))
            {
                if (columnLookup.TryGetValue(result[i].AttributeColumnName!, out var ordinal))
                {
                    result[i] = result[i] with { ColumnOrdinal = ordinal };
                    continue;
                }
            }

            // Priority 2: Exact property name match (case-insensitive)
            if (columnLookup.TryGetValue(result[i].PropertyName, out var ordinal2))
            {
                result[i] = result[i] with { ColumnOrdinal = ordinal2 };
                continue;
            }

            // Priority 3: Snake_case conversion (e.g., UserId -> user_id)
            if (columnLookup.TryGetValue(result[i].PropertyNameSnakeCase, out var ordinal3))
            {
                result[i] = result[i] with { ColumnOrdinal = ordinal3 };
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a value to a JsonNode for JSON serialization.
    /// </summary>
    private static JsonNode? ConvertToJsonNode(object? value, Type dataType)
    {
        if (value == null || value == DBNull.Value)
            return null;

        // Handle specific types
        if (value is DateTime dt)
            return JsonValue.Create(dt.ToString("O")); // ISO 8601 format

        if (value is DateTimeOffset dto)
            return JsonValue.Create(dto.ToString("O"));

        if (value is DateOnly dateOnly)
            return JsonValue.Create(dateOnly.ToString("yyyy-MM-dd"));

        if (value is TimeOnly timeOnly)
            return JsonValue.Create(timeOnly.ToString("HH:mm:ss"));

        if (value is TimeSpan ts)
            return JsonValue.Create(ts.ToString());

        if (value is Guid guid)
            return JsonValue.Create(guid.ToString());

        if (value is byte[] bytes)
            return JsonValue.Create(Convert.ToBase64String(bytes));

        if (value is bool b)
            return JsonValue.Create(b);

        if (value is string s)
            return JsonValue.Create(s);

        // Numeric types
        if (value is int i) return JsonValue.Create(i);
        if (value is long l) return JsonValue.Create(l);
        if (value is decimal d) return JsonValue.Create(d);
        if (value is double db) return JsonValue.Create(db);
        if (value is float f) return JsonValue.Create(f);
        if (value is short sh) return JsonValue.Create(sh);
        if (value is byte by) return JsonValue.Create(by);

        // Default: convert to string
        return JsonValue.Create(value.ToString());
    }

    /// <summary>
    /// Converts a string to camelCase.
    /// </summary>
    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        // Handle snake_case
        if (str.Contains('_'))
        {
            var parts = str.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return str;

            return parts[0].ToLowerInvariant() +
                   string.Concat(parts.Skip(1).Select(p => char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
        }

        // Simple camelCase conversion
        if (char.IsUpper(str[0]))
            return char.ToLowerInvariant(str[0]) + str[1..];

        return str;
    }

    /// <summary>
    /// Converts a string from PascalCase to snake_case.
    /// </summary>
    private static string ToSnakeCase(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var result = new System.Text.StringBuilder();

        for (int i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    result.Append('_');
                result.Append(char.ToLowerInvariant(c));
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    #endregion

    /// <summary>
    /// Internal class to hold property mapping information.
    /// </summary>
    private record PropertyMappingInfo
    {
        public required PropertyInfo Property { get; init; }
        public string? AttributeColumnName { get; init; }
        public bool IsIgnored { get; init; }
        public object? DefaultValue { get; init; }
        public string? DateFormat { get; init; }
        public required string PropertyName { get; init; }
        public required string PropertyNameLower { get; init; }
        public required string PropertyNameSnakeCase { get; init; }
        public int ColumnOrdinal { get; init; } = -1;
    }
}
