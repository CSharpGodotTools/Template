#if DEBUG
namespace GodotUtils.Debugging;

/// <summary>
/// Creates display-order trackers for dictionary visualization.
/// </summary>
internal sealed class VisualDictionaryDisplayOrderTrackerFactory : IVisualDictionaryDisplayOrderTrackerFactory
{
    /// <summary>
    /// Creates a display-order tracker instance.
    /// </summary>
    /// <param name="useStableOrder">Whether to preserve stable ordering.</param>
    /// <returns>Tracker instance.</returns>
    public IVisualDictionaryDisplayOrderTracker Create(bool useStableOrder)
    {
        return new VisualDictionaryDisplayOrderTracker(useStableOrder);
    }
}
#endif
