namespace Cheetah.ComponentTest;

public interface IComponentTest
{
    Task<TestResult> RunAsync(CancellationToken cancellationToken);
}