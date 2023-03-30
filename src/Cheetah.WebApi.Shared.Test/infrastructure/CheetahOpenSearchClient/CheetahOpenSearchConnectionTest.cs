using System;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Cheetah.WebApi.Shared_test.infrastructure.CheetahOpenSearchClient
{
    public class CheetahOpenSearchConnectionTest
    {
        [Fact]
        public async Task GetAccessToken_CacheHit_ReturnsCachedToken()
        {
            // Arrange
            var cacheMock = new Mock<IDistributedCache>();
            var accessToken = "cached-access-token";
            cacheMock.Setup(c => c.GetStringAsync("my-access-token"))
                     .ReturnsAsync(accessToken);

            // Create a TokenService instance using the mocked cache
            var tokenService = new TokenService(cacheMock.Object);

            // Act
            var result = await tokenService.GetAccessToken();

            // Assert
            Assert.Equal(accessToken, result);
            cacheMock.Verify(c => c.GetStringAsync("my-access-token"), Times.Once);
            cacheMock.Verify(c => c.SetStringAsync(
                "my-access-token",
                accessToken,
                It.IsAny<DistributedCacheEntryOptions>()), Times.Never);
        }

        [Fact]
        public async Task GetAccessToken_CacheMiss_ReturnsNewTokenAndCachesIt()
        {
            // Arrange
            var cacheMock = new Mock<IDistributedCache>();
            cacheMock.Setup(c => c.GetStringAsync("my-access-token"))
                     .ReturnsAsync((string)null);

            var tokenResponse = new TokenResponse
            {
                AccessToken = "new-access-token",
                ExpiresIn = 3600,
                TokenType = "Bearer"
            };
            var tokenClientMock = new Mock<TokenClient>("https://your-auth-server.com/connect/token", "your-client-id", "your-client-secret");
            tokenClientMock.Setup(c => c.RequestClientCredentialsAsync("your-scope"))
                           .ReturnsAsync(tokenResponse);

            // Create a TokenService instance using the mocked cache and token client
            var tokenService = new TokenService(cacheMock.Object, tokenClientMock.Object);

            // Act
            var result = await tokenService.GetAccessToken();

            // Assert
            Assert.Equal(tokenResponse.AccessToken, result);
            cacheMock.Verify(c => c.GetStringAsync("my-access-token"), Times.Once);
            cacheMock.Verify(c => c.SetStringAsync(
                "my-access-token",
                tokenResponse.AccessToken,
                It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpiration == DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn))), Times.Once);
            tokenClientMock.Verify(c => c.RequestClientCredentialsAsync("your-scope"), Times.Once);
        }
    }
}