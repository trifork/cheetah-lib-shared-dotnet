namespace Cheetah.OpenSearch
{
    public interface ICheetahOpenSearchClient
    {
        public global::OpenSearch.Client.OpenSearchClient InternalClient { get; }

        /// <summary>
        /// Queries the OpenSearch instance for all indexes
        /// </summary>
        /// <returns>A List containing all index-names</returns>
        Task<List<string>> GetIndices();
    }
}
