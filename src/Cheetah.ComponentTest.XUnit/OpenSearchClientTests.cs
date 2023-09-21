using Cheetah.ComponentTest.OpenSearch;
using Cheetah.ComponentTest.XUnit.Model.OpenSearch;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.XUnit
{

    [Trait("TestType", "IntegrationTests")]
    [Collection("IntegrationTests")]
    public class OpenSearchClientTests
    {
        readonly IConfiguration _configuration;
        public OpenSearchClientTests()
        {
            var conf = new Dictionary<string, string>
            {
                { "OPENSEARCH:URL", "http://localhost:9200"},
                { "OPENSEARCH:CLIENTID", "ClientId"},
                { "OPENSEARCH:CLIENTSECRET", "1234"},
                { "OPENSEARCH:AUTHENDPOINT", "http://localhost:1752/oauth2/token"}
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
                new("Document 1", 2), new("Document 2", 3), new("Document 3", 4), new("Document 4", 5),
            };

            var openSearchClient = OpenSearchClientFactory.Create(_configuration);

            // Make sure the index is empty - Okay if this fails, since the index might not be there.
            await openSearchClient.DeleteIndexAsync(indexName, allowFailure: true);

            // Insert some data and verify its count
            await openSearchClient.InsertAsync(indexName, documents);
            await openSearchClient.RefreshIndexAsync(indexName);

            // Verify the correct count
            (await openSearchClient.CountIndexedDocumentsAsync(indexName))
                .Should().Be(documents.Count);

            // Verify that all our documents were inserted
            var actualDocuments = await openSearchClient.GetFromIndexAsync<OpenSearchTestModel>(indexName);
            actualDocuments.Should().BeEquivalentTo(documents, options => options.WithoutStrictOrdering());

            // Verify that we can delete it all again and that nothing is left
            await openSearchClient.DeleteIndexAsync(indexName);
        }
    }
}
