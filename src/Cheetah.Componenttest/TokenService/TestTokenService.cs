using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Core.Infrastructure.Auth;
using IdentityModel.Client;

namespace Cheetah.ComponentTest.TokenService
{
    public class TestTokenService : ITokenService
    {
        private static TokenResponse cachedResponse;
        private readonly string tokenEndpoint;
        private readonly string clientId;
        private readonly string clientSecret;

        public TestTokenService(string clientId, string clientSecret, string tokenEndpoint)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.tokenEndpoint = tokenEndpoint;
        }

        public async Task<TokenResponse?> RequestAccessTokenCachedAsync(CancellationToken cancellationToken)
        {
            var tokenResponse = await RequestClientCredentialsTokenAsync(cancellationToken);
            return tokenResponse;
        }

        public async Task<TokenResponse> RequestClientCredentialsTokenAsync(CancellationToken cancellationToken)
        {
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            var httpClient = new HttpClient();
            var tokenClient = new TokenClient(
                            httpClient,
                            new TokenClientOptions()
                            {
                                Address = tokenEndpoint,
                                ClientId = clientId,
                                ClientSecret = clientSecret
                            }
                        );
            var tokenResponse = await tokenClient
                .RequestClientCredentialsTokenAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            // Check if the token request was successful
            if (!tokenResponse.IsError)
            {
                cachedResponse = tokenResponse;
                return tokenResponse;
            }
            else
            {
                throw tokenResponse.Exception;
            }
        }
    }
}
