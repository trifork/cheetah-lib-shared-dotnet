namespace Platypus.WebApi.Shared.Infrastructure.ServiceProvider
{
    /// <summary>
    /// We want to do a value compare on type
    /// </summary>
    public class ServiceCollectionInstallerComparer : IEqualityComparer<IServiceCollectionInstaller>
    {
        public bool Equals(IServiceCollectionInstaller x, IServiceCollectionInstaller y)
        {
            return y != null && x != null && x.GetType().ToString() == y.GetType().ToString();
        }

        public int GetHashCode(IServiceCollectionInstaller obj)
        {
            return obj.GetType().ToString().GetHashCode();
        }
    }
}
