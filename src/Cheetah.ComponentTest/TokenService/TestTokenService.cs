using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Core.Authentication;
using IdentityModel.Client;

namespace Cheetah.ComponentTest.TokenService
{
    public class TestTokenService : ITokenService
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

        public async Task<TokenResponse?> RequestAccessTokenCachedAsync(CancellationToken cancellationToken)
        {
            var tokenResponse = await RequestClientCredentialsTokenAsync(cancellationToken);
            return tokenResponse;
        }

        public async Task<TokenResponse> RequestClientCredentialsTokenAsync(CancellationToken cancellationToken)
        {
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
                .RequestClientCredentialsTokenAsync(cancellationToken: cancellationToken, scope: oauthScope)
                .ConfigureAwait(false);

            // Check if the token request was successful
            if (!tokenResponse.IsError)
            {
                return tokenResponse;
            }
            else
            {
                throw tokenResponse.Exception;
            }
        }
    }
}
