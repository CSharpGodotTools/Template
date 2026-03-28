using System;
using System.Threading;
using System.Threading.Tasks;

namespace __TEMPLATE__;

public interface IBackgroundTaskTracker : IDisposable
{
    CancellationToken ShutdownToken { get; }

    void Run(Func<CancellationToken, Task> taskFactory, string taskName);
    void Track(Task task, string taskName);

    Task WaitForAllAsync();
    Task ShutdownAsync();
}
