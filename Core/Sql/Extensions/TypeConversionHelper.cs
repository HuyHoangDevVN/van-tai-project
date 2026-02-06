using System.Globalization;
using System.Reflection;

namespace Core.Sql.Extensions;

/// <summary>
/// Helper class for safely converting database values to .NET types.
/// Handles complex scenarios like DBNull, nullable types, enums, and custom conversions.
/// </summary>
public static class TypeConversionHelper
{
    /// <summary>
    /// Safely sets a value to an object's property, handling type conversions.
    /// </summary>
    /// <param name="prop">The PropertyInfo of the target property.</param>
    /// <param name="item">The object instance to set the value on.</param>
    /// <param name="value">The value to set (from database).</param>
    /// <param name="defaultValue">Optional default value if the source is DBNull.</param>
    /// <param name="dateFormat">Optional date format for DateTime parsing.</param>
    public static void SetValueToObject(PropertyInfo prop, object item, object? value,
        object? defaultValue = null, string? dateFormat = null)
    {
        if (prop == null || item == null)
            return;

        try
        {
            // Handle DBNull - use default value or null/default
            if (value == null || value == DBNull.Value)
            {
                if (defaultValue != null)
                {
                    prop.SetValue(item, ConvertValue(defaultValue, prop.PropertyType));
                }
                else if (IsNullableType(prop.PropertyType) || !prop.PropertyType.IsValueType)
                {
                    prop.SetValue(item, null);
                }
                // For non-nullable value types, leave as default (don't set)
                return;
            }

            // Convert and set the value
            var convertedValue = ConvertValue(value, prop.PropertyType, dateFormat);
            prop.SetValue(item, convertedValue);
        }
        catch (Exception)
        {
            // Silently fail on conversion errors - property will keep its default value
            // In production, you might want to log this
        }
    }

    /// <summary>
    /// Converts a value to the target type with comprehensive type handling.
    /// </summary>
    /// <param name="value">The source value to convert.</param>
    /// <param name="targetType">The target type to convert to.</param>
    /// <param name="dateFormat">Optional date format for DateTime parsing.</param>
    /// <returns>The converted value.</returns>
    public static object? ConvertValue(object? value, Type targetType, string? dateFormat = null)
    {
        if (value == null || value == DBNull.Value)
        {
            return GetDefaultValue(targetType);
        }

        var sourceType = value.GetType();

        // If types match, return directly
        if (targetType.IsAssignableFrom(sourceType))
        {
            return value;
        }

        // Handle Nullable<T>
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            targetType = underlyingType;
        }

        // String conversion
        if (targetType == typeof(string))
        {
            return value.ToString();
        }

        // Guid conversion (string to Guid)
        if (targetType == typeof(Guid))
        {
            return ConvertToGuid(value);
        }

        // Boolean conversion (handles MySQL bit/tinyint)
        if (targetType == typeof(bool))
        {
            return ConvertToBoolean(value);
        }

        // DateTime conversion
        if (targetType == typeof(DateTime))
        {
            return ConvertToDateTime(value, dateFormat);
        }

        // DateOnly conversion (.NET 6+)
        if (targetType == typeof(DateOnly))
        {
            return ConvertToDateOnly(value, dateFormat);
        }

        // TimeOnly conversion (.NET 6+)
        if (targetType == typeof(TimeOnly))
        {
            return ConvertToTimeOnly(value);
        }

        // TimeSpan conversion
        if (targetType == typeof(TimeSpan))
        {
            return ConvertToTimeSpan(value);
        }

        // Enum conversion
        if (targetType.IsEnum)
        {
            return ConvertToEnum(value, targetType);
        }

        // Numeric conversions
        if (IsNumericType(targetType))
        {
            return ConvertToNumeric(value, targetType);
        }

        // Byte array conversion
        if (targetType == typeof(byte[]))
        {
            return ConvertToByteArray(value);
        }

