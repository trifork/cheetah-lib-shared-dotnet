namespace Cheetah.ComponentTest
{
    public struct TestResult
    {
        public bool IsPassed { get; }
        public string? ErrorMessage { get; set; }

        private TestResult(bool isPassed, string? errorMessage = null)
        {
            IsPassed = isPassed;
            ErrorMessage = errorMessage;
        }

        public static TestResult Passed => new(true);

        public static TestResult Failed(string errorMessage)
        {
            return new(false, errorMessage);
        }
    };
}
