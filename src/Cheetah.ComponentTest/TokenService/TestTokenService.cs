using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Authentication;
using IdentityModel.Client;

namespace Cheetah.ComponentTest.TokenService
{
    internal class TestTokenService : ITokenService
    {
        private readonly string tokenEndpoint;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string? oauthScope;

        public TestTokenService(string clientId, string clientSecret, string tokenEndpoint, string? oauthScope = null)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.tokenEndpoint = tokenEndpoint;
            this.oauthScope = oauthScope;
        }

        public async Task<(string AccessToken, long Expiration, string? PrincipalName)?> RequestAccessTokenAsync(CancellationToken cancellationToken)
        {
            var tokenResponse = await FetchAccessTokenAsync(cancellationToken);
            
            if(tokenResponse.AccessToken == null)
            {
                return null;
            }
            
            return (tokenResponse.AccessToken, DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn).ToUnixTimeMilliseconds(), null);
        }

        private async Task<TokenResponse> FetchAccessTokenAsync(CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();
            var tokenClient = new TokenClient(
                            httpClient,
                            new TokenClientOptions
                            {
                                Address = tokenEndpoint,
                                ClientId = clientId,
                                ClientSecret = clientSecret
                            }
                        );
            var tokenResponse = await tokenClient
                .RequestClientCredentialsTokenAsync(cancellationToken: cancellationToken, scope: oauthScope)
                .ConfigureAwait(false);

            // Check if the token request was successful
            if (tokenResponse.IsError)
            {
                throw tokenResponse.Exception ?? new OAuth2TokenException("Failed to retrieve access token, but the error had no accompagnying exception");
            }
            
            return tokenResponse;

        }
    }
}
