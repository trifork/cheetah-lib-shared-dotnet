namespace Cheetah.WebApi.Shared.Infrastructure.ServiceProvider
{
    public class Priorities
    {
        public const int Default = InstallerPriorityAttribute.DefaultPriority;

        public const int BeforeConfig = Default - 1;
        public const int AfterConfig = Default + 1;
    }
}
