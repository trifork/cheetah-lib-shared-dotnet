using System;
using Cheetah.Core.Config;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.OpenSearch
{
    public class OpenSearchClientFactory
    {
        private const string OPENSEARCH_PREFIX = "OPENSEARCH:";
        private const string URL_KEY = OPENSEARCH_PREFIX + "URL";
        private const string CLIENT_ID_KEY = OPENSEARCH_PREFIX + "CLIENTID";
        private const string CLIENT_SECRET_KEY = OPENSEARCH_PREFIX + "CLIENTSECRET";
        private const string OAUTHSCOPE_KEY = OPENSEARCH_PREFIX + "OAUTHSCOPE";
        private const string AUTHENDPOINT_KEY = OPENSEARCH_PREFIX + "AUTHENDPOINT";
        
        private readonly IConfiguration _configuration;

        private OpenSearchClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static OpenSearchClientFactory Create(IConfiguration configuration)
        {
            return new OpenSearchClientFactory(configuration);
        }

        public OpenSearchClient Build()
        {
            var url = _configuration.GetValue<string>(URL_KEY);
            var clientId = _configuration.GetValue<string>(CLIENT_ID_KEY);
            var clientSecret = _configuration.GetValue<string>(CLIENT_SECRET_KEY);
            var oauthScope = _configuration.GetValue<string?>(OAUTHSCOPE_KEY);
            var authEndpoint = _configuration.GetValue<string>(AUTHENDPOINT_KEY);

            var config = new OpenSearchConfig
            {
                AuthMode = OpenSearchConfig.OpenSearchAuthMode.OAuth2,
                Url = url,
                ClientId = clientId,
                ClientSecret = clientSecret,
                OAuthScope = oauthScope,
                TokenEndpoint = authEndpoint
            };

            return new OpenSearchClient(config);
        }
    }
}
