using System;
using System.Linq;
using Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments;
using Cheetah.WebApi.Shared.Infrastructure.Services.NamingStrategies;
using Cheetah.WebApi.Shared.Test.Models;
using Xunit;

namespace Cheetah.WebApi.Shared.Test.Infrastructure.Services
{
  public class IndiciesBuilderTest
  {
    [Fact]
    public void Build_SimpleIndexNamingStrategy()
    {
      //Arrange
      var harddate = new DateTime(2020, 01, 01);
      var namingStrategy = new SimpleIndexNamingStrategy();
      var from = new DateTimeOffset(harddate.AddYears(-2));
      var to = new DateTimeOffset(harddate);
      var prefix = new IndexPrefix("Prefix");
      var indexBase = IndexType.TestIndex("Indexbase");
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
      var indexBase = IndexType.TestIndex("Indexbase");
      var customer = new CustomerIdentifier("Customer");


      //Act
      var index = namingStrategy.Build(from, to, prefix, indexBase, customer);

      //Assert
      Assert.Equal("indexbase_prefix_customer_*", index.First().Pattern);

    }

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
      Assert.True(indexList.Exists(x => x.Pattern.Equals("indexbase_prefix_customer_2018")));
      Assert.True(indexList.Exists(x => x.Pattern.Equals("indexbase_prefix_customer_2019")));
      Assert.True(indexList.Exists(x => x.Pattern.Equals("indexbase_prefix_customer_2020")));
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
      var indexBase = IndexType.TestIndex("Indexbase");
      var customer = new CustomerIdentifier("Customer");


      //Act
      var indexList = namingStrategy.Build(from, to, prefix, indexBase, customer).ToList();
      //Assert
      Assert.True(indexList.Count == 3);
      Assert.True(indexList.Exists(x => x.Pattern.Equals("indexbase_prefix_customer_2018*")));
      Assert.True(indexList.Exists(x => x.Pattern.Equals("indexbase_prefix_customer_2019*")));
      Assert.True(indexList.Exists(x => x.Pattern.Equals("indexbase_prefix_customer_2020*")));
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
      var indexBase = IndexType.TestIndex("Indexbase");
      var customer = new CustomerIdentifier("Customer");


      //Act
      var indexList = namingStrategy.Build(from, to, prefix, indexBase, customer).ToList();

      //Assert
      Assert.True(indexList.Count == 3);
      Assert.True(indexList.Exists(x => x.Pattern.Equals("indexbase_prefix_customer_2020_01")));
      Assert.True(indexList.Exists(x => x.Pattern.Equals("indexbase_prefix_customer_2020_02")));
      Assert.True(indexList.Exists(x => x.Pattern.Equals("indexbase_prefix_customer_2020_03")));
    }
  }
}