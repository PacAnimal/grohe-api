using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Application.Auth;

// swagger AuthorizeOperationFilter, assuming all controllers require authorization
public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType is null)
            return;

        var allowAnonymous = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
                             || context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

        if (allowAnonymous) return;
        
        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
        // operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

        var basicAuthScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "basic"
            }
        };
        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [basicAuthScheme] = Array.Empty<string>()
            }
        };
    }
}