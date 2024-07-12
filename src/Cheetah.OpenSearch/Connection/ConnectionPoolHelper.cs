using System;
using System.Linq;
using OpenSearch.Net;

namespace Cheetah.OpenSearch.Connection
{
    /// <summary>
    /// Helper class for creating <see cref="IConnectionPool"/> instances
    /// </summary>
    public static class ConnectionPoolHelper
    {
        /// <summary>
        /// Create a <see cref="IConnectionPool"/> from a url
        /// </summary>
        /// <param name="url">The url for OpenSearch, if using multiple OpenSearch instances, supply a comma-separated list of URLs</param>
        /// <returns></returns>
        public static IConnectionPool GetConnectionPool(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (url.Contains(','))
            {
                return new StaticConnectionPool(url.Split(',').Select(x => new Uri(x)));
            }

            return new SingleNodeConnectionPool(new Uri(url));
        }
    }
}
