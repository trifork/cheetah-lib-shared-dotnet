using System;
using System.Linq;
using OpenSearch.Net;

namespace Cheetah.OpenSearch.Connection
{
    public static class ConnectionPoolHelper
    {
        public static IConnectionPool GetConnectionPool(string url)
        {
            if (url.Contains(','))
            {
                return new StaticConnectionPool(url.Split(',').Select(x => new Uri(x)));
            }

            return new SingleNodeConnectionPool(new Uri(url));
        }
    }
}
