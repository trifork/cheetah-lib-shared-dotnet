using OpenSearch.Client;

namespace Cheetah.Core.Interfaces
{
    public interface ICheetahOpenSearchClient
    {
        public OpenSearchClient InternalClient { get; }

        /// <summary>
        /// Queries the OpenSearch instance for all indexes
        /// </summary>
        /// <returns>A List containing all index-names</returns>
        Task<List<string>> GetIndices();
    }
}
