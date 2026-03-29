using System.Threading;

namespace __TEMPLATE__.Netcode;

internal sealed class QueueMetrics
{
    private int _depth;
    private int _highWaterMark;
    private long _droppedCount;

    public int HighWaterMark => Volatile.Read(ref _highWaterMark);
    public long DroppedCount => Interlocked.Read(ref _droppedCount);

    public bool TryReserve(int maxDepth, out int depthAfterReserve)
    {
        while (true)
        {
            int observed = Volatile.Read(ref _depth);
            if (observed >= maxDepth)
            {
                depthAfterReserve = observed;
                return false;
            }

            int next = observed + 1;
            if (Interlocked.CompareExchange(ref _depth, next, observed) != observed)
                continue;

            depthAfterReserve = next;
            UpdateHighWaterMark(next);
            return true;
        }
    }

    public void Release()
    {
        Interlocked.Decrement(ref _depth);
    }

    public long IncrementDropped()
    {
        return Interlocked.Increment(ref _droppedCount);
    }

    private void UpdateHighWaterMark(int currentDepth)
    {
        while (true)
        {
            int observed = Volatile.Read(ref _highWaterMark);
            if (currentDepth <= observed)
                return;

            if (Interlocked.CompareExchange(ref _highWaterMark, currentDepth, observed) == observed)
                return;
        }
    }
}
