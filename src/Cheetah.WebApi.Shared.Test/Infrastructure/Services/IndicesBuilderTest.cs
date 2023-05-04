using System;
using System.Linq;
using System.Text.RegularExpressions;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;
using Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies;
using Cheetah.WebApi.Shared.Test.Models;
using Xunit;

namespace Cheetah.WebApi.Shared.Test.Infrastructure.Services
{
    public class IndicesBuilderTest
    {
        [Fact]
        public void ShouldExpandSingleMonth()
        {
            var strategy = new MonthResolutionIndexNamingStrategy();
            var builder = new IndicesBuilder(new IndexPrefix("pf"), strategy);
            var indextype = IndexType.TestIndex("Indexbase");

            var enumerable = builder
                .Build(
                    indextype,
                    DateTimeOffset.Now,
                    DateTimeOffset.Now,
                    new CustomerIdentifier("cust1")
                )
                .ToList();
            Assert.Single(enumerable);
        }

        [Fact]
        public void ShouldReturnZeroIndicesForNegativeInterval()
        {
            var indexNamingStrategy = new MonthResolutionIndexNamingStrategy();
            var builder = new IndicesBuilder(new IndexPrefix("pf"), indexNamingStrategy);
            var indextype = IndexType.TestIndex("Indexbase");

            var enumerable = builder
                .Build(
                    indextype,
                    DateTimeOffset.Now,
                    DateTimeOffset.Now.AddMonths(-1),
                    new CustomerIdentifier("testId1")
                )
                .ToList();
            Assert.Empty(enumerable);
        }

        [Fact]
        public void ShouldExpandThreeMonths()
        {
            var customer = "testid";
            var indextype = IndexType.TestIndex("Indexbase");

            var from = DateTimeOffset.Now;
            var to = from.AddMonths(2);

            var indexNamingStrategy = new MonthResolutionIndexNamingStrategy();
            var builder = new IndicesBuilder(new IndexPrefix("pf"), indexNamingStrategy);

            var result = builder
                .Build(indextype, from, to, new CustomerIdentifier(customer))
                .ToList();
            Assert.Equal(3, result.Count);

            var validExpression = new Regex(customer + "_[0-9]{4}_[0-9]{2}");
            foreach (var index in result)
            {
                Assert.Matches(validExpression, index.Pattern);
            }
        }

        [Fact]
        public void ShouldExpand99Months()
        {
            var customer = "testid";
            var indextype = IndexType.TestIndex("Indexbase");

            var from = DateTimeOffset.Now;
            var to = from.AddMonths(98);

            var indexNamingStrategy = new MonthResolutionIndexNamingStrategy();
            var builder = new IndicesBuilder(new IndexPrefix("pf"), indexNamingStrategy);

            var result = builder
                .Build(indextype, from, to, new CustomerIdentifier(customer))
                .ToList();
            Assert.Equal(99, result.Count);

            var validExpression = new Regex(customer + "_[0-9]{4}_[0-9]{2}");
            foreach (var index in result)
            {
                Assert.Matches(validExpression, index.Pattern);
            }
        }

        [Fact]
        public void ShouldExpandMultipleIdentifiers()
        {
            var customer1 = "testid1";
            var customer2 = "testid2";
            var indextype = IndexType.TestIndex("Indexbase");

            var indexNamingStrategy = new MonthResolutionIndexNamingStrategy();
            var builder = new IndicesBuilder(new IndexPrefix("pf"), indexNamingStrategy);

            var from = DateTimeOffset.Now;
            var to = from.AddMonths(98);

            var result = builder
                .Build(
                    indextype,
                    from,
                    to,
                    new CustomerIdentifier(customer1),
                    new CustomerIdentifier(customer2)
                )
                .ToList();
            Assert.Equal(198, result.Count);

            var validExpression = new Regex(
                $"{builder.Prefix}_({customer1}|{customer2})" + "_[0-9]{4}_[0-9]{2}"
            );
            foreach (var index in result)
            {
                Assert.Matches(validExpression, index.Pattern);
            }
        }

        [Fact]
        public void ShouldExpandMultipleIdentifiers_exact_match()
        {
            var customer1 = "testid1";
            var customer2 = "testid2";
            var indextype = IndexType.TestIndex("Indexbase");

            var indexNamingStrategy = new MonthResolutionIndexNamingStrategy();
            var builder = new IndicesBuilder(new IndexPrefix("pf"), indexNamingStrategy);

            var from = DateTimeOffset.Now;
            var to = from.AddMonths(98);

            var result = builder
                .Build(
                    indextype,
                    from,
                    to,
                    new CustomerIdentifier(customer1),
                    new CustomerIdentifier(customer2)
                )
                .ToList();
            Assert.Equal(198, result.Count);

            var validExpressionCustomer1 = new Regex(
                $"{builder.Prefix}_({customer1})" + "_[0-9]{4}_[0-9]{2}"
            );

            var customer1IndexCount = result.Count(
                r => validExpressionCustomer1.IsMatch(r.Pattern)
            );
            Assert.Equal(99, customer1IndexCount);

            var validExpressionCustomer2 = new Regex(
                $"{builder.Prefix}_({customer2})" + "_[0-9]{4}_[0-9]{2}"
            );

            var customer2IndexCount = result.Count(
                r => validExpressionCustomer2.IsMatch(r.Pattern)
            );
            Assert.Equal(99, customer2IndexCount);
        }

        [Theory]
        [InlineData("prefix", "ok", "ok2")]
        public void ShouldReturnWildcardsWhenNoInterval(string prefix, params string[] ids)
        {
            var indextype = IndexType.TestIndex("Indexbase");
            var indexNamingStrategy = new MonthResolutionIndexNamingStrategy();
            var builder = new IndicesBuilder(new IndexPrefix(prefix), indexNamingStrategy);

            var result = builder
                .Build(indextype, ids.Select(id => new CustomerIdentifier(id)).ToArray())
                .ToList();

            Assert.Equal(2, result.Count);
            Assert.True(result.All(r => r.Pattern.EndsWith("_*")));
        }

        [Theory]
        [InlineData("prefix", "OK", "Ok2")]
        public void ShouldConvertToLowerCase(string prefix, params string[] ids)
        {
            var indextype = IndexType.TestIndex("Indexbase");
            var indexNamingStrategy = new MonthResolutionIndexNamingStrategy();
            var builder = new IndicesBuilder(new IndexPrefix(prefix), indexNamingStrategy);

            var result = builder
                .Build(indextype, ids.Select(id => new CustomerIdentifier(id)).ToArray())
                .ToList();

            Assert.Equal(2, result.Count);
            foreach (var id in ids)
            {
                Assert.Contains(
                    result,
                    r =>
                        r.Pattern.StartsWith(
                            $"{indextype}_{builder.Prefix}_{id.ToLowerInvariant()}_"
                        )
                );
            }
        }
    }
}
