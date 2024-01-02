using System.Collections.Generic;
using System.Threading.Tasks;
using Cheetah.ComponentTest.Extensions;
using Cheetah.ComponentTest.Test.Model.OpenSearch;
using Cheetah.OpenSearch;
using Cheetah.OpenSearch.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OpenSearch.Client;
using Xunit;

namespace Cheetah.ComponentTest.Test
{

    [Trait("TestType", "IntegrationTests")]
    [Collection("IntegrationTests")]
    public class OpenSearchClientTests
    {
        readonly IOpenSearchClient _client;
        public OpenSearchClientTests()
        {
            var conf = new Dictionary<string, string?>
            {
                { "OPENSEARCH:URL", "http://localhost:9200"},
                { "OPENSEARCH:AUTHMODE", "OAuth2" },
                { "OPENSEARCH:OAUTH2:CLIENTID", "ClientId"},
                { "OPENSEARCH:OAUTH2:CLIENTSECRET", "1234"},
                { "OPENSEARCH:OAUTH2:TOKENENDPOINT", "http://localhost:1752/oauth2/token"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(conf)
                .AddEnvironmentVariables()
                .Build();

            var config = new OpenSearchConfig();
            configuration.Bind(OpenSearchConfig.Position, config);
           
            _client = OpenSearchClientFactory.CreateTestClientFromConfiguration(config);
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

            // Make sure the index is empty - Okay if this fails, since the index might not be there.
            await _client.DeleteIndexAsync(indexName, allowFailure: true);

            // Insert some data and verify its count
            await _client.InsertAsync(indexName, documents);
            await _client.RefreshIndexAsync(indexName);

            // Verify the correct count
            (await _client.CountIndexedDocumentsAsync(indexName))
                .Should().Be(documents.Count);

            // Verify that all our documents were inserted
            var actualDocuments = await _client.GetFromIndexAsync<OpenSearchTestModel>(indexName);
            actualDocuments.Should().BeEquivalentTo(documents, options => options.WithoutStrictOrdering());

            // Verify that we can delete it all again and that nothing is left
            await _client.DeleteIndexAsync(indexName);
        }
    }
}
