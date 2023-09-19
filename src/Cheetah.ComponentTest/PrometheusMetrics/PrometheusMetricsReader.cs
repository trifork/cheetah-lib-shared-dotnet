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


        /// <summary>
        /// Creates a reader allowing to read from a prometheus endpoint
        /// </summary>
        /// <param name="host">The host to connect to</param>
        /// <param name="port">The port to connect to, defaults to 9249</param>
        public PrometheusMetricsReader(string host, int port = 9249)
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://" + host + ":" + port)
            };
        }

        /// <summary>
        /// Returns all metrics returned by the metrics endpoint
        /// </summary>
        /// <param name="logMetricsLines">Íf set to true, all lines not starting with # are logged to Console</param>
        /// <returns>All metrics returned by the metrics endpoint</returns>
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

        /// <summary>
        /// Returns all metrics containing the input string, returned by the metrics endpoint
        /// </summary>
        /// <param name="logMetricsLines">Íf set to true, all lines not starting with #, containing the input string are logged to Console</param>
        /// <param name="contains">The string which metrics should contain</param>
        /// <returns>All metrics returned by the metrics endpoint</returns>
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
                if (logMetricsLines)
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
