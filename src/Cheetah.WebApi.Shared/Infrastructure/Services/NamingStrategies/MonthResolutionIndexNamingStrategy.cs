using Cheetah.WebApi.Shared.Core.NamingStrategies;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies;

public class MonthResolutionIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
{
    public IEnumerable<IndexDescriptor> Build(DateTimeOffset from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer)
    {
        var firstMonth = new DateTime(from.Year, @from.Month, 1);
        var lastMonth = new DateTime(to.Year, to.Month, 1);

        var basename = IndexUtils.GetBaseName(prefix, type);

        for (var current = firstMonth; current <= lastMonth; current = current.AddMonths(1))
        {
            yield return new IndexDescriptor(customer, $"{basename}_{customer}_{current.Year}_{current.Month:00}");
        }
    }
}