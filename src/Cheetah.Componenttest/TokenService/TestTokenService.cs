using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Core.Infrastructure.Auth;
using IdentityModel.Client;

namespace Cheetah.ComponentTest.TokenService
{
    public class TestTokenService : ITokenService
    {
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
            return !tokenResponse.IsError ? tokenResponse : throw tokenResponse.Exception; // Get the access token from the token response
        }
    }
}
