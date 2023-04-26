using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cheetah.WebApi.Shared.Infrastructure.ServiceProvider
{
  public static class ServiceCollectionExtensions
  {
    public static void Install(this IServiceCollection services, IHostEnvironment hostEnvironment,
        Assembly assembly, int? priorityFilter = null, IServiceCollectionInstaller[]? additionalInstallers = null)
    {
      var installerTypes = FilterInstallerTypes(assembly.GetAvailableTypes());
      if (installerTypes == null)
      {
        return;
      }

      var allInstallers = installerTypes.Select(x => (IServiceCollectionInstaller)Activator.CreateInstance(x)!);
      if (additionalInstallers != null)
      {
        allInstallers = allInstallers.Concat(additionalInstallers).Distinct(new ServiceCollectionInstallerComparer());
      }

      if (priorityFilter.HasValue)
      {
        allInstallers = allInstallers.Where(x => x != null && GetPriority(x) == priorityFilter).ToArray();
      }

      var orderedInstallerTypes = allInstallers.OrderBy(GetPriority);

      foreach (var installer in orderedInstallerTypes)
      {
        installer?.Install(services, hostEnvironment);
      }
    }

    public static void Install(this IServiceCollection services, IHostEnvironment hostEnvironment, params IServiceCollectionInstaller[] allInstallers)
    {
      var orderedInstallerTypes = allInstallers.OrderBy(GetPriority);

      foreach (var installer in orderedInstallerTypes)
      {
        installer.Install(services, hostEnvironment);
      }
    }


    private static int GetPriority(IServiceCollectionInstaller serviceCollectionInstaller)
    {
      return serviceCollectionInstaller.GetType().GetCustomAttributes(typeof(InstallerPriorityAttribute), false)
        .FirstOrDefault() is InstallerPriorityAttribute attribute ? attribute.Priority : InstallerPriorityAttribute.DefaultPriority;
    }

    private static IEnumerable<Type> FilterInstallerTypes(IEnumerable<Type> types)
    {
      return types.Where(t => t.GetTypeInfo().IsClass &&
                              t.GetTypeInfo().IsAbstract == false &&
                              t.GetTypeInfo().IsGenericTypeDefinition == false &&
                              typeof(IServiceCollectionInstaller).IsAssignableFrom(t));
    }

    private static Type[] GetAvailableTypes(this Assembly assembly, bool includeNonExported = false)
    {
      try
      {
        if (includeNonExported)
        {
          return assembly.GetTypes();
        }
        return assembly.GetExportedTypes();
      }
      catch (ReflectionTypeLoadException e)
      {
        return e.Types.Where(x => x != null).Select(x => x!).ToArray();
        // NOTE: perhaps we should not ignore the exceptions here, and log them?
      }
    }
  }
}