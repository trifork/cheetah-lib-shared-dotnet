using Cheetah.ComponentTest;
using Serilog.Core;
using Serilog.Events;

await new ComponentTestRunner()
    .AddAllTests()
    .WithConfiguration<ComponentTestConfig>(ComponentTestConfig.Position)
    .WithLoggingLevelSwitch(new LoggingLevelSwitch(LogEventLevel.Information))
    .RunAsync(args);
