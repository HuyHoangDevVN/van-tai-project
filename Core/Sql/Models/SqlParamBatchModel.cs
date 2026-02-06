namespace Core.Sql.Models;

/// <summary>
/// Represents a batch of SQL parameters for a single execution within a batch operation.
/// Each instance represents one set of parameters for one execution of the stored procedure.
/// </summary>
public class SqlParamBatchModel
{
    /// <summary>
    /// The list of parameters for this batch item.
    /// </summary>
    public List<SqlParamModel> Params { get; set; } = [];

    /// <summary>
    /// Creates a new empty SqlParamBatchModel.
    /// </summary>
    public SqlParamBatchModel() { }

    /// <summary>
    /// Creates a new SqlParamBatchModel with the specified parameters.
    /// </summary>
    public SqlParamBatchModel(List<SqlParamModel> parameters)
    {
        Params = parameters ?? [];
    }

    /// <summary>
    /// Creates a new SqlParamBatchModel from a params array.
    /// </summary>
    public SqlParamBatchModel(params SqlParamModel[] parameters)
    {
        Params = [.. parameters];
    }

    /// <summary>
    /// Adds a parameter to this batch item.
    /// </summary>
    public SqlParamBatchModel AddParam(SqlParamModel param)
    {
        Params.Add(param);
        return this;
    }

    /// <summary>
    /// Adds an input parameter to this batch item.
    /// </summary>
    public SqlParamBatchModel AddInput(string name, object? value)
    {
        Params.Add(SqlParamModel.Input(name, value));
        return this;
    }
}
