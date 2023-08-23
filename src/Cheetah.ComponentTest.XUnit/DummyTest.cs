namespace Cheetah.ComponentTest.XUnit;


[Trait("TestType", "IntegrationTests")]
public class DummyTest
{
    [Fact]
    public void FooTest()
    {
        Assert.True(true);
    }
}
