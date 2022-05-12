using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometheus;
using System.Security.Cryptography.X509Certificates;

namespace Platypus.WebApi.Shared.Middleware.Metric
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
