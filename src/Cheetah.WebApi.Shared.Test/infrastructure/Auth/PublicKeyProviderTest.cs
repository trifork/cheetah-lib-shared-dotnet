using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Cheetah.WebApi.Shared.Core.Config;
using Cheetah.WebApi.Shared.Infrastructure.Auth;
using Xunit.Abstractions;

namespace PublicKeyProviderTests
{
    public class PublicKeyProviderTest
    {
        private readonly ITestOutputHelper output;

        public PublicKeyProviderTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("https://oauth.cheetah.trifork.dev")]
        [InlineData("https://oauth.cheetah.trifork.dev/")]
        [InlineData("http://oauthsimulator:1751/")]
        public async Task GetKeys_WhenCalled_ReturnsJwks(string oauthUrl)
        {
            // Arrange
            var oauthConfig = new OAuthConfig { OAuthUrl = oauthUrl };
            var options = Options.Create(oauthConfig);
            var httpClient = CreateMockedHttpClient("{\"keys\":[{\"kty\":\"RSA\",\"n\":\"some-value\",\"e\":\"AQAB\"}]}");
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var publicKeyProvider = new PublicKeyProvider(options, httpClient, memoryCache);
            var clientId = "test-client-id";

            // Act
            var result = await publicKeyProvider.GetKeys(clientId);

            // Assert
            Assert.Single(result);
            Assert.Equal("RSA", result[0].Kty);
            Assert.Equal("some-value", result[0].N);
            Assert.Equal("AQAB", result[0].E);
        }

        private HttpClient CreateMockedHttpClient(string responseContent)
        {
            var messageHandler = new Mock<HttpMessageHandler>();
            messageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => output.WriteLine($"RequestUri: {req.RequestUri}"))
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                });

            return new HttpClient(messageHandler.Object);
        }
    }
}
