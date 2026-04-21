using Core.Sql.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Api.Security;

public class AdminAccessOptions
{
    public bool Enabled { get; set; } = true;
    public string RoleHeaderName { get; set; } = "X-User-Role";
    public string ApiKeyHeaderName { get; set; } = "X-Admin-Key";
    public string? ApiKey { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireAdminAccessAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var options = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<AdminAccessOptions>>()
            .Value;

        if (!options.Enabled)
        {
            return Task.CompletedTask;
        }

        var request = context.HttpContext.Request;
        var role = request.Headers[options.RoleHeaderName].FirstOrDefault();
        var apiKey = request.Headers[options.ApiKeyHeaderName].FirstOrDefault();

        var hasAdminRole = string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
        var hasValidApiKey = !string.IsNullOrWhiteSpace(options.ApiKey) &&
                             string.Equals(apiKey, options.ApiKey, StringComparison.Ordinal);

        if (hasAdminRole || hasValidApiKey)
        {
            return Task.CompletedTask;
        }

        context.Result = new ObjectResult(
            BaseResponse<object>.Error(
                "Bạn không có quyền truy cập chức năng quản trị.",
                StatusCodes.Status403Forbidden))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

        return Task.CompletedTask;
    }
}
