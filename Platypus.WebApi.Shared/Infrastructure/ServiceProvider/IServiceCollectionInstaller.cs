using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Platypus.WebApi.Shared.Infrastructure.ServiceProvider
{
    public interface IServiceCollectionInstaller
    {
        void Install(IServiceCollection services, IHostEnvironment hostEnvironment);
    }
}
