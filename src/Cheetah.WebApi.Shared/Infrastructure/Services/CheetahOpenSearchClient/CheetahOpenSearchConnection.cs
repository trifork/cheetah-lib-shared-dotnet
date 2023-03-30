using IdentityModel.Client;
using Microsoft.Extensions.Caching.Distributed;
using OpenSearch.Net;

namespace Cheetah.Shared.WebApi.Infrastructure.Services.CheetahOpenSearchClient
{
    internal class CheetahOpenSearchConnection : HttpConnection
    {
        private readonly IDistributedCache cache;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tokenUrl;
        private readonly TokenClient tokenClient;
        public CheetahOpenSearchConnection(IDistributedCache cache, IHttpClientFactory httpClientfactory, string clientId, string clientSecret, string tokenEndpoint)
        {
            this.cache = cache;
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            _tokenUrl = tokenEndpoint ?? throw new ArgumentNullException(nameof(tokenEndpoint));

            // Todo: Should it be per request?
            var client = httpClientfactory.CreateClient("CheetahOpenSearchConnection");

            tokenClient = new TokenClient(client, new TokenClientOptions()
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
            });
        }

        protected virtual System.Net.Http.HttpMessageHandler InnerCreateHttpClientHandler(RequestData requestData) =>
                base.CreateHttpClientHandler(requestData);

        protected override System.Net.Http.HttpMessageHandler CreateHttpClientHandler(RequestData requestData) =>
            new OAuth2HttpClientHandler(cache, tokenClient, InnerCreateHttpClientHandler(requestData));

    }

    internal class OAuth2HttpClientHandler : DelegatingHandler
    {

        private readonly TokenClient tokenClient;
        private readonly IDistributedCache cache;

        private const string cacheKey = "opensearch-access-token";

        public OAuth2HttpClientHandler(IDistributedCache cache, TokenClient tokenClient, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.cache = cache;
            this.tokenClient = tokenClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            var cachedAccessToken = await this.cache.GetStringAsync(cacheKey);

            if (cachedAccessToken != null)
            {
                var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                // Check if the token request was successful
                if (!tokenResponse.IsError)
                {
                    // Get the access token from the token response
                    var accessToken = tokenResponse.AccessToken;

                    // Cache the access token
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                    };
                    await cache.SetStringAsync(cacheKey, accessToken, cacheOptions, cancellationToken).ConfigureAwait(false);

                    cachedAccessToken = accessToken;
                }
                else
                {
                    throw tokenResponse.Exception;
                }
            }

            request.Headers.Add("Authorization", $"Bearer {cachedAccessToken}");

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}