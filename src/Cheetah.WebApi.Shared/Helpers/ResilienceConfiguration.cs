using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Cheetah.WebApi.Shared.Helpers
{
    public static class ResilienceConfiguration
    {
        /// <summary>
        /// Adds a resilient and transient-fault handling policy
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy<TService>(
            IServiceProvider serviceProvider,
            int retryCount = 6
        )
        {
            ILogger<TService> logger = serviceProvider.GetService<ILogger<TService>>() ?? throw new InvalidOperationException("Logger must not be null");
            return GetRetryPolicy(logger, retryCount, -1);
        }

        /// <summary>
        /// Adds a resilient and transient-fault handling policy
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
            ILogger logger,
            int retryCount = 6,
            int secondsBetweenRetries = -1
        )
        {
            TimeSpan SleepDurationProvider(int retryAttempt)
            {
                if (secondsBetweenRetries > 0)
                {
                    return TimeSpan.FromSeconds(secondsBetweenRetries);
                }
                else
                {
                    Random jitterer = new();
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // exponential back-off: 2, 4, 8 etc
                        + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)); // added some jitter
                }
            }

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.Unauthorized)
                .Or<TimeoutRejectedException>()
                .OrInner<SocketException>() //Service is possibly not ready
                .Or<SocketException>() //Service is possibly not ready
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound) //Service is possibly not ready
                .WaitAndRetryAsync(
                    retryCount,
                    SleepDurationProvider,
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        logger.LogWarning(
                            "Delaying for {delay} ms, then making retry {retry}.",
                            timespan.TotalMilliseconds,
                            retryAttempt
                        );
                    }
                );
        }
    }
}
