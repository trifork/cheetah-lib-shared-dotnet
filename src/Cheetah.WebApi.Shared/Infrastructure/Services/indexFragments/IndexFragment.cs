using System.Text.RegularExpressions;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments
{
  public class IndexFragment
  {
    private static readonly Regex LettersAndNumbersOnly = new("[^A-Za-z0-9_-]");
    protected string Value { get; }

    protected IndexFragment(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        throw new IndexFragmentException();
      }

      if (LettersAndNumbersOnly.IsMatch(value))
        throw new IndexFragmentException();

      Value = value.ToLowerInvariant();
    }

    protected IndexFragment()
    {
      Value = string.Empty;
    }

    public override string ToString()
    {
      return Value;
    }
  }
}