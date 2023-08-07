using System;
using Cheetah.Core.Config;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.OpenSearch
{
    public class OpenSearchClientBuilder
    {
        private const string ADDRESS = "URL";
        private const string CLIENT_ID = "CLIENTID";
        private const string CLIENT_SECRET = "CLIENTSECRET";
        private const string OAUTHSCOPE = "OAUTHSCOPE";
        private const string AUTH_ENDPOINT = "AUTHENDPOINT";
        private string ConfigurationPrefix = "";
        private IConfiguration? Configuration;

        private OpenSearchClientBuilder()
        {
        }

        public static OpenSearchClientBuilder Create()
        {
            return new OpenSearchClientBuilder();
        }

        public OpenSearchClientBuilder WithOpenSearchConfigurationPrefix(IConfiguration configuration, string prefix = "")
        {
            Configuration = configuration;
            ConfigurationPrefix = prefix;
            return this;
        }

        public OpenSearchClient Build()
        {
            var configuration = Configuration ?? throw new InvalidOperationException("OpenSeach configuration must be set");
            // im not entirely sure what the point of ConfigurationPrefix is so if youre getting errors trying to use it,
            // this is probably a good place to look
            var configurationSection = configuration.GetSection(ConfigurationPrefix + "OPENSEARCH");

            var osAddress = configurationSection.GetValue<string>(ADDRESS);
            var clientId = configurationSection.GetValue<string>(CLIENT_ID);
            var clientSecret = configurationSection.GetValue<string>(CLIENT_SECRET);
            var oauthScope = configurationSection.GetValue<string?>(OAUTHSCOPE);
            var authEndpoint = configurationSection.GetValue<string>(AUTH_ENDPOINT);

            var config = new OpenSearchConfig
            {
                AuthMode = OpenSearchConfig.OpenSearchAuthMode.OAuth2,
                Url = osAddress,
                ClientId = clientId,
                ClientSecret = clientSecret,
                OAuthScope = oauthScope,
                TokenEndpoint = authEndpoint
            };

            return new OpenSearchClient(config);
        }
    }
}
