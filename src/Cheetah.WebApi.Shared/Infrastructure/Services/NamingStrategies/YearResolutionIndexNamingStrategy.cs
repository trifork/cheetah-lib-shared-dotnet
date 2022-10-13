using Cheetah.WebApi.Shared.Core.NamingStrategies;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies;

public class YearResolutionIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
{
    public IEnumerable<IndexDescriptor> Build(DateTimeOffset @from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer)
    {
        var first = from.Year;
        var last = to.Year;
        //bug: fails if index for year does not exist!

        var basename = IndexUtils.GetBaseName(prefix, type);

        for (var current = first; current <= last; current++)
        {
            yield return new IndexDescriptor(customer, $"{basename}_{customer}_{current}");
        }
    }
}
