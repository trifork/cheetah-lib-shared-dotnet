using System;
using System.Collections.Generic;
using System.Linq;
using Cheetah.WebApi.Shared.Core.IndexFragments;
using Cheetah.WebApi.Shared.Core.NamingStrategies;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

public class IndicesBuilder : IIndicesBuilder
{
    private readonly ITimeIntervalIndexNamingStrategy _indexNamingStrategy;
    public IndexPrefix Prefix { get; }

    public IndicesBuilder(IndexPrefix prefix, ITimeIntervalIndexNamingStrategy indexNamingStrategy)
    {
        _indexNamingStrategy = indexNamingStrategy;
        Prefix = prefix;
    }

    public IEnumerable<IndexDescriptor> Build(IndexTypeBase type, params CustomerIdentifier[] customerIdentifiers)
    {
        var basename = IndexUtils.GetBaseName(Prefix, type);

        return customerIdentifiers
                .Select(customer => new IndexDescriptor(customer, $"{basename}_{customer}_*"))
                .ToList();
    }

    public IEnumerable<IndexDescriptor> Build(IndexTypeBase type, DateTimeOffset from, DateTimeOffset to, params CustomerIdentifier[] identifiers)
    {
        return identifiers
                .SelectMany(customer => _indexNamingStrategy.Build(from, to, Prefix, type, customer))
                .ToList();
    }
}