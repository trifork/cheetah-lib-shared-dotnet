using System;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.OpenSearch;

public class OpenSearchReaderBuilder
{
    private const string OPENSEARCH_URL = "OPENSEARCH:URL";
    private const string OPENSEARCH_CLIENTID = "OPENSEARCH:CLIENTID";
    private const string OPENSEARCH_CLIENTSECRET = "OPENSEARCH:CLIENTSECRET";
    private const string OPENSEARCH_AUTH_ENDPOINT = "OPENSEARCH:AUTHENDPOINT";
    private string? OpenSearchConfigurationPrefix;
    private IConfiguration? Configuration;
    private string? IndexName;

    public static OpenSearchReaderBuilder Create()
    {
        return new OpenSearchReaderBuilder();
    }
    
    public OpenSearchReaderBuilder WithOpenSearchConfugurationPrefix(string prefix, IConfiguration configuration)
    {
        Configuration = configuration;
        OpenSearchConfigurationPrefix = prefix;
        return this;
    }

    public OpenSearchReaderBuilder WithIndex(string indexName)
    {
        IndexName = indexName;
        return this;
    }

    public OpenSearchReader Build()
    {
        var reader = new OpenSearchReader()
        {
            Index = IndexName
        };

        if (OpenSearchConfigurationPrefix != null && Configuration != null)
        {
            if (!string.IsNullOrEmpty(OpenSearchConfigurationPrefix))
            {
                reader.Server = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_URL);
                reader.ClientId = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_CLIENTID);
                reader.ClientSecret = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_CLIENTSECRET);
                reader.AuthEndpoint = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_AUTH_ENDPOINT);
            }
            else
            {
                reader.Server = Configuration.GetValue<string>(OPENSEARCH_URL);
                reader.ClientId = Configuration.GetValue<string>(OPENSEARCH_CLIENTID);
                reader.ClientSecret = Configuration.GetValue<string>(OPENSEARCH_CLIENTSECRET);
                reader.AuthEndpoint = Configuration.GetValue<string>(OPENSEARCH_AUTH_ENDPOINT);   
            }
        }
        else
        {
            throw new InvalidOperationException("KafkaConfigurationPrefix or Configuration is not set");
        }
        
        reader.Prepare();
        return reader;
    }
}
