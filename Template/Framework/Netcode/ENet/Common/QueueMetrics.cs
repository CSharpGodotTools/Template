using System.Threading;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Tracks queue depth, high-water mark, and drop counts for bounded worker queues.
/// </summary>
internal sealed class QueueMetrics
{
    private int _depth;
    private int _highWaterMark;
    private long _droppedCount;

    /// <summary>
    /// Gets the highest observed queue depth.
    /// </summary>
    public int HighWaterMark => Volatile.Read(ref _highWaterMark);

    /// <summary>
    /// Gets total number of dropped items recorded for this queue.
    /// </summary>
    public long DroppedCount => Interlocked.Read(ref _droppedCount);

    /// <summary>
    /// Attempts to reserve one queue slot when current depth is below max depth.
    /// </summary>
    /// <param name="maxDepth">Maximum allowed depth.</param>
    /// <param name="depthAfterReserve">Depth after reservation succeeds or current depth when it fails.</param>
    /// <returns><see langword="true"/> when reservation succeeds.</returns>
    public bool TryReserve(int maxDepth, out int depthAfterReserve)
    {
        while (true)
        {
            int observed = Volatile.Read(ref _depth);

            // Reject reservations when queue depth is already at or above limit.
            if (observed >= maxDepth)
            {
                depthAfterReserve = observed;
                return false;
            }

            int next = observed + 1;

            // Retry when another thread changed depth before this CAS completed.
            if (Interlocked.CompareExchange(ref _depth, next, observed) != observed)
                continue;

            // Update high-water mark only after the reservation is atomically committed.
            depthAfterReserve = next;
            UpdateHighWaterMark(next);
            return true;
        }
    }

    /// <summary>
    /// Releases one reserved queue slot.
    /// </summary>
    public void Release()
    {
        Interlocked.Decrement(ref _depth);
    }

    /// <summary>
    /// Increments and returns the dropped-item counter.
    /// </summary>
    /// <returns>Updated dropped-item count.</returns>
    public long IncrementDropped()
    {
        return Interlocked.Increment(ref _droppedCount);
    }

    /// <summary>
    /// Updates the high-water mark using lock-free compare-and-swap.
    /// </summary>
    /// <param name="currentDepth">Current observed queue depth.</param>
    private void UpdateHighWaterMark(int currentDepth)
    {
        while (true)
        {
            int observed = Volatile.Read(ref _highWaterMark);

            // Stop when current depth does not exceed the recorded high-water mark.
            if (currentDepth <= observed)
                return;

            // Stop once high-water mark is updated by this thread.
            if (Interlocked.CompareExchange(ref _highWaterMark, currentDepth, observed) == observed)
                return;
        }
    }
}
