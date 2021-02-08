using System;
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
            }
        }
    }
}
