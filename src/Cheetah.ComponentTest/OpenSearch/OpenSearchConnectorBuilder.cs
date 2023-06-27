using System;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.OpenSearch;

public class OpenSearchConnectorBuilder
{
    private const string OPENSEARCH_URL = "OPENSEARCH:URL";
    private const string OPENSEARCH_CLIENTID = "OPENSEARCH:CLIENTID";
    private const string OPENSEARCH_CLIENTSECRET = "OPENSEARCH:CLIENTSECRET";
    private const string OPENSEARCH_AUTH_ENDPOINT = "OPENSEARCH:AUTHENDPOINT";
    private string? OpenSearchConfigurationPrefix;
    private IConfiguration? Configuration;

    private OpenSearchConnectorBuilder()
    {
        
    }
    
    public static OpenSearchConnectorBuilder Create() 
    {
        return new OpenSearchConnectorBuilder();
    }
    
    public OpenSearchConnectorBuilder WithOpenSearchConfigurationPrefix(IConfiguration configuration, string prefix = "")
    {
        Configuration = configuration;
        OpenSearchConfigurationPrefix = prefix;
        return this;
    }

    public OpenSearchConnector Build()
    {
        var connector = new OpenSearchConnector()
        {
        };

        if (OpenSearchConfigurationPrefix != null && Configuration != null)
        {
            if (!string.IsNullOrEmpty(OpenSearchConfigurationPrefix))
            {
                connector.Server = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_URL);
                connector.ClientId = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_CLIENTID);
                connector.ClientSecret = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_CLIENTSECRET);
                connector.AuthEndpoint = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_AUTH_ENDPOINT);
            }
            else
            {
                connector.Server = Configuration.GetValue<string>(OPENSEARCH_URL);
                connector.ClientId = Configuration.GetValue<string>(OPENSEARCH_CLIENTID);
                connector.ClientSecret = Configuration.GetValue<string>(OPENSEARCH_CLIENTSECRET);
                connector.AuthEndpoint = Configuration.GetValue<string>(OPENSEARCH_AUTH_ENDPOINT);   
            }
        }
        else
        {
            throw new InvalidOperationException("OpenSearchConfigurationPrefix or Configuration is not set");
        }
        
        connector.Prepare();
        return connector;
    }
}
