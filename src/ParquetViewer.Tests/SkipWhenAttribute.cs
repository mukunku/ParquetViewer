namespace ParquetViewer.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SkipWhenAttribute : Attribute
    {
        public Type TestClassToSkip { get; }
        public string? Reason { get; set; }

        public SkipWhenAttribute(Type testClassToSkip, string? reason)
        {
            TestClassToSkip = testClassToSkip;
            Reason = reason;
        }
    }
}
