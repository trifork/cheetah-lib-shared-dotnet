using Serilog;

namespace Cheetah.ComponentTest
{
  public abstract class ComponentTest : IComponentTest
  {
    /// <summary>
    /// Maximum time the test is allowed to run for before being automatically failed
    /// </summary>
    protected abstract TimeSpan TestTimeout { get; }
    internal abstract Task Arrange(CancellationToken cancellationToken);
    internal abstract Task Act(CancellationToken cancellationToken);
    internal abstract Task<TestResult> Assert(CancellationToken cancellationToken);
    public async Task<TestResult> RunAsync(CancellationToken cancellationToken)
    {
      var timeoutCtx = new CancellationTokenSource(TestTimeout);
      var linkedCtx = CancellationTokenSource.CreateLinkedTokenSource(timeoutCtx.Token, cancellationToken);
      try
      {
        Log.Information("Running {testName}...", GetType().Name);
        Log.Information("============= Arrange =============");
        await Arrange(linkedCtx.Token);
        Log.Information("============= Act     =============");
        await Act(linkedCtx.Token);
        Log.Information("============= Assert  =============");
        var result = await Assert(linkedCtx.Token);

        if (result.IsPassed)
        {
          Log.Information("{testName} passed!\n", GetType().Name);
        }
        else
        {
          Log.Error("{testName} failed!\n", GetType().Name);
        }

        return result;
      }
      catch (OperationCanceledException ex)
      {
        if (timeoutCtx.IsCancellationRequested)
        {
          return TestResult.Failed($"The test did not finish before the test timeout of '{TestTimeout}'");
        }

        throw;
      }
      catch (Exception ex)
      {
        return TestResult.Failed($"An exception was thrown of type '{ex.GetType().Name} with message '{ex.Message}'");
      }
    }
  }
}