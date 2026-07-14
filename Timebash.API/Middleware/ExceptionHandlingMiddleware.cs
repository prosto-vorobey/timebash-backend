using System.Security.Claims;
using Timebash.Core.Exceptions;

namespace Timebash.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogInformation(
                exception,
                "Request {RequestMethod} {RequestPath} was cancelled for user {UserId}",
                context.Request.Method, 
                context.Request.Path,
                context.User?.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        catch (Exception exception)
        {
            if (exception is DomainExceptionBase) _logger.LogWarning(
                exception,
                "Client error for request {RequestMethod} {RequestPath} from user: {UserId}",
                context.Request.Method, 
                context.Request.Path, 
                context.User?.FindFirstValue(ClaimTypes.NameIdentifier));
            else _logger.LogError(
                exception,
                "Unhandled exception for request {RequestMethod} {RequestPath} from user: {UserId}",
                context.Request.Method, 
                context.Request.Path, 
                context.User?.FindFirstValue(ClaimTypes.NameIdentifier));

            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;

        var (status, title, detail, extensions) = GetErrorInformation(exception);
        extensions["traceId"] = context.TraceIdentifier;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions = extensions,
        };

        return context.Response.WriteAsJsonAsync(problem);
    }

    private static (int, string, string, Dictionary<string, object?>) GetErrorInformation( Exception exception)
    {
        var extensions = new Dictionary<string, object?>();

        int status;
        string title;
        string detail;

        switch (exception)
        {
            case ResourceConflictException resourceEx:
                status = resourceEx.StatusCode;
                title = resourceEx.Title;
                detail = resourceEx.Message;
                extensions["field"] = resourceEx.Field;
                break;

            case DomainExceptionBase domainEx:
                status = domainEx.StatusCode;
                title = domainEx.Title;
                detail = domainEx.Message;
                break;

            default:
                status = StatusCodes.Status500InternalServerError;
                title = "Internal server error";
                detail = "An unexpected error occurred";
                break;
        }
        
        return (status, title, detail, extensions);
    }
}
