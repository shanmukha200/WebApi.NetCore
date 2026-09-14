using System.ComponentModel.DataAnnotations;
using System.Net;
using WebApi.NetCore.Dtos;

namespace WebApi.NetCore.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception occurred while processing request.");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            ValidationException => new ApiErrorResponse { Message = exception.Message },
            UnauthorizedAccessException => new ApiErrorResponse { Message = exception.Message },
            KeyNotFoundException => new ApiErrorResponse { Message = exception.Message },
            _ => new ApiErrorResponse { Message = "An unexpected error occurred.", Details = exception.Message }
        };

        context.Response.StatusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(response);
    }
}
