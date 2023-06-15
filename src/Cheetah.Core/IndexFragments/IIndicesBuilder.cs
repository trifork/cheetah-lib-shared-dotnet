using Cheetah.Core.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.Core.IndexFragments
{
    public interface IIndicesBuilder
    {
        IEnumerable<IndexDescriptor> Build(
            IndexTypeBase type,
            params CustomerIdentifier[] customerIdentifiers
        );
        IEnumerable<IndexDescriptor> Build(
            IndexTypeBase type,
            DateTimeOffset from,
            DateTimeOffset to,
            params CustomerIdentifier[] identifiers
        );
        IndexPrefix Prefix { get; }
    }
}
