using System;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.OpenSearch;

public class OpenSearchWriterBuilder{

    public static OpenSearchWriterBuilder<T> Create<T>() where T : class
    {
        return new OpenSearchWriterBuilder<T>();
    }

    private OpenSearchWriterBuilder(){ }
}

public class OpenSearchWriterBuilder<T> where T : class
{
    private const string OPENSEARCH_URL = "OPENSEARCH:URL";
    private const string OPENSEARCH_CLIENTID = "OPENSEARCH:CLIENTID";
    private const string OPENSEARCH_CLIENTSECRET = "OPENSEARCH:CLIENTSECRET";
    private const string OPENSEARCH_AUTH_ENDPOINT = "OPENSEARCH:AUTHENDPOINT";
    private string? OpenSearchConfigurationPrefix;
    private IConfiguration? Configuration;
    private string? IndexName;
    private string? IndexPrefix;
    
    public OpenSearchWriterBuilder<T> WithOpenSearchConfigurationPrefix(string prefix, IConfiguration configuration)
    {
        Configuration = configuration;
        OpenSearchConfigurationPrefix = prefix;
        return this;
    }
    
    public OpenSearchWriterBuilder<T> WithIndex(string indexName)
    {
        IndexName = indexName;
        return this;
    }
    
    public OpenSearchWriter<T> Build()
    {
        var writer = new OpenSearchWriter<T>()
        {
            IndexName = IndexName
        };

        if (OpenSearchConfigurationPrefix != null && Configuration != null)
        {
            if (!string.IsNullOrEmpty(OpenSearchConfigurationPrefix))
            {
                writer.Server = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_URL);
                writer.ClientId = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_CLIENTID);
                writer.ClientSecret = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_CLIENTSECRET);
                writer.AuthEndpoint = Configuration.GetSection(OpenSearchConfigurationPrefix).GetValue<string>(OPENSEARCH_AUTH_ENDPOINT);
            }
            else
            {
                writer.Server = Configuration.GetValue<string>(OPENSEARCH_URL);
                writer.ClientId = Configuration.GetValue<string>(OPENSEARCH_CLIENTID);
                writer.ClientSecret = Configuration.GetValue<string>(OPENSEARCH_CLIENTSECRET);
                writer.AuthEndpoint = Configuration.GetValue<string>(OPENSEARCH_AUTH_ENDPOINT);   
            }
        }
        else
        {
            throw new InvalidOperationException("OpenSearchConfigurationPrefix or Configuration is not set");
        }
        
        writer.Prepare();
        return writer;
    }
}
