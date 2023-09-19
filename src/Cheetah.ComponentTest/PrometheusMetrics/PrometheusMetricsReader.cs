using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Observability.ComponentTest.PrometheusMetrics
{
    public class PrometheusMetricsReader : IDisposable
    {
        private readonly HttpClient httpClient;

        public PrometheusMetricsReader(string host, int port = 9249)
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://" + host + ":" + port)
            };
        }

        public async Task<Dictionary<string, string>> GetMetricsAsync(bool logMetricsLines = false)
        {
            var stream = await httpClient.GetStreamAsync("");
            var metrics = new Dictionary<string, string>();
            using var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                {
                    continue;
                }
                if (logMetricsLines)
                {
                    Console.WriteLine(line);
                }
                var split = line.LastIndexOf(' ');
                metrics.Add(line[..(split - 1)], line[split..]);
            }
            return metrics;
        }

        public async Task<Dictionary<string, string>> GetMetricsAsync(string contains, bool logMetricsLines = false)
        {
            var stream = await httpClient.GetStreamAsync("");
            var metrics = new Dictionary<string, string>();
            using var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#") || !line.Contains(contains))
                {
                    continue;
                }
                if(logMetricsLines)
                {
                    Console.WriteLine(line);
                }
                var split = line.LastIndexOf(' ');
                metrics.Add(line[..(split - 1)], line[split..]);
            }
            return metrics;
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}
