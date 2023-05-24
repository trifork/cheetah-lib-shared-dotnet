using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;

namespace Cheetah.WebApi.Shared.Test.Models
{
    public class IndexType : IndexTypeBase
    {
        public static IndexType TestIndex(string indexBase = "indexBase")
        {
            return new IndexType(indexBase);
        }

        public IndexType(string name)
            : base(name) { }
    }
}
