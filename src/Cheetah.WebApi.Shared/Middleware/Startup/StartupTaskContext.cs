using System.Threading;

namespace Cheetah.WebApi.Shared.Middleware.Startup
{
    public class StartupTaskContext
    {
        private int outstandingTaskCount;

        /// <summary>
        /// Register a task to shared context
        /// </summary>
        public void RegisterTask()
        {
            Interlocked.Increment(ref outstandingTaskCount);
        }

        /// <summary>
        /// Mark a task as done to shared context 
        /// </summary>
        public void MarkTaskAsComplete()
        {
            Interlocked.Decrement(ref outstandingTaskCount);
        }

        /// <summary>
        /// Check if all tasks is complete
        /// </summary>
        public bool IsComplete => outstandingTaskCount == 0;
    }
}
