using Cheetah.WebApi.Shared.Core.NamingStrategies;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies;

public class ReturnIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
{
    public IEnumerable<IndexDescriptor> Build(DateTimeOffset @from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer)
    {
        var basename = IndexUtils.GetBaseName(prefix, type);

        yield return new IndexDescriptor(customer, $"{basename}");
    }
}