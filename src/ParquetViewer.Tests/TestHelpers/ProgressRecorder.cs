namespace ParquetViewer.Tests.TestHelpers;

/// <summary>
/// Records progress synchronously, unlike <see cref="Progress{T}"/> which posts reports to a sync context.
/// </summary>
/// <param name="onReport">Optional callback run after each report, e.g. to cancel part way through</param>
public sealed class ProgressRecorder(Action? onReport = null) : IProgress<int>
{
    private int _total;
    private readonly List<int> _reports = [];

    public int Total => _total;

    public int[] Reports
    {
        get
        {
            lock (_reports)
                return [.. _reports];
        }
    }

    public void Report(int value)
    {
        Interlocked.Add(ref _total, value);
        lock (_reports)
            _reports.Add(value);

        onReport?.Invoke();
    }
}