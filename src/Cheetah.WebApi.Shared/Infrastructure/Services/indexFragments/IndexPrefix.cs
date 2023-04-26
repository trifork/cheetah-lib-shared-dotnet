namespace Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments
{
  public class IndexPrefix : IndexFragment
  {
    private IndexPrefix() : base()
    {
    }

    public IndexPrefix(string value) : base(value) { }
    public static IndexPrefix Empty = new();
  }
}