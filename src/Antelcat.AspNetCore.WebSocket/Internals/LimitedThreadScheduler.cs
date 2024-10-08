// Provides a task scheduler that ensures a maximum concurrency level while
// running on top of the thread pool.

namespace Antelcat.AspNetCore.WebSocket.Internals;

internal class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
{
    [ThreadStatic] private static bool currentThreadIsProcessingItems;

    private readonly LinkedList<Task> tasks = [];

    public int Parallelism
    {
        get => parallelism;
        set
        {
            lock (tasks)
            {
                parallelism = value;
                var tmp = value - delegatesQueuedOrRunning;
                while (tmp > 0)
                {
                    ++delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                    tmp--;
                }
            }
        }
    }

    private int parallelism;

    private int delegatesQueuedOrRunning;

    public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);
#else
        if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
#endif
        parallelism = maxDegreeOfParallelism;
    }

    // Queues a task to the scheduler.
    protected sealed override void QueueTask(Task task)
    {
        lock (tasks)
        {
            tasks.AddLast(task);
            if (delegatesQueuedOrRunning >= Parallelism) return;
            ++delegatesQueuedOrRunning;
            NotifyThreadPoolOfPendingWork();
        }
    }

    private void NotifyThreadPoolOfPendingWork()
    {
        ThreadPool.UnsafeQueueUserWorkItem(_ =>
        {
            currentThreadIsProcessingItems = true;
            try
            {
                while (true)
                {
                    Task item;
                    lock (tasks)
                    {
                        if (tasks.Count == 0)
                        {
                            --delegatesQueuedOrRunning;
                            break;
                        }

                        item = tasks.First!.Value;
                        tasks.RemoveFirst();
                    }

                    TryExecuteTask(item);
                }
            }
            finally
            {
                currentThreadIsProcessingItems = false;
            }
        }, null);
    }

    protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) =>
        currentThreadIsProcessingItems && (taskWasPreviouslyQueued
            ? TryDequeue(task) && TryExecuteTask(task)
            : TryExecuteTask(task));

    protected sealed override bool TryDequeue(Task task)
    {
        lock (tasks) return tasks.Remove(task);
    }

    public sealed override int MaximumConcurrencyLevel => parallelism;

    protected sealed override IEnumerable<Task> GetScheduledTasks()
    {
        var lockTaken = false;
        try
        {
            Monitor.TryEnter(tasks, ref lockTaken);
            if (lockTaken) return tasks;
            else throw new NotSupportedException();
        }
        finally
        {
            if (lockTaken) Monitor.Exit(tasks);
        }
    }
}