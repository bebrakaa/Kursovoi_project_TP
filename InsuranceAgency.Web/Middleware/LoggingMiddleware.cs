namespace InsuranceAgency.Web.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        _logger.LogInformation(
            "Incoming request: {Method} {Path}",
            requestMethod,
            requestPath);

        try
        {
            await _next(context);

            _logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode}",
                requestMethod,
                requestPath,
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Request failed: {Method} {Path} - Error: {ErrorMessage}",
                requestMethod,
                requestPath,
                ex.Message);

            throw;
        }
    }
}

