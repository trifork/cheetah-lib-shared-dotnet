using System.Collections.Generic;

namespace Cheetah.ComponentTest.PrometheusMetrics
{
    public class PrometheusHistogram
    {
        public double Count { get; }
        public List<KeyValuePair<string, double>> Quantiles { get; }


        public PrometheusHistogram(double count)
        {
            Count = count;
            Quantiles = new List<KeyValuePair<string, double>>();
        }

        public void AddQuantile(string quantile, double value)
        {
            Quantiles.Add(new KeyValuePair<string, double>(quantile, value));
        }
    }
}
