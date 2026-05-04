using RegisterPanel.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;
using System.Text.Json;

namespace RegisterPanel.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            Log.Error(ex, "Domain exception. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteDomainExceptionResponse(context, ex);
        }
        catch (FluentValidation.ValidationException ex)
        {
            Log.Error(ex, "Validation exception. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteValidationExceptionResponse(context, ex);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteInternalServerErrorResponse(context, ex);
        }
    }

    private static async Task WriteDomainExceptionResponse(HttpContext context, DomainException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        context.Response.ContentType = "application/json";

        object response = new { errors = new[] { ex.Message } };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static async Task WriteValidationExceptionResponse(
        HttpContext context,
        FluentValidation.ValidationException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        Dictionary<string, string[]> errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        object response = new { errors };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private async Task WriteInternalServerErrorResponse(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/problem+json";

        ProblemDetails problem = new()
        {
            Type    = "https://tools.ietf.org/html/rfc7807",
            Title   = "Internal Server Error",
            Status  = (int)HttpStatusCode.InternalServerError,
            Detail  = _environment.IsDevelopment()
                        ? ex.ToString()
                        : "An unexpected error occurred."
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
