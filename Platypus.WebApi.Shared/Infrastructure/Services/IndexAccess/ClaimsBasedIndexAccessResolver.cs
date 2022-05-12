using Microsoft.AspNetCore.Http;

namespace Platypus.WebApi.Shared.Infrastructure.Services.IndexAccess;

public class ClaimsBasedIndexAccessResolver : IIndexAccessResolver
{
    private const string CustomerClaimName = "skagerak:customer";

    private readonly IIndicesBuilder _indicesBuilder;
    private readonly IHttpContextAccessor _contextAccessor;

    public ClaimsBasedIndexAccessResolver(IIndicesBuilder indicesBuilder, IHttpContextAccessor contextAccessor)
    {
        _indicesBuilder = indicesBuilder;
        _contextAccessor = contextAccessor;
    }

    public bool IsAccessible(IndexDescriptor indexName)
    {
        var accessibleCustomers = GetAccessibleCustomers();
        return accessibleCustomers.Contains(indexName.Customer);
    }

    public List<IndexDescriptor> GetAccessibleIndices(IndexTypeBase type)
    {
        var customerIds = GetAccessibleCustomers();
        return _indicesBuilder.Build(type, customerIds).ToList();
    }

    public List<IndexDescriptor> GetAccessibleIndices(DateTimeOffset @from, DateTimeOffset to, IndexTypeBase type)
    {
        var customerIds = GetAccessibleCustomers();
        return _indicesBuilder.Build(type, from, to, customerIds).ToList();
    }

    private CustomerIdentifier[] GetAccessibleCustomers() =>
        _contextAccessor.HttpContext?.User.Claims
            .Where(c => c.Type.Equals(CustomerClaimName))
            .Select(c => new CustomerIdentifier(c.Value))
            .ToArray() ?? Array.Empty<CustomerIdentifier>();
}