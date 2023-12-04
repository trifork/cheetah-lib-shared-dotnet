using System;
using System.Net.Http;

namespace Cheetah.Auth.Util
{
    /// <summary>
    /// Default implementation of <see cref="IHttpClientFactory"/>
    /// </summary>
    public sealed class DefaultHttpClientFactory : IHttpClientFactory, IDisposable
    {
        private readonly Lazy<HttpMessageHandler> _handlerLazy = new Lazy<HttpMessageHandler>(() => new HttpClientHandler());

        /// <summary>
        /// Create a <see cref="HttpClient"/> with the supplied name/>
        /// </summary>
        /// <param name="name">The name to use for the <see cref="HttpClient"/></param>
        /// <returns>The created <see cref="HttpClient"/></returns>
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handlerLazy.Value, disposeHandler: false);
        }

        /// <summary>
        /// Dispose of the factory and any resources it holds
        /// </summary>
        public void Dispose()
        {
            if (_handlerLazy.IsValueCreated)
            {
                _handlerLazy.Value.Dispose();
            }
        }
    }
}
