using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace Cheetah.WebApi.Shared.Middleware.Metric
{
  public class KestrelMetricService : BackgroundService
  {
    readonly KestrelMetricServer _kestrelserver;

    /// <summary>
    /// Instantiate a stand-alone Kestrel based metric server that only serves Prometheus requests.
    /// </summary>
    public KestrelMetricService(int port)
    {
      _kestrelserver = new KestrelMetricServer(port);
    }

    /// <summary>
    /// Start kestrel server.
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _kestrelserver.Start();
      return Task.CompletedTask;
    }
  }
}