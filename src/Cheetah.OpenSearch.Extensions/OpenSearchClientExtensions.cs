using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Cheetah.ComponentTest.Extensions
{
    // TODO: Should this be removed entirely or moved to Cheetah.OpenSearch?
    /// <summary>
    /// Extension methods for <see cref="IOpenSearchClient"/> to simplify common operations.
    /// </summary>
    public static class OpenSearchClientExtensions
    {
        /// <summary>
        /// Asynchronously deletes an index
        /// </summary>
        /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
        /// <param name="indexName">The index to delete</param>
        /// <param name="allowFailure">Whether or not deletion of the index is allowed to fail. Useful if you want to clean up before running a test.</param>
        /// <returns>A <see cref="DeleteIndexResponse"/> which can be used to verify the result of the delete operation</returns>
        public static async Task<DeleteIndexResponse> DeleteIndexAsync(this IOpenSearchClient client, string indexName,
            bool allowFailure = false)
        {
            var response = await client.Indices.DeleteAsync(indexName);
            return allowFailure
                ? response
                : response.ThrowIfNotValid();
        }

        /// <summary>
        /// Asynchronously counts the amount of documents in a given index.
        /// </summary>
        /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
        /// <param name="indexName">The index to count documents in</param>
        /// <returns>The amount of documents present in the index</returns>
        public static async Task<long> CountIndexedDocumentsAsync(this IOpenSearchClient client, string indexName)
        {
            return (await client.CountAsync<object>(q => q.Index(indexName))).ThrowIfNotValid().Count;
        }

        /// <summary>
        /// Asynchronously bulk inserts documents into an index.
        /// </summary>
        /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
        /// <param name="indexName">The index to insert documents into</param>
        /// <param name="documents">The documents to insert</param>
        /// <typeparam name="T">The type of document to insert</typeparam>
        /// <returns>A <see cref="BulkResponse"/> which can be used to verify the result of the insert operation</returns>
        /// <exception cref="ArgumentException">Thrown when attempting to insert 0 documents into the index</exception>
        public static async Task<BulkResponse> InsertAsync<T>(this IOpenSearchClient client, string indexName, ICollection<T> documents) where T : class
        {

            return documents.Count != 0
                        ? await client.BulkAsync(b => b.Index(indexName).CreateMany(documents)).ThrowIfNotValid()
                        : throw new ArgumentException($"Attempted to insert 0 documents into index {indexName}");
        }

        /// <summary>
        /// Asynchronously refreshes an index.
        /// </summary>
        /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
        /// <param name="indexName">The index to refresh. If empty or null, will refresh all indexes.</param>
        /// <returns>A <see cref="RefreshResponse"/> which can be used to verify the result of the refresh operation</returns>
        public static async Task<RefreshResponse> RefreshIndexAsync(this IOpenSearchClient client, string indexName)
        {
            return await client.Indices.RefreshAsync(indexName).ThrowIfNotValid();
        }

        /// <summary>
        /// Asynchronously retrieves up to <paramref name="maxCount"/> documents from an index
        /// </summary>
        /// <param name="client">The <see cref="OpenSearchClient"/> used to access OpenSearch</param>
        /// <param name="indexName">The index to retrieve documents from</param>
        /// <param name="maxCount">The maximum number of documents to retrieve</param>
        /// <typeparam name="T">The type of document to retrieve</typeparam>
        /// <returns>The retrieved collection of documents</returns>
        public static async Task<IEnumerable<T>> GetFromIndexAsync<T>(this IOpenSearchClient client, string indexName, int maxCount = 100) where T : class
        {
            return (await client.SearchAsync<T>(q => q.Index(indexName).Size(maxCount))).ThrowIfNotValid().Hits.Select(x => x.Source);
        }

        /// <summary>
        /// Utility method to simplify validation of responses from OpenSearch.
        /// </summary>
        /// <param name="response">The response to validate</param>
        /// <typeparam name="T">The type of response. Must implement <see cref="IResponse"/></typeparam>
        /// <returns>The validated response</returns>
        /// <exception cref="OpenSearchClientException">Thrown when the response is not valid.</exception>
        public static T ThrowIfNotValid<T>(this T response) where T : IResponse
        {
            if (!response.IsValid)
            {
                throw new OpenSearchClientException($"Response did not indicate success. Debug info: {response.DebugInformation}");
            }
            return response;
        }

        /// <summary>
        /// Utility method to simplify validation of responses from OpenSearch.
        /// </summary>
        /// <param name="response">A task that returns a response to validate</param>
        /// <typeparam name="T">The type of response. Must implement <see cref="IResponse"/></typeparam>
        /// <returns>A task that returns the validated response</returns>
        /// <exception cref="OpenSearchClientException">Thrown when the response is not valid.</exception>
        public static async Task<T> ThrowIfNotValid<T>(this Task<T> response) where T : IResponse
        {
            return (await response).ThrowIfNotValid();
        }
    }
}
