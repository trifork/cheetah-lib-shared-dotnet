using System.Collections.Generic;
using System.Threading.Tasks;
using OpenSearch.Client;

namespace Cheetah.OpenSearch
{
    public interface ICheetahOpenSearchClient
    {
        public IOpenSearchClient InternalClient { get; }

        /// <summary>
        /// Queries the OpenSearch instance for all indexes
        /// </summary>
        /// <returns>A List containing all index-names</returns>
        Task<List<string>> GetIndices();
    }
}
