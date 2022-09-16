using Cheetah.WebApi.Shared.Infrastructure.Services;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cheetah.WebApi.Shared_test.infrastructure.IndexFragments
{
    public class IndexFragmentTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("$$")]
        [InlineData("!!WAT!!")]
        [InlineData("ok..")]
        public void ShouldRejectCustomerIdentifiersWithIllegalChars(string badId)
        {
            Assert.Throws<IndexFragmentException>(() => new CustomerIdentifier(badId));
        }

        [Theory]
        [InlineData("")]
        [InlineData("!@#$%")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("   ")]
        public void ShouldRejectPrefixWithIllegalChars(string badPrefix)
        {
            Assert.Throws<IndexFragmentException>(() => new IndexPrefix(badPrefix));
        }

        [Fact]
        public void ShouldSupportUnderscoreInCustomerId()
        {
            var id = "CustomerKAM_1";
            var customerIdentifier = new CustomerIdentifier(id);
            Assert.Equal(id.ToLowerInvariant(), customerIdentifier.ToString());
        }

        [Fact]
        public void ShouldSupportDashInCustomerId()
        {
            var id = "CustomerKAM-1";
            var customerIdentifier = new CustomerIdentifier(id);
            Assert.Equal(id.ToLowerInvariant(), customerIdentifier.ToString());
        }

        [Fact]
        public void ShouldSupportUnderscoreInPrefix()
        {
            var rawPrefix = "db_prefiXX_1";
            var prefix = new IndexPrefix(rawPrefix);
            Assert.Equal(rawPrefix.ToLowerInvariant(), prefix.ToString());
        }
    }
}
