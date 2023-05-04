namespace Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments
{
  public class IndexPrefix : IndexFragment
  {
    private IndexPrefix() : base()
    {
    }

    public IndexPrefix(string value) : base(value) { }
    public static readonly IndexPrefix Empty = new();
  }
}