using System.Runtime.CompilerServices;

namespace ParquetViewer.Tests
{
    internal class SkippableTestMethodAttribute : TestMethodAttribute
    {
        public SkippableTestMethodAttribute([CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1) 
            : base(callerFilePath, callerLineNumber)
        {
            
        }

        public override Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
        {
            var methodInfo = testMethod.MethodInfo;
            var skipAttrs = methodInfo.GetCustomAttributes(typeof(SkipWhenAttribute), inherit: true)
                                      .Cast<SkipWhenAttribute>()
                                      .ToList();

            var skipAttribute = skipAttrs.FirstOrDefault(a => a.TestClassToSkip.FullName == testMethod.TestClassName);
            if (skipAttribute is not null)
            {
                var result = new TestResult
                {
                    Outcome = UnitTestOutcome.Inconclusive, // treated as skipped in MSTest
                    TestFailureException = null
                };
                result.TestContextMessages 
                    = $"Test skipped for {testMethod.TestClassName}.{testMethod.TestMethodName}" +
                    $"{(skipAttribute.Reason is not null ? $" {skipAttribute.Reason}" : string.Empty)}.";

                return Task.FromResult(new[] { result });
            }

            return base.ExecuteAsync(testMethod);
        }
    }
}
