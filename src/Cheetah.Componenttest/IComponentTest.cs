namespace Common;

public interface IComponentTest
{
    Task<TestResult> RunAsync(CancellationToken cancellationToken);
}