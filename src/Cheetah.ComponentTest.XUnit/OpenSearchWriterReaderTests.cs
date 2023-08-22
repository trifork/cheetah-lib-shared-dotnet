using Cheetah.ComponentTest.OpenSearch;
using Cheetah.ComponentTest.XUnit.Model;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.XUnit;

[Trait("TestType", "IntegrationTests")]
public class OpenSearchWriterReaderTests
{    
    readonly IConfiguration _configuration;

    public OpenSearchWriterReaderTests()
    {
        var conf = new Dictionary<string, string>
        {
            { "OPENSEARCH:URL", "http://opensearch:9200"},
            { "OPENSEARCH:CLIENTID", "opensearch"},
            { "OPENSEARCH:CLIENTSECRET", "1234"},
            { "OPENSEARCH:AUTHENDPOINT", "http://cheetahoauthsimulator:80/oauth2/token"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(conf)
            .AddEnvironmentVariables()
            .Build();
    }   
    [Fact]
    public async Task Should_WriteAndRead_When_UsingOpenSearch()
    {
        var indexName = "test-index";

        var documents = new List<OpenSearchTestModel>
        {
            new ("Document 1", 2),
            new ("Document 2", 3),
            new ("Document 3", 4),
            new ("Document 4", 5),
        };

        var opensearchClient = OpenSearchClientFactory.Create(_configuration)
            .Build();

        // the Index call takes around 365ms and the DeleteIndex calls take around 165ms so they seem to be running synchronously
        // which means we can use them without Thread.Sleep which is great
        opensearchClient.DeleteIndex(indexName);
        Assert.Equal(0, opensearchClient.Count(indexName));

        opensearchClient.Index(indexName, documents);
        opensearchClient.RefreshIndex(indexName);
        Assert.Equal(documents.Count, opensearchClient.Count(indexName));
        Assert.All(opensearchClient.Search<OpenSearchTestModel>(indexName), d => documents.Contains(d.Source));

        opensearchClient.DeleteIndex(indexName);
        opensearchClient.RefreshIndex(indexName);
        Assert.Equal(0, opensearchClient.Count(indexName));
    }
}
