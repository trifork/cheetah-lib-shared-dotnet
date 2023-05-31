using Cheetah.ComponentTest;
using ComponentTestExample;

await new ComponentTestRunner()
    .AddTest<ExampleComponentTest>()
    .WithConfiguration<ComponentTestConfig>(ComponentTestConfig.Position)
    .RunAsync(args);
