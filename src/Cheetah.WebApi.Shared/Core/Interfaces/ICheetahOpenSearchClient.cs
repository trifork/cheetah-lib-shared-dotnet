using System.Collections.Generic;
using System.Threading.Tasks;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using OpenSearch.Client;

namespace Cheetah.WebApi.Shared.Core.Interfaces
{
    public interface ICheetahOpenSearchClient
    {
        public OpenSearchClient InternalClient { get; }

        /// <summary>
        /// Queries the OpenSearch instance for all indexes
        /// </summary>
        /// <returns>A List containing all index-names</returns>
        Task<List<string>> GetIndices(List<IndexDescriptor> indices);
    }
}
