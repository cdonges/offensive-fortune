namespace offensive_fortune;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var path = context.Request.Path.Value ?? "/";
        
        // Only log requests to the root path
        if (path == "/")
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            
            var requestLog = new RequestLog
            {
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Path = path,
                Method = context.Request.Method
            };

            await dbContext.RequestLogs.AddAsync(requestLog);
            await dbContext.SaveChangesAsync();
        }

        await next(context);
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
