using System.Diagnostics;

namespace InsuranceAgency.Web.Middleware;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        await _next(context);

        stopwatch.Stop();

        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (elapsedMs > 1000) // Log slow requests
        {
            _logger.LogWarning(
                "Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                requestMethod,
                requestPath,
                elapsedMs);
        }
        else
        {
            _logger.LogInformation(
                "Request timing: {Method} {Path} - {ElapsedMs}ms",
                requestMethod,
                requestPath,
                elapsedMs);
        }

        // Add timing header to response (only if response hasn't started)
        if (!context.Response.HasStarted)
        {
            context.Response.Headers.Append("X-Response-Time", $"{elapsedMs}ms");
        }
    }
}

