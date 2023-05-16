using System.Threading;
using System.Threading.Tasks;

namespace Cheetah.ComponentTest
{
    public interface IComponentTest
    {
        Task<TestResult> RunAsync(CancellationToken cancellationToken);
    }
}
