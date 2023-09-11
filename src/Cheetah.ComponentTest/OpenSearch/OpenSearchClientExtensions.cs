using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenSearch.Client;

namespace Cheetah.ComponentTest.OpenSearch;

public static class OpenSearchClientExtensions
{
    /// <summary>
    /// Asynchronously deletes an index
    /// </summary>
    /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
    /// <param name="indexName">The index to delete</param>
    /// <returns>A <see cref="DeleteIndexResponse"/> which can be used to verify the result of the delete operation</returns>
    public static Task<DeleteIndexResponse> DeleteIndexAsync(this OpenSearchClient client, string indexName)
        => client.Indices.DeleteAsync(indexName);

    /// <summary>
    /// Asynchronously counts the amount of documents in a given index.
    /// </summary>
    /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
    /// <param name="indexName">The index to count documents in</param>
    /// <returns>The amount of documents present in the index</returns>
    public static async Task<long> CountIndexedDocumentsAsync(this OpenSearchClient client, string indexName) 
        => (await client.CountAsync<object>(q => q.Index(indexName))).Count;

    /// <summary>
    /// Asynchronously bulk inserts documents into an index.
    /// </summary>
    /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
    /// <param name="indexName">The index to insert documents into</param>
    /// <param name="documents">The documents to insert</param>
    /// <typeparam name="T">The type of document to insert</typeparam>
    /// <returns>A <see cref="BulkResponse"/> which can be used to verify the result of the insert operation</returns>
    /// <exception cref="ArgumentException">Thrown when attempting to insert 0 documents into the index</exception>
    public static Task<BulkResponse> InsertAsync<T>(this OpenSearchClient client, string indexName, params T[] documents) where T : class 
        => documents.Any() 
            ? client.BulkAsync(b => b.Index(indexName).CreateMany(documents)) 
            : throw new ArgumentException($"Attempted to insert 0 documents into index {indexName}");

    /// <summary>
    /// Asynchronously refreshes an index.
    /// </summary>
    /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
    /// <param name="indexName">The index to refresh. If empty or null, will refresh all indexes.</param>
    /// <returns>A <see cref="RefreshResponse"/> which can be used to verify the result of the refresh operation</returns>
    public static Task<RefreshResponse> RefreshIndexAsync(this OpenSearchClient client, string indexName)
        => client.Indices.RefreshAsync(indexName);

    /// <summary>
    /// Asynchronously retrieves up to <param name="maxCount"> documents from an index</param>
    /// </summary>
    /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
    /// <param name="indexName">The index to retrieve documents from</param>
    /// <param name="maxCount">The maximum number of documents to retrieve</param>
    /// <typeparam name="T">The type of document to retrieve</typeparam>
    /// <returns>The retrieved collection of documents</returns>
    public static async Task<IEnumerable<T>> GetFromIndexAsync<T>(this OpenSearchClient client, string indexName, int maxCount = 100) where T : class
        => (await client.SearchAsync<T>(q => q.Index(indexName).Size(maxCount))).Hits.Select(x => x.Source);
}
