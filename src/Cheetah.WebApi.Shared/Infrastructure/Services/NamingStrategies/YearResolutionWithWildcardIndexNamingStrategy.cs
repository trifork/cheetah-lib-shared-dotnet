using System;
using System.Collections.Generic;
using Cheetah.Core.Infrastructure.Services.IndexAccess;
using Cheetah.Core.NamingStrategies;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies
{
    /// <summary>
    /// Use to build year resolution with wildcard indices
    /// </summary>
    public class YearResolutionWithWildcardIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
    {
        /// <summary>
        /// Build year resolution with wildcard indices in a specified timeframe.
        /// </summary>
        /// <param name="from"> Start date of the timeframe </param>
        /// <param name="to"> End date of the timeframe </param>
        /// <param name="prefix"> Index prefix </param>
        /// <param name="type"> Index type</param>
        /// <param name="customer"> Costumer identifier </param>
        /// <returns> IndexDescriptor for year resolution with wildcard indices </returns>
        public IEnumerable<IndexDescriptor> Build(
            DateTimeOffset @from,
            DateTimeOffset to,
            IndexPrefix prefix,
            IndexTypeBase type,
            CustomerIdentifier customer
        )
        {
            var first = from.Year;
            var last = to.Year;

            var basename = IndexUtils.GetBaseName(prefix, type);

            for (var current = first; current <= last; current++)
            {
                yield return new IndexDescriptor(customer, $"{basename}_{customer}_{current}*");
            }
        }
    }
}
