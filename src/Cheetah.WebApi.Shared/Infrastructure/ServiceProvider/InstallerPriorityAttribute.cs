[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class InstallerPriorityAttribute : Attribute
{
    public const int DefaultPriority = 100;

    public int Priority { get; private set; }

    public InstallerPriorityAttribute(int priority)
    {
        Priority = priority;
    }

}