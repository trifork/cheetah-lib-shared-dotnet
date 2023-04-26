using System;
using System.Collections.Generic;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;

public interface IIndexAccessResolver
{
    bool IsAccessible(IndexDescriptor indexDescriptor);
    List<IndexDescriptor> GetAccessibleIndices(IndexTypeBase type);
    List<IndexDescriptor> GetAccessibleIndices(DateTimeOffset from, DateTimeOffset to, IndexTypeBase type);
}

public struct IndexDescriptor
{
    public IndexDescriptor(CustomerIdentifier customer, string pattern)
    {
        Customer = customer;
        Pattern = pattern;
    }

    public string Pattern { get; set; }
    public CustomerIdentifier Customer { get; set; }

    public override string ToString() => Pattern;
}