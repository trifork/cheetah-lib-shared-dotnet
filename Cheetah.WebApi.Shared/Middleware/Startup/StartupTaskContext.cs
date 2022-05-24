namespace Cheetah.WebApi.Shared.Middleware.Startup;

public class StartupTaskContext
{
    private int outstandingTaskCount;

    public void RegisterTask()
    {
        Interlocked.Increment(ref outstandingTaskCount);
    }

    public void MarkTaskAsComplete()
    {
        Interlocked.Decrement(ref outstandingTaskCount);
    }

    public bool IsComplete => outstandingTaskCount == 0;
}