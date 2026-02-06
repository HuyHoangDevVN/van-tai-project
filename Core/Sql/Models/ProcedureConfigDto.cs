using Core.Sql.Models;

namespace Core.Sql.Models;

/// <summary>
/// DTO for Sys_ProcedureConfig table.
/// Maps function keys to stored procedure names.
/// </summary>
public class ProcedureConfigDto
{
    [CustomDataSet("FunctionKey")]
    public string FunctionKey { get; set; } = string.Empty;

    [CustomDataSet("ProcedureName")]
    public string ProcedureName { get; set; } = string.Empty;

    [CustomDataSet("Description")]
    public string? Description { get; set; }

    [CustomDataSet("ModuleName")]
    public string? ModuleName { get; set; }

    [CustomDataSet("IsActive")]
    public bool IsActive { get; set; } = true;
}
