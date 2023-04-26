using Microsoft.Extensions.Hosting;
using Serilog;

namespace Cheetah.ComponentTest;

public class ComponentTestWorker : BackgroundService
{
    private readonly List<IComponentTest> _componentTests;
    private readonly IHostApplicationLifetime _lifetime;
    private int _successfulTestCounter = 0;
    private readonly List<(string testName, string failureMessage)> _testFailures = new();
    private readonly object _stoppingLock = new();
    private bool _stopping;

    public ComponentTestWorker(IEnumerable<IComponentTest> componentTests, IHostApplicationLifetime lifetime)
    {
        _componentTests = componentTests.ToList();
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            foreach (var test in _componentTests)
            {
                var result = await test.RunAsync(stoppingToken);

                if (result.IsPassed)
                {
                    _successfulTestCounter++;
                }
                else
                {
                    _testFailures.Add((test.GetType().Name, result.ErrorMessage ?? "No failure message provided"));
                }
            }

            EndTest();
        }
        catch (Exception e)
        {
            lock (_stoppingLock)
            {
                // Suppress any exceptions thrown while the test is already stopping.
                if (_stopping) return;
            }

            Log.Error(
                "An unexpected exception was thrown while running tests. It had type '{exceptionType}' and message: '{message}'",
                e.GetType().FullName,
                e.Message);
            Exit(-1);
            throw;
        }
    }

    private void EndTest()
    {
        Log.Information("============= Results  =============");
        Log.Information("{totalTests} run, {passedTests} passed, {failedTests} failed.",
            _componentTests.Count,
            _successfulTestCounter,
            _testFailures.Count);

        if (_testFailures.Any())
        {
            Log.Error("{failCount} of {totalCount} tests failed. With the following reasons:",
                _testFailures.Count,
                _componentTests.Count);
            foreach (var failure in _testFailures)
            {
                Log.Error("{testName} failed with message: {failureMessage}", failure.testName, failure.failureMessage);
            }

            Exit(-1);
            return;
        }

        Log.Information("All tests passed");
        Exit(0);
    }

    private void Exit(int exitCode)
    {
        lock (_stoppingLock)
        {
            if (_stopping) return;
            _stopping = true;
        }

        Log.Information("Exiting with exit code: {exitCode}", exitCode);
        Environment.ExitCode = exitCode;
        _lifetime.StopApplication();
    }
}