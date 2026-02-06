namespace Core.Sql.Models;

/// <summary>
/// Model for executing a single stored procedure or SQL query.
/// </summary>
public class SqlExecuteModel
{
    /// <summary>
    /// The stored procedure name or SQL query to execute.
    /// </summary>
    public string SqlProcOrQuery { get; set; } = string.Empty;

    /// <summary>
    /// The list of parameters to pass to the stored procedure or query.
    /// </summary>
    public List<SqlParamModel> Params { get; set; } = [];

    /// <summary>
    /// Indicates whether this is a stored procedure (true) or raw SQL query (false).
    /// Default is true (stored procedure).
    /// </summary>
    public bool IsStoredProcedure { get; set; } = true;

    /// <summary>
    /// Optional command timeout in seconds. If null, uses default connection timeout.
    /// </summary>
    public int? CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// Creates a new empty SqlExecuteModel.
    /// </summary>
    public SqlExecuteModel() { }

    /// <summary>
    /// Creates a new SqlExecuteModel for a stored procedure.
    /// </summary>
    public SqlExecuteModel(string sqlProcOrQuery, List<SqlParamModel>? parameters = null,
        bool isStoredProcedure = true)
    {
        SqlProcOrQuery = sqlProcOrQuery;
        Params = parameters ?? [];
        IsStoredProcedure = isStoredProcedure;
    }

    /// <summary>
    /// Factory method for creating a stored procedure execution model.
    /// </summary>
    public static SqlExecuteModel StoredProcedure(string procedureName, params SqlParamModel[] parameters)
        => new(procedureName, [.. parameters], true);

    /// <summary>
    /// Factory method for creating a raw SQL query execution model.
    /// </summary>
    public static SqlExecuteModel RawQuery(string query, params SqlParamModel[] parameters)
        => new(query, [.. parameters], false);

    /// <summary>
    /// Adds a parameter to this execution model.
    /// </summary>
    public SqlExecuteModel AddParam(SqlParamModel param)
    {
        Params.Add(param);
        return this;
    }

    /// <summary>
    /// Adds an input parameter to this execution model.
    /// </summary>
    public SqlExecuteModel AddInput(string name, object? value)
    {
        Params.Add(SqlParamModel.Input(name, value));
        return this;
    }

    /// <summary>
    /// Adds an output parameter to this execution model.
    /// </summary>
    public SqlExecuteModel AddOutput(string name, MySqlConnector.MySqlDbType dbType, int? size = null)
    {
        Params.Add(SqlParamModel.Output(name, dbType, size));
        return this;
    }
}
