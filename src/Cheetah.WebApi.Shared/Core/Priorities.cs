using Cheetah.WebApi.Shared.Infrastructure.ServiceProvider;

namespace Cheetah.WebApi.Shared.Core
{
    public class Priorities
    {
        public const int Default = InstallerPriorityAttribute.DefaultPriority;

        public const int BeforeConfig = Default - 1;
        public const int AfterConfig = Default + 1;
    }
}
