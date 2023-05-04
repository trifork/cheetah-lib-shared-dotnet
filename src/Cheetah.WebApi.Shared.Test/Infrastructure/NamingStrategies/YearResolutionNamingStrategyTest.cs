using System;
using System.Linq;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;
using Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies;
using Cheetah.WebApi.Shared.Test.Models;
using Xunit;

namespace Cheetah.WebApi.Shared.Test.Infrastructure.NamingStrategies
{
    public class YearResolutionNamingStrategyTest
    {

        [Fact]
        public void Build_YearResolutionIndexNamingStrategy()
        {
            //Arrange
            var harddate = new DateTime(2020, 1, 1);
            var namingStrategy = new YearResolutionIndexNamingStrategy();
            var from = new DateTimeOffset(harddate.AddYears(-2));
            var to = new DateTimeOffset(harddate);
            var prefix = new IndexPrefix("Prefix");
            var indexBase = IndexType.TestIndex("Indexbase");
            var customer = new CustomerIdentifier("Customer");


            //Act
            var indexList = namingStrategy.Build(from, to, prefix, indexBase, customer).ToList();
            //Assert
            Assert.True(indexList.Count == 3);
            Assert.True(indexList.Exists(x => x.Pattern.Equals($"{indexBase}_{prefix}_{customer}_2018".ToLower())));
            Assert.True(indexList.Exists(x => x.Pattern.Equals($"{indexBase}_{prefix}_{customer}_2019".ToLower())));
            Assert.True(indexList.Exists(x => x.Pattern.Equals($"{indexBase}_{prefix}_{customer}_2020".ToLower())));
        }

        [Fact]
        public void ShouldNotGenerateForNegativeInterval()
        {
            var strategy = new YearResolutionIndexNamingStrategy();
            var customerId = "customerId";
            var now = DateTimeOffset.Now;
            var indexType = IndexType.TestIndex("Indexbase");


            var indices = strategy.Build(now, now.AddYears(-1), new IndexPrefix("__"), indexType, new CustomerIdentifier(customerId)).ToList();

            Assert.Empty(indices);
        }

        [Fact]
        public void ShouldGenerateForSingleYear()
        {
            var strategy = new YearResolutionIndexNamingStrategy();
            var customerId = "customerId";
            var now = DateTimeOffset.Now;

            var indexType = IndexType.TestIndex("Indexbase");
            var indexPrefix = new IndexPrefix("abc");

            var indices = strategy.Build(now, now, indexPrefix, indexType, new CustomerIdentifier(customerId)).ToList();

            Assert.Single(indices);
            Assert.Equal($"{indexType}_{indexPrefix}_{customerId.ToLowerInvariant()}_{now.Year}", indices.Single().Pattern);
        }

        [Fact]
        public void ShouldGenerateForMultipleYears()
        {
            var strategy = new YearResolutionIndexNamingStrategy();
            var customerId = "customerId";
            var now = DateTimeOffset.Now;
            var indexType = IndexType.TestIndex("Indexbase");


            var indices = strategy.Build(now, now.AddYears(1), new IndexPrefix("__"), indexType, new CustomerIdentifier(customerId)).ToList();

            Assert.Equal(2, indices.Count);
        }

        [Fact]
        public void ShouldRenderIndexWithoutPrefix()
        {
            var strategy = new YearResolutionIndexNamingStrategy();
            var customerId = "customerId";
            var now = DateTimeOffset.Now;

            var indexType = IndexType.TestIndex("Indexbase");

            var indices = strategy.Build(now, now, IndexPrefix.Empty, indexType, new CustomerIdentifier(customerId)).ToList();

            Assert.Single(indices);
            Assert.Equal($"{indexType}_{customerId.ToLowerInvariant()}_{now.Year}", indices.Single().Pattern);
        }
    }
}
