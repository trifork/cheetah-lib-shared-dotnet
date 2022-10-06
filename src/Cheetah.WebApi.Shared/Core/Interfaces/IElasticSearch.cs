using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;

namespace Cheetah.template.WebApi.Core.Interfaces
{
    public interface IElasticSearch
    {
        /// <summary>
        /// queries the ElasticSearch instance defined in appsettings.json, for all indecies-names using Nest.
        /// </summary>
        /// <returns>A List containing all index-names</returns>
        Task<List<string>> GetIndicies(List<IndexDescriptor> indices);
    }
}
