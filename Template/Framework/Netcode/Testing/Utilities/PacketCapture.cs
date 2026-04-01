using System;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

/// <summary>
/// Stores the latest captured packet and exposes async wait semantics.
/// </summary>
/// <typeparam name="T">Captured packet type.</typeparam>
public sealed class PacketCapture<T>
{
    private readonly TaskCompletionSource<bool> _tcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Gets the captured packet value.
    /// </summary>
    public T Packet { get; private set; } = default!;

    /// <summary>
    /// Gets a value indicating whether a packet has been captured.
    /// </summary>
    public bool IsSet => _tcs.Task.IsCompleted;

    /// <summary>
    /// Sets the captured packet and completes pending waiters.
    /// </summary>
    /// <param name="packet">Captured packet value.</param>
    public void Set(T packet)
    {
        Packet = packet;
        Console.WriteLine($"[Test] Packet captured: {typeof(T).Name}");
        _tcs.TrySetResult(true);
    }

    /// <summary>
    /// Waits for capture completion until timeout.
    /// </summary>
    /// <param name="timeout">Maximum wait duration.</param>
    /// <returns><see langword="true"/> when capture completed before timeout.</returns>
    public async Task<bool> WaitAsync(TimeSpan timeout)
    {

        // Return whether packet capture finished before the timeout task won.
        Task completed = await Task.WhenAny(_tcs.Task, Task.Delay(timeout));
        return completed == _tcs.Task;
    }
}
