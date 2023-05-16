using Cheetah.ComponentTest;

await new ComponentTestRunner()
    .AddAllTests()
    .WithConfiguration<ComponentTestConfig>(ComponentTestConfig.Position)
    .RunAsync(args);
