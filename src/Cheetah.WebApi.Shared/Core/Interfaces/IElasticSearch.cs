using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;

namespace Cheetah.Shared.WebApi.Core.Interfaces
{
    public interface IElasticSearch
    {
        /// <summary>
        /// Queries the ElasticSearch instance for all indexes
        /// </summary>
        /// <returns>A List containing all index-names</returns>
        Task<List<string>> GetIndices(List<IndexDescriptor> indices);
    }
}
