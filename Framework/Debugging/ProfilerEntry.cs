using Godot;

namespace GodotUtils.Debugging;

public class ProfilerEntry
{
    public ulong StartTimeUsec { get; private set; }
    public ulong AccumulatedTimeUsec { get; private set; }
    public int FrameCount { get; private set; }

    public void Start()
    {
        StartTimeUsec = Time.GetTicksUsec();
    }

    public void Stop()
    {
        AccumulatedTimeUsec += Time.GetTicksUsec() - StartTimeUsec;
        FrameCount++;
    }

    public void Reset()
    {
        AccumulatedTimeUsec = 0UL;
        FrameCount = 0;
    }

    /// <summary>
    /// Returns the average frame time in milliseconds with specified accuracy.
    /// </summary>
    public string GetAverageMs(int accuracy)
    {
        if (FrameCount == 0)
            return "0.0";

        double avgMs = (double)AccumulatedTimeUsec / FrameCount / 1000.0;
        return avgMs.ToString($"F{accuracy}");
    }
}
