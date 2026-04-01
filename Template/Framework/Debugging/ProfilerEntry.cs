using Godot;

namespace __TEMPLATE__.Debugging;

/// <summary>
/// Stores timing samples for a single profiler key across multiple frames.
/// </summary>
public class ProfilerEntry
{
    /// <summary>
    /// Gets the timestamp at which the current sample started, in microseconds.
    /// </summary>
    public ulong StartTimeUsec { get; private set; }

    /// <summary>
    /// Gets accumulated sampled time, in microseconds.
    /// </summary>
    public ulong AccumulatedTimeUsec { get; private set; }

    /// <summary>
    /// Gets how many samples have been accumulated.
    /// </summary>
    public int FrameCount { get; private set; }

    /// <summary>
    /// Starts a new timing sample.
    /// </summary>
    public void Start()
    {
        StartTimeUsec = Time.GetTicksUsec();
    }

    /// <summary>
    /// Stops the active timing sample and accumulates it.
    /// </summary>
    public void Stop()
    {
        AccumulatedTimeUsec += Time.GetTicksUsec() - StartTimeUsec;
        FrameCount++;
    }

    /// <summary>
    /// Clears accumulated timing and frame counters.
    /// </summary>
    public void Reset()
    {
        AccumulatedTimeUsec = 0UL;
        FrameCount = 0;
    }

    /// <summary>
    /// Returns the average frame time in milliseconds with specified accuracy.
    /// </summary>
    /// <param name="accuracy">Number of fractional digits to include in the formatted result.</param>
    /// <returns>Average frame time in milliseconds formatted with the requested precision.</returns>
    public string GetAverageMs(int accuracy)
    {
        // Avoid division by zero when no samples have been recorded.
        if (FrameCount == 0)
            return "0.0";

        double avgMs = (double)AccumulatedTimeUsec / FrameCount / 1000.0;
        return avgMs.ToString($"F{accuracy}");
    }
}
