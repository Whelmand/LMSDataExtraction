namespace LMSDataExtraction.Api.Middleware;

public class BearerTokenMiddleware
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;

    public BearerTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        string authHeader = context.Request.Headers[AuthorizationHeader].ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
        {
            await WriteUnauthorized(context, "Authorization header ontbreekt.");
            return;
        }

        if (!authHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await WriteUnauthorized(context, "Authorization header moet beginnen met 'Bearer '.");
            return;
        }

        string token = authHeader.Substring(BearerPrefix.Length).Trim();

        if (string.IsNullOrEmpty(token))
        {
            await WriteUnauthorized(context, "Bearer token is leeg.");
            return;
        }

        await _next(context);
    }

    private static bool IsPublicPath(PathString path)
    {
        if (path.StartsWithSegments("/swagger"))
        {
            return true;
        }

        if (path.StartsWithSegments("/health"))
        {
            return true;
        }

        return false;
    }

    private static async Task WriteUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"" + message + "\"}");
    }
}
