using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace Cheetah.WebApi.Shared.Middleware.Metric
{
    public class KestrelMetricService : BackgroundService
    {
        KestrelMetricServer _kestrelserver;
        public KestrelMetricService(int port)
        {
            _kestrelserver = new KestrelMetricServer(port);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _kestrelserver.Start();
            return Task.CompletedTask;
        }
    }
}
