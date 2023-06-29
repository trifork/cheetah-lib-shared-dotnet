namespace Cheetah.ComponentTest.XUnit.Model;

public class OpenSearchTestModel
{
    public string TestString { get; }
    public int TestInteger { get; }

    public OpenSearchTestModel(string testString, int testInteger) {
        TestString = testString;
        TestInteger = testInteger;
    }
}
