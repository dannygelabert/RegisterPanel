using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace RegisterPanel.Api.Extensions;

/// <summary>
/// Injects a JWT Bearer security scheme into the OpenAPI document so Scalar
/// can display the authentication UI and include the token in requests.
/// </summary>
public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type        = SecuritySchemeType.Http,
            Scheme      = "bearer",
            BearerFormat = "JWT",
            In          = ParameterLocation.Header,
            Description = "Enter your JWT Bearer token."
        };

        return Task.CompletedTask;
    }
}
