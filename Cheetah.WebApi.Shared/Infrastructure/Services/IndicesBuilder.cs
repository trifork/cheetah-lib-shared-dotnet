using System.Text.RegularExpressions;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;

namespace Cheetah.WebApi.Shared.Infrastructure.Services;

public interface IIndicesBuilder
{
    IEnumerable<IndexDescriptor> Build(IndexTypeBase type, params CustomerIdentifier[] customerIdentifiers);
    IEnumerable<IndexDescriptor> Build(IndexTypeBase type, DateTimeOffset from, DateTimeOffset to, params CustomerIdentifier[] identifiers);
    IndexPrefix Prefix { get; }
}

public class IndicesBuilder : IIndicesBuilder
{
    private readonly ITimeIntervalIndexNamingStrategy _indexNamingStrategy;
    public IndexPrefix Prefix { get; }

    public IndicesBuilder(IndexPrefix prefix, ITimeIntervalIndexNamingStrategy indexNamingStrategy)
    {
        _indexNamingStrategy = indexNamingStrategy;
        Prefix = prefix;
    }

    public IEnumerable<IndexDescriptor> Build(IndexTypeBase type, params CustomerIdentifier[] customerIdentifiers)
    {
        var basename = IndexUtils.GetBaseName(Prefix, type);

        return customerIdentifiers
                .Select(customer => new IndexDescriptor(customer, $"{basename}_{customer}_*"))
                .ToList()
            ;
    }

    public IEnumerable<IndexDescriptor> Build(IndexTypeBase type, DateTimeOffset from, DateTimeOffset to, params CustomerIdentifier[] identifiers)
    {
        return identifiers
                .SelectMany(customer => _indexNamingStrategy.Build(from, to, Prefix, type, customer))
                .ToList()
            ;
    }
}

public interface ITimeIntervalIndexNamingStrategy
{
    public IEnumerable<IndexDescriptor> Build(DateTimeOffset from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer);
}

public class ReturnIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
{
    public IEnumerable<IndexDescriptor> Build(DateTimeOffset @from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer) 
    {
        var basename = IndexUtils.GetBaseName(prefix, type);

        yield return new IndexDescriptor(customer, $"{basename}");
    }
}

public class CustomerIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
{
    public IEnumerable<IndexDescriptor> Build(DateTimeOffset @from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer)
    {
        var basename = IndexUtils.GetBaseName(prefix, type);

        yield return new IndexDescriptor(customer, $"{basename}_{customer}_*");
    }
}
public class YearResolutionIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
{
    public IEnumerable<IndexDescriptor> Build(DateTimeOffset @from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer)
    {
        var first = from.Year;
        var last = to.Year;
        //bug: fails if index for year does not exist!

        var basename = IndexUtils.GetBaseName(prefix, type);

        for (var current = first; current <= last; current++)
        {
            yield return new IndexDescriptor(customer, $"{basename}_{customer}_{current}");
        }
    }
}
public class YearResolutionWithWildcardIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
{
    public IEnumerable<IndexDescriptor> Build(DateTimeOffset @from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer)
    {
        var first = from.Year;
        var last = to.Year;

        var basename = IndexUtils.GetBaseName(prefix, type);

        for (var current = first; current <= last; current++)
        {
            yield return new IndexDescriptor(customer, $"{basename}_{customer}_{current}*");
        }
    }
}

public class MonthResolutionIndexNamingStrategy : ITimeIntervalIndexNamingStrategy
{
    public IEnumerable<IndexDescriptor> Build(DateTimeOffset from, DateTimeOffset to, IndexPrefix prefix, IndexTypeBase type, CustomerIdentifier customer)
    {
        var firstMonth = new DateTime(from.Year, @from.Month, 1);
        var lastMonth = new DateTime(to.Year, to.Month, 1);

        var basename = IndexUtils.GetBaseName(prefix, type);

        for (var current = firstMonth; current <= lastMonth; current = current.AddMonths(1))
        {
            yield return new IndexDescriptor(customer, $"{basename}_{customer}_{current.Year}_{current.Month:00}");
        }
    }
}

public static class IndexUtils
{
    public static string GetBaseName(IndexPrefix prefix, IndexTypeBase type)
    {
        // Det endeligt format er <datatype>_[<optional_prefix>]_<customerid>_<yyyy> indtil vi laver det om 😉
        var pf = prefix.ToString();
        return string.IsNullOrWhiteSpace(pf) ? $"{type}" : $"{type}_{pf}";
    }
}


public class IndexFragmentException : ArgumentException
{
    public override string Message => "Value must be a non empty string consisting of letters and numbers";
}

public class IndexFragment
{
    private static readonly Regex LettersAndNumbersOnly = new("[^A-Za-z0-9_-]");
    protected string Value { get; }

    protected IndexFragment(string value)
    {

        if (LettersAndNumbersOnly.IsMatch(value))
            throw new IndexFragmentException();

        Value = value.ToLowerInvariant();
    }

    protected IndexFragment()
    {
        Value = String.Empty;
    }

    public override string ToString()
    {
        return Value;
    }
}

public class IndexPrefix : IndexFragment
{
    private IndexPrefix() : base()
    {
    }

    public IndexPrefix(string value) : base(value) { }
    public static IndexPrefix Empty = new IndexPrefix();
}

public class CustomerIdentifier : IndexFragment
{
    public CustomerIdentifier(string value) : base(value)
    {
    }

    public bool IsMatch(string thatValue) =>
        string.Compare(this.Value, thatValue, StringComparison.InvariantCultureIgnoreCase) == 0;
}

public abstract class IndexTypeBase : IndexFragment
{
    protected IndexTypeBase(string name) : base(name)
    {
    }
}
