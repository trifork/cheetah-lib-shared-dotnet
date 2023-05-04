using System;
using System.Collections.Generic;
using Cheetah.WebApi.Shared.Core.NamingStrategies;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies
{
    /// <summary>
    /// Use to build simple indices
    /// </summary>
    public class SimpleIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
    {
        /// <summary>
        /// Build simple indices in a specified timeframe.  
        /// </summary>
        /// <param name="from"> Start date of the timeframe </param>
        /// <param name="to"> End date of the timeframe </param>
        /// <param name="prefix"> Index prefix </param>
        /// <param name="type"> Index type </param>
        /// <param name="customer"> Costumer identifier </param>
        /// <returns> IndexDescriptor for simple indices </returns>
        public IEnumerable<IndexDescriptor> Build(DateTimeOffset @from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer)
        {
            var basename = IndexUtils.GetBaseName(prefix, type);

            yield return new IndexDescriptor(customer, $"{basename}");
        }
    }
}
