using Microsoft.AspNetCore.Http;

namespace Platypus.WebApi.Shared.Middleware.Startup;

public class StartupTasksMiddleware : IMiddleware
{
    private readonly StartupTaskContext context;

    public StartupTasksMiddleware(StartupTaskContext context)
    {
        this.context = context;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        if (context.IsComplete)
        {
            await next(httpContext);
        }
        else
        {
            var response = httpContext.Response;
            response.StatusCode = 503;
            response.Headers["Retry-After"] = "30";
            await response.WriteAsync("Service Unavailable");
        }
    }
}