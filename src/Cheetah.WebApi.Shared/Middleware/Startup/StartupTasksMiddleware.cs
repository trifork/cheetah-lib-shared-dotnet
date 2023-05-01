using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Cheetah.WebApi.Shared.Middleware.Startup
{
  public class StartupTasksMiddleware : IMiddleware
  {
    private readonly StartupTaskContext context;

    public StartupTasksMiddleware(StartupTaskContext context)
    {
      this.context = context;
    }

    /// <summary>
    /// Invoke task async 
    /// </summary>
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
}