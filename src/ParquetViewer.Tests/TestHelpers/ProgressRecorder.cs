namespace ParquetViewer.Tests.TestHelpers;

public sealed class ProgressRecorder : IProgress<int>
{
    private int _total;
    public int Total => _total;
    public void Report(int value) => Interlocked.Add(ref _total, value);
}