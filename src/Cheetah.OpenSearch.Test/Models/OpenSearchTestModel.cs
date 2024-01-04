namespace Cheetah.OpenSearch.Test.Models
{
    public class OpenSearchTestModel
    {
        public OpenSearchTestModel(string testString, int testInteger)
        {
            TestString = testString;
            TestInteger = testInteger;
        }
        
        public string TestString { get; set; }
        public int TestInteger { get; set; }
    }
}
