using System.Data;
using MySqlConnector;

namespace Core.Sql.Models;

/// <summary>
/// Represents a single SQL parameter for stored procedure or query execution.
/// </summary>
public class SqlParamModel
{
    /// <summary>
    /// The name of the parameter (e.g., "@userId", "p_UserId").
    /// </summary>
    public string ParamName { get; set; } = string.Empty;

    /// <summary>
    /// The value to be passed to the parameter.
    /// </summary>
    public object? ParamValue { get; set; }

    /// <summary>
    /// Optional: The MySQL data type. If null, MySqlConnector will infer the type.
    /// </summary>
    public MySqlDbType? DbType { get; set; }

    /// <summary>
    /// The direction of the parameter (Input, Output, InputOutput, ReturnValue).
    /// Default is Input.
    /// </summary>
    public ParameterDirection Direction { get; set; } = ParameterDirection.Input;

    /// <summary>
    /// Optional: Size of the parameter for string/binary types.
    /// </summary>
    public int? Size { get; set; }

    /// <summary>
    /// Creates a new SqlParamModel instance.
    /// </summary>
    public SqlParamModel() { }

    /// <summary>
    /// Creates a new SqlParamModel with specified values.
    /// </summary>
    public SqlParamModel(string paramName, object? paramValue, MySqlDbType? dbType = null,
        ParameterDirection direction = ParameterDirection.Input, int? size = null)
    {
        ParamName = paramName;
        ParamValue = paramValue;
        DbType = dbType;
        Direction = direction;
        Size = size;
    }

    /// <summary>
    /// Factory method for creating an Input parameter.
    /// </summary>
    public static SqlParamModel Input(string name, object? value, MySqlDbType? dbType = null)
        => new(name, value, dbType, ParameterDirection.Input);

    /// <summary>
    /// Factory method for creating an Output parameter.
    /// </summary>
    public static SqlParamModel Output(string name, MySqlDbType dbType, int? size = null)
        => new(name, null, dbType, ParameterDirection.Output, size);

    /// <summary>
    /// Factory method for creating an InputOutput parameter.
    /// </summary>
    public static SqlParamModel InputOutput(string name, object? value, MySqlDbType dbType, int? size = null)
        => new(name, value, dbType, ParameterDirection.InputOutput, size);
}
