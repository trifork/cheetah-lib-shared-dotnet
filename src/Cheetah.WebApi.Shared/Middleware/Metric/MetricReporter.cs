using System;
using Prometheus;

namespace Cheetah.WebApi.Shared.Middleware.Metric
{
    public class MetricReporter : IMetricReporter
    {
        private readonly Counter _requestCounter;
        private readonly Histogram _responseTimeHistogram;

        /// <summary>
        /// Instantiate a MetricReporter that consists of a Prometheus metric counter and Prometheus histogram
        /// The counter represents the total number of requests serviced by this API.
        /// The Histogram represents the duration in seconds between the response to a request.
        /// </summary>
        public MetricReporter()
        {
            _requestCounter = Metrics.CreateCounter(
                "total_requests",
                "The total number of requests serviced by this API."
            );

            _responseTimeHistogram = Metrics.CreateHistogram(
                "request_duration_seconds",
                "The duration in seconds between the response to a request.",
                new HistogramConfiguration
                {
                    Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
                    LabelNames = new[] { "status_code", "method" }
                }
            );
        }

        /// <summary>
        /// Increments the request counter
        /// </summary>
        public void RegisterRequest()
        {
            _requestCounter.Inc();
        }

        /// <summary>
        /// Register a response time in the MetricReporter response time histogram with the status code, method and the elapsed response time.
        /// </summary>
        /// <param name="statusCode"> The status code</param>
        /// <param name="method"> The method name</param>
        /// <param name="elasped"> Timespan elapsed </param>
        public void RegisterResponseTime(int statusCode, string method, TimeSpan elasped)
        {
            _responseTimeHistogram
                .Labels(statusCode.ToString(), method)
                .Observe(elasped.TotalSeconds);
        }
    }
}
