namespace Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments
{
    public static class IndexUtils
    {
        public static string GetBaseName(IndexPrefix prefix, IndexTypeBase type)
        {
            // The final formatr <datatype>_[<optional_prefix>]_<customerid>_<yyyy>
            var pf = prefix.ToString();
            return string.IsNullOrWhiteSpace(pf) ? $"{type}" : $"{type}_{pf}";
        }
    }
}