        // Last resort: use Convert.ChangeType
        try
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return GetDefaultValue(targetType);
        }
    }

    #region Type-Specific Conversions

    /// <summary>
    /// Converts a value to Guid.
    /// </summary>
    private static Guid ConvertToGuid(object value)
    {
        if (value is Guid guid)
            return guid;

        if (value is byte[] bytes && bytes.Length == 16)
            return new Guid(bytes);

        if (value is string str)
        {
            if (Guid.TryParse(str, out var parsedGuid))
                return parsedGuid;
        }

        return Guid.Empty;
    }

    /// <summary>
    /// Converts a value to Boolean.
    /// Handles MySQL's various representations: bit, tinyint, string.
    /// </summary>
    private static bool ConvertToBoolean(object value)
    {
        if (value is bool b)
            return b;

        // Handle numeric types (MySQL tinyint, bit)
        if (value is sbyte sb)
            return sb != 0;
        if (value is byte by)
            return by != 0;
        if (value is short s)
            return s != 0;
        if (value is int i)
            return i != 0;
        if (value is long l)
            return l != 0;
        if (value is ulong ul)
            return ul != 0;
        if (value is decimal d)
            return d != 0;

        // Handle string representations
        if (value is string str)
        {
            str = str.Trim().ToLowerInvariant();
            return str == "1" || str == "true" || str == "yes" || str == "y" || str == "on";
        }

        return Convert.ToBoolean(value);
    }

    /// <summary>
    /// Converts a value to DateTime.
    /// </summary>
    private static DateTime ConvertToDateTime(object value, string? dateFormat)
    {
        if (value is DateTime dt)
            return dt;

        if (value is DateTimeOffset dto)
            return dto.DateTime;

        if (value is string str)
        {
            if (!string.IsNullOrEmpty(dateFormat))
            {
                if (DateTime.TryParseExact(str, dateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedExact))
                    return parsedExact;
            }

            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed;
        }

        if (value is long ticks)
            return new DateTime(ticks);

        return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a value to DateOnly.
    /// </summary>
    private static DateOnly ConvertToDateOnly(object value, string? dateFormat)
    {
        if (value is DateOnly dateOnly)
            return dateOnly;

        if (value is DateTime dt)
            return DateOnly.FromDateTime(dt);

        if (value is string str)
        {
            if (!string.IsNullOrEmpty(dateFormat))
            {
                if (DateOnly.TryParseExact(str, dateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedExact))
                    return parsedExact;
            }

            if (DateOnly.TryParse(str, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
        }

        var dateTime = ConvertToDateTime(value, dateFormat);
        return DateOnly.FromDateTime(dateTime);
    }

    /// <summary>
    /// Converts a value to TimeOnly.
    /// </summary>
    private static TimeOnly ConvertToTimeOnly(object value)
    {
        if (value is TimeOnly timeOnly)
            return timeOnly;

        if (value is TimeSpan ts)
            return TimeOnly.FromTimeSpan(ts);

        if (value is DateTime dt)
            return TimeOnly.FromDateTime(dt);

        if (value is string str)
        {
            if (TimeOnly.TryParse(str, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
        }

        return TimeOnly.MinValue;
    }

    /// <summary>
    /// Converts a value to TimeSpan.
    /// </summary>
    private static TimeSpan ConvertToTimeSpan(object value)
    {
        if (value is TimeSpan ts)
            return ts;

        if (value is TimeOnly to)
            return to.ToTimeSpan();

        if (value is string str)
        {
            if (TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
        }

        if (value is long ticks)
            return new TimeSpan(ticks);

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Converts a value to an Enum type.
    /// </summary>
    private static object ConvertToEnum(object value, Type enumType)
    {
        if (value is string str)
        {
            if (Enum.TryParse(enumType, str, ignoreCase: true, out var result))
                return result!;
        }

        // Try to convert numeric value
        try
        {
            var underlyingType = Enum.GetUnderlyingType(enumType);
            var numericValue = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);

            if (Enum.IsDefined(enumType, numericValue))
                return Enum.ToObject(enumType, numericValue);
        }
        catch
        {
            // Fall through to default
        }

        // Return first enum value as default
        var values = Enum.GetValues(enumType);
        return values.Length > 0 ? values.GetValue(0)! : Activator.CreateInstance(enumType)!;
    }

    /// <summary>
    /// Converts a value to a numeric type.
    /// </summary>
    private static object ConvertToNumeric(object value, Type targetType)
    {
        // Handle string numeric representations
        if (value is string str)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str))
                return GetDefaultValue(targetType)!;

            // Remove currency symbols and formatting
            str = str.Replace(",", "").Replace("$", "").Replace("€", "");
        }

        try
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return GetDefaultValue(targetType)!;
        }
    }

    /// <summary>
    /// Converts a value to byte array.
    /// </summary>
    private static byte[]? ConvertToByteArray(object value)
    {
        if (value is byte[] bytes)
            return bytes;

        if (value is string base64)
        {
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return System.Text.Encoding.UTF8.GetBytes(base64);
            }
        }

        return null;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if a type is nullable.
    /// </summary>
    public static bool IsNullableType(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    /// <summary>
    /// Checks if a type is a numeric type.
    /// </summary>
    public static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType == typeof(byte) ||
               underlyingType == typeof(sbyte) ||
               underlyingType == typeof(short) ||
               underlyingType == typeof(ushort) ||
               underlyingType == typeof(int) ||
               underlyingType == typeof(uint) ||
               underlyingType == typeof(long) ||
               underlyingType == typeof(ulong) ||
               underlyingType == typeof(float) ||
               underlyingType == typeof(double) ||
               underlyingType == typeof(decimal);
    }

    /// <summary>
    /// Gets the default value for a type.
    /// </summary>
    public static object? GetDefaultValue(Type type)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type);
        return null;
    }

    /// <summary>
    /// Safely converts an object to the specified type.
    /// Returns default value on failure.
    /// </summary>
    public static T? SafeConvert<T>(object? value, T? defaultValue = default)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;

        try
        {
            var result = ConvertValue(value, typeof(T));
            return result != null ? (T)result : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    #endregion
}
