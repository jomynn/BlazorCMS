using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class AddAuthHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any()
            || context.MethodInfo.GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();

        if (hasAuthorize)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                }
            };
        }
    }
}
