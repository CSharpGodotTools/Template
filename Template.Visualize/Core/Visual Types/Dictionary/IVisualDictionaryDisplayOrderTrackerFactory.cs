#if DEBUG
namespace GodotUtils.Debugging;

/// <summary>
/// Factory for building display-order trackers.
/// </summary>
internal interface IVisualDictionaryDisplayOrderTrackerFactory
{
    /// <summary>
    /// Creates a display-order tracker instance.
    /// </summary>
    /// <param name="useStableOrder">Whether to preserve stable ordering.</param>
    /// <returns>Tracker instance.</returns>
    IVisualDictionaryDisplayOrderTracker Create(bool useStableOrder);
}
#endif
