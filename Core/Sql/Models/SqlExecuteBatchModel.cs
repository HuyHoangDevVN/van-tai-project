namespace Core.Sql.Models;

/// <summary>
/// Model for executing a stored procedure multiple times in a batch (within a transaction).
/// </summary>
public class SqlExecuteBatchModel
{
    /// <summary>
    /// The stored procedure name or SQL query to execute for each batch item.
    /// </summary>
    public string SqlProcOrQuery { get; set; } = string.Empty;

    /// <summary>
    /// The list of parameter sets. Each item represents one execution of the procedure.
    /// </summary>
    public List<SqlParamBatchModel> BatchParams { get; set; } = [];

    /// <summary>
    /// Indicates whether this is a stored procedure (true) or raw SQL query (false).
    /// Default is true (stored procedure).
    /// </summary>
    public bool IsStoredProcedure { get; set; } = true;

    /// <summary>
    /// Optional command timeout in seconds for each execution. If null, uses default.
    /// </summary>
    public int? CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// If true, continues executing remaining batch items even if one fails.
    /// If false (default), rolls back entire transaction on first failure.
    /// </summary>
    public bool ContinueOnError { get; set; } = false;

    /// <summary>
    /// Creates a new empty SqlExecuteBatchModel.
    /// </summary>
    public SqlExecuteBatchModel() { }

    /// <summary>
    /// Creates a new SqlExecuteBatchModel with specified values.
    /// </summary>
    public SqlExecuteBatchModel(string sqlProcOrQuery, List<SqlParamBatchModel>? batchParams = null,
        bool isStoredProcedure = true)
    {
        SqlProcOrQuery = sqlProcOrQuery;
        BatchParams = batchParams ?? [];
        IsStoredProcedure = isStoredProcedure;
    }

    /// <summary>
    /// Factory method for creating a batch stored procedure execution model.
    /// </summary>
    public static SqlExecuteBatchModel StoredProcedure(string procedureName)
        => new(procedureName, [], true);

    /// <summary>
    /// Adds a batch item (set of parameters for one execution).
    /// </summary>
    public SqlExecuteBatchModel AddBatchItem(SqlParamBatchModel batchItem)
    {
        BatchParams.Add(batchItem);
        return this;
    }

    /// <summary>
    /// Adds a batch item from a list of parameters.
    /// </summary>
    public SqlExecuteBatchModel AddBatchItem(params SqlParamModel[] parameters)
    {
        BatchParams.Add(new SqlParamBatchModel(parameters));
        return this;
    }

    /// <summary>
    /// Gets the total number of batch items to execute.
    /// </summary>
    public int BatchCount => BatchParams.Count;
}
