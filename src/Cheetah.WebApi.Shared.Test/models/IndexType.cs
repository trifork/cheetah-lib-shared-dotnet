using Cheetah.WebApi.Shared.Infrastructure.Services;

namespace Cheetah.WebApi.Shared_test.models
{
    public class IndexType : IndexTypeBase
    {

        public static IndexType testIndex(string indexBase = "indexBase")
        {
            return new IndexType(indexBase);
        }

        public IndexType(string name) : base(name)
        {
        }
    }
}
