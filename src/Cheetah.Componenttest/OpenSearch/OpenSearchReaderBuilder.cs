using System;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.OpenSearch;


public class OpenSearchReaderBuilder
{
    public static OpenSearchReaderBuilder<T> Create<T>() where T : class
    {
        return new OpenSearchReaderBuilder<T>();
    }
    
    private OpenSearchReaderBuilder(){}
}

public class OpenSearchReaderBuilder<T> where T : class
{
    private const string OPENSEARCH_URL = "OPENSEARCH:URL";
    private const string OPENSEARCH_CLIENTID = "OPENSEARCH:CLIENTID";
    private const string OPENSEARCH_CLIENTSECRET = "OPENSEARCH:CLIENTSECRET";
    private const string OPENSEARCH_AUTH_ENDPOINT = "OPENSEARCH:AUTHENDPOINT";
    private string? OpenSearchConfigurationPrefix;
    private IConfiguration? Configuration;
    private string? IndexName;
    private string? IndexPrefix;

    internal OpenSearchReaderBuilder()
    {
        
    }
    
    public OpenSearchReaderBuilder<T> WithOpenSearchConfigurationPrefix(string prefix, IConfiguration configuration)
    {
        Configuration = configuration;
        OpenSearchConfigurationPrefix = prefix;
        return this;
    }

    public OpenSearchReaderBuilder<T> WithIndex(string indexName)
    {
        IndexName = indexName;
        return this;
    }

    public OpenSearchReaderBuilder<T> WithPrefix(string indexPrefix)
    {
        IndexPrefix = indexPrefix;
        return this;
    }

    public OpenSearchReader<T> Build()
    {
        var reader = new OpenSearchReader<T>()
        {
            IndexName = IndexName
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
            throw new InvalidOperationException("OpenSearchConfigurationPrefix or Configuration is not set");
        }
        
        reader.Prepare();
        return reader;
    }
}
