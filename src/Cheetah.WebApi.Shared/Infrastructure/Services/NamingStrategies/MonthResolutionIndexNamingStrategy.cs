using System;
using System.Collections.Generic;
using Cheetah.Core.Infrastructure.Services.IndexAccess;
using Cheetah.Core.NamingStrategies;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies
{
    /// <summary>
    /// Use to build month resolution indices
    /// </summary>
    public class MonthResolutionIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
    {
        /// <summary>
        /// Build month resolution indices in a specified timeframe.
        /// </summary>
        /// <param name="from"> Start date of the timeframe </param>
        /// <param name="to"> End date of the timeframe </param>
        /// <param name="prefix"> Index prefix </param>
        /// <param name="type"> Index type</param>
        /// <param name="customer"> Costumer identifier </param>
        /// <returns> IndexDescriptor for month resolution indices </returns>
        public IEnumerable<IndexDescriptor> Build(
            DateTimeOffset from,
            DateTimeOffset to,
            IndexPrefix prefix,
            IndexTypeBase type,
            CustomerIdentifier customer
        )
        {
            var firstMonth = new DateTime(from.Year, @from.Month, 1);
            var lastMonth = new DateTime(to.Year, to.Month, 1);

            var basename = IndexUtils.GetBaseName(prefix, type);

            for (var current = firstMonth; current <= lastMonth; current = current.AddMonths(1))
            {
                yield return new IndexDescriptor(
                    customer,
                    $"{basename}_{customer}_{current.Year}_{current.Month:00}"
                );
            }
        }
    }
}
