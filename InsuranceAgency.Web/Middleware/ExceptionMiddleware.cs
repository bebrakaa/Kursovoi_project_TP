using System.Net;
using System.Text.Json;
using InsuranceAgency.Application.Common.Exceptions;

namespace InsuranceAgency.Web.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Don't modify response if it has already started
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case NotFoundException notFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new { error = notFoundException.Message });
                break;
            case Domain.Exceptions.ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = validationException.Message });
                break;
            case Domain.Exceptions.DomainException domainException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = domainException.Message });
                break;
            default:
                result = JsonSerializer.Serialize(new { error = "An error occurred while processing your request" });
                break;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}

