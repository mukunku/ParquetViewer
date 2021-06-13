using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParquetFileViewer.Helpers
{
    public static class ParallelAsync
    {
        public static async Task ForeachAsync<T>(IEnumerable<T> source, int maxParallelCount, Func<T, Task> action)
        {
            var exceptions = new ConcurrentBag<Exception>();
            using (SemaphoreSlim completeSemphoreSlim = new SemaphoreSlim(1))
            using (SemaphoreSlim taskCountLimitsemaphoreSlim = new SemaphoreSlim(maxParallelCount))
            {
                await completeSemphoreSlim.WaitAsync();
                int runningtaskCount = source.Count();

                foreach (var item in source)
                {
                    await taskCountLimitsemaphoreSlim.WaitAsync();

                    Task.Run(async () =>
                    {
                        try
                        {
                            await action(item).ContinueWith(task =>
                            {
                                if (task.IsFaulted)
                                    exceptions.Add(task.Exception?.InnerException ?? task.Exception ?? new Exception("Unknown Processing Error"));

                                Interlocked.Decrement(ref runningtaskCount);
                                if (runningtaskCount == 0)
                                {
                                    completeSemphoreSlim.Release();
                                }
                            });
                        }
                        finally
                        {
                            taskCountLimitsemaphoreSlim.Release();
                        }
                    }).GetHashCode();
                }

                await completeSemphoreSlim.WaitAsync();

                if (exceptions.Count > 0)
                    throw new AggregateException(exceptions.ToArray());
            }
        }
    }
}
