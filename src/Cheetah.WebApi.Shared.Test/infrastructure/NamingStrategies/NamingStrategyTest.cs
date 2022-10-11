using Cheetah.WebApi.Shared.Infrastructure.Services;
using Cheetah.WebApi.Shared_test.models;
using System;
using System.Linq;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;
using Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies;
using Xunit;

namespace Cheetah.WebApi.Shared_test.infrastructure.NamingStrategies
{
    public class NamingStrategyTest
    {
        [Fact]
        public void SimpleIndexNamingStrategy()
        {
            //Arrange
            var harddate = new DateTime(2020, 01, 01);
            var namingStrategy = new SimpleIndexNamingStrategy();
            var from = new DateTimeOffset(harddate.AddYears(-2));
            var to = new DateTimeOffset(harddate);
            var prefix = new IndexPrefix("Prefix");
            var indexBase = IndexType.testIndex("Indexbase");
            var customer = new CustomerIdentifier("Customer");


            //Act
            var index = namingStrategy.Build(from, to, prefix, indexBase, customer);

            //Assert
            Assert.Equal("indexbase_prefix", index.First().Pattern);

        }

        [Fact]
        public void Build_CustomerIndexNamingStrategy()
        {
            //Arrange
            var harddate = new DateTime(2020, 01, 01);
            var namingStrategy = new CustomerIndexNamingStrategy();
            var from = new DateTimeOffset(harddate.AddYears(-2));
            var to = new DateTimeOffset(harddate);
            var prefix = new IndexPrefix("Prefix");
            var indexBase = IndexType.testIndex("Indexbase");
            var customer = new CustomerIdentifier("Customer");


            //Act
            var index = namingStrategy.Build(from, to, prefix, indexBase, customer);

            //Assert
            Assert.Equal("indexbase_prefix_customer_*", index.First().Pattern);

        }



        [Fact]
        public void Build_YearResolutionWithWildcardIndexNamingStrategy()
        {
            //Arrange
            var harddate = new DateTime(2020, 1, 1);
            var namingStrategy = new YearResolutionWithWildcardIndexNamingStrategy();
            var from = new DateTimeOffset(harddate.AddYears(-2));
            var to = new DateTimeOffset(harddate);
            var prefix = new IndexPrefix("Prefix");
            var indexBase = IndexType.testIndex("Indexbase");
            var customer = new CustomerIdentifier("Customer");


            //Act
            var indexList = namingStrategy.Build(from, to, prefix, indexBase, customer).ToList();
            //Assert
            Assert.True(indexList.Count == 3);
            Assert.True(indexList.Exists(x => x.Pattern.Equals($"{indexBase}_{prefix}_{customer}_2018*".ToLower())));
            Assert.True(indexList.Exists(x => x.Pattern.Equals($"{indexBase}_{prefix}_{customer}_2019*".ToLower())));
            Assert.True(indexList.Exists(x => x.Pattern.Equals($"{indexBase}_{prefix}_{customer}_2020*".ToLower())));
        }

        [Fact]
        public void Build_MonthResolutionIndexNamingStrategy()
        {
            //Arrange
            var harddate = new DateTime(2020, 3, 1);
            var namingStrategy = new MonthResolutionIndexNamingStrategy();
            var from = new DateTimeOffset(harddate.AddMonths(-2));
            var to = new DateTimeOffset(harddate);
            var prefix = new IndexPrefix("Prefix");
            var indexBase = IndexType.testIndex("Indexbase");
            var customer = new CustomerIdentifier("Customer");


            //Act
            var indexList = namingStrategy.Build(from, to, prefix, indexBase, customer).ToList();

            //Assert
            Assert.True(indexList.Count == 3);
            Assert.True(indexList.Exists(x => x.Pattern.Equals($"{indexBase}_{prefix}_{customer}_2020_01".ToLower())));
            Assert.True(indexList.Exists(x => x.Pattern.Equals($"{indexBase}_{prefix}_{customer}_2020_02".ToLower())));
            Assert.True(indexList.Exists(x => x.Pattern.Equals($"{indexBase}_{prefix}_{customer}_2020_03".ToLower())));
        }
    }
}
