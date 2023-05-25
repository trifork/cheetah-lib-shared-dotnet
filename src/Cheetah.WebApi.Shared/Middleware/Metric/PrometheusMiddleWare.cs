using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Cheetah.WebApi.Shared.Middleware.Metric
{
    public class PrometheusMiddleWare
    {
        private readonly RequestDelegate _request;

        /// <summary>
        /// Instantiate a Prometheus MiddleWare class to handle HTTP requests
        /// </summary>
        /// <param name="request"> Request delegate</param>
        public PrometheusMiddleWare(RequestDelegate request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// Invoke a httpContext to the Metric Reporter to register request and register response time.
        /// </summary>
        public async Task Invoke(HttpContext httpContext, IMetricReporter reporter)
        {
            var path = httpContext.Request.Path.Value;
            if (path == "/metrics")
            {
                await _request.Invoke(httpContext);
                return;
            }
            var sw = Stopwatch.StartNew();

            try
            {
                await _request.Invoke(httpContext);
            }
            finally
            {
                sw.Stop();
                reporter.RegisterRequest();
                reporter.RegisterResponseTime(
                    httpContext.Response.StatusCode,
                    httpContext.Request.Method,
                    sw.Elapsed
                );
            }
        }
    }
}
