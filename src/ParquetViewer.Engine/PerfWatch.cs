using System.Collections.Concurrent;
using System.Text;

namespace ParquetViewer.Engine
{
    public static class PerfWatch
    {
        private static readonly ConcurrentDictionary<string, PerfWatchRecord> _perfWatches = new();

        private static PerfWatchRecord GetOrCreate(string stopwatchId)
        {
            _perfWatches.TryAdd(stopwatchId, new PerfWatchRecord());
            return _perfWatches[stopwatchId];
        }

        public static void Milestone(string key, string message)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            GetOrCreate(key).Milestone(message);
        }

        public static void Milestone(string message)
        {
            GetOrCreate("MAIN").Milestone(message);
        }

        public static string PrintAndReset()
        {
            var sb = new StringBuilder();

            try
            {
                var list = new List<(DateTime, string)>();
                foreach (var perfWatch in _perfWatches.Where(pw => pw.Key != "MAIN"))
                {
                    var averageRuntime = TimeSpan.FromMilliseconds(perfWatch.Value.Milestones.Last().LoggedOn
                        .Subtract(perfWatch.Value.Milestones.First().LoggedOn).TotalMilliseconds / perfWatch.Value.Milestones.Count());
                    
                    list.Add((perfWatch.Value.Milestones.Last().LoggedOn, $"{perfWatch.Key}: Average time: {averageRuntime} Count: {perfWatch.Value.Milestones.Count()}"));
                }

                var allMilestones = _perfWatches.Where(pw => pw.Key == "MAIN").First().Value.Milestones.Concat(list).OrderBy(m => m.Item1);
                var previousMilestoneDate = DateTime.MinValue;
                foreach(var milestone in allMilestones)
                {
                    previousMilestoneDate = previousMilestoneDate == DateTime.MinValue ? milestone.Item1 : previousMilestoneDate;
                    var timePassed = previousMilestoneDate.Subtract(milestone.Item1).ToString("mm\\:ss\\.fff");
                    sb.AppendLine($"[{timePassed}] {milestone.Item2}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.ToString());
            }

            Reset();
            return sb.ToString();
        }

        private static void Reset()
        {
            _perfWatches.Clear();
        }

        private class PerfWatchRecord
        {
            public List<(DateTime LoggedOn, string Message)> Milestones { get; }

            public PerfWatchRecord()
            {
                Milestones = new(1024);
            }

            public void Milestone(string message)
            {
                Milestones.Add((DateTime.UtcNow, message/*, Environment.CurrentManagedThreadId*/));
            }
        }
    }
}
