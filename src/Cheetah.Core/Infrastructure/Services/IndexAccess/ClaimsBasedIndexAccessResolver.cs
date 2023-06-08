using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.Core.Infrastructure.Services.IndexAccess
{
    /// <summary>
    /// A resolver used to return accessible indices
    /// </summary>
    public class ClaimsBasedIndexAccessResolver : IIndexAccessResolver
    {
        private const string CustomerClaimName = "cheetah:customer";

        private readonly IIndicesBuilder _indicesBuilder;
        private readonly IHttpContextAccessor _contextAccessor;

        public ClaimsBasedIndexAccessResolver(
            IIndicesBuilder indicesBuilder,
            IHttpContextAccessor contextAccessor
        )
        {
            _indicesBuilder = indicesBuilder;
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Check if an specific index is accessible
        /// </summary>
        /// <param name="indexName"> IndexDescriptor </param>
        /// <returns> A boolean describing if the index is accessible</returns>
        public bool IsAccessible(IndexDescriptor indexName)
        {
            var accessibleCustomers = GetAccessibleCustomers();
            return accessibleCustomers.Contains(indexName.Customer);
        }

        /// <summary>
        /// Get accessible indices of a specified index type
        /// </summary>
        /// <param name="type"> Index type </param>
        /// <returns> A list of all accessible indices of a specified index type </returns>
        public List<IndexDescriptor> GetAccessibleIndices(IndexTypeBase type)
        {
            var customerIds = GetAccessibleCustomers();
            return _indicesBuilder.Build(type, customerIds).ToList();
        }

        /// <summary>
        /// Get accessible indices of a specified index type in a specified timeframe
        /// </summary>
        /// <param name="from"> Start date of the timeframe </param>
        /// <param name="to"> End date of the timeframe </param>
        /// <param name="type"> Index type </param>
        /// <returns> A list of all accessible indices of a specified index type and timeframe</returns>
        public List<IndexDescriptor> GetAccessibleIndices(
            DateTimeOffset @from,
            DateTimeOffset to,
            IndexTypeBase type
        )
        {
            var customerIds = GetAccessibleCustomers();
            return _indicesBuilder.Build(type, from, to, customerIds).ToList();
        }

        private CustomerIdentifier[] GetAccessibleCustomers()
        {
            return _contextAccessor.HttpContext?.User.Claims
                    .Where(c => c.Type.Equals(CustomerClaimName))
                    .Select(c => new CustomerIdentifier(c.Value))
                    .ToArray() ?? Array.Empty<CustomerIdentifier>();
        }
    }
}
