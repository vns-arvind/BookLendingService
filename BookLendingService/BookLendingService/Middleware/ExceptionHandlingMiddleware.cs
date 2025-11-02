using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BookLending.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            // Handle FluentValidation errors gracefully
            _logger.LogWarning(ex, "Validation failed");
            await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Validation failed", ex.Errors);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await WriteProblemDetailsAsync(context, HttpStatusCode.NotFound, "Resource not found", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            await WriteProblemDetailsAsync(context, HttpStatusCode.Unauthorized, "Unauthorized", ex.Message);
        }
        catch (Exception ex)
        {
            // Unexpected exception
            _logger.LogError(ex, "Unhandled exception occurred");

            await WriteProblemDetailsAsync(context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                appMessage: ex.Message);
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        object? appMessage = null)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new ProblemDetails
        {
            Title = title,
            Status = (int)statusCode,
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Detail = appMessage?.ToString(),
            Instance = context.Request.Path
        };

        // Customize response serialization
        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
