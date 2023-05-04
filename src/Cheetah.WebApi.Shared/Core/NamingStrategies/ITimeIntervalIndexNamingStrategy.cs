using System;
using System.Collections.Generic;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.WebApi.Shared.Core.NamingStrategies
{
    public interface ITimeIntervalIndexNamingStrategy
    {
        public IEnumerable<IndexDescriptor> Build(DateTimeOffset from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer);
    }
}
