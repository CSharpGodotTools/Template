using System;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

public sealed class PacketCapture<T>
{
    private readonly TaskCompletionSource<bool> _tcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public T Packet { get; private set; }
    public bool IsSet => _tcs.Task.IsCompleted;

    public void Set(T packet)
    {
        Packet = packet;
        Console.WriteLine($"[Test] Packet captured: {typeof(T).Name}");
        _tcs.TrySetResult(true);
    }

    public async Task<bool> WaitAsync(TimeSpan timeout)
    {
        Task completed = await Task.WhenAny(_tcs.Task, Task.Delay(timeout));
        return completed == _tcs.Task;
    }
}
