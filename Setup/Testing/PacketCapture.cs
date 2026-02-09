using System;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

public sealed class PacketCapture<T>
{
    private readonly TaskCompletionSource<bool> _tcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public T Packet { get; private set; }

    public void Set(T packet)
    {
        Packet = packet;
        _tcs.TrySetResult(true);
    }

    public async Task<bool> WaitAsync(TimeSpan timeout)
    {
        Task completed = await Task.WhenAny(_tcs.Task, Task.Delay(timeout));
        return completed == _tcs.Task;
    }
}
