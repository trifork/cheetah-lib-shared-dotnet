
namespace Platypus.WebApi.Shared.Middleware.Metric
{
    public interface IMetricReporter
    {
        void RegisterRequest();
        void RegisterResponseTime(int statusCode, string method, TimeSpan elasped);
    }
}