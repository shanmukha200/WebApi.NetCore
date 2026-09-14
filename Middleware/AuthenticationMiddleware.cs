namespace WebApi.NetCore.Middleware;

public class AuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var hasHeader = context.Request.Headers.TryGetValue("Authorization", out var authorization);

        if (hasHeader && !authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid authorization scheme." });
            return;
        }

        await next(context);
    }
}
