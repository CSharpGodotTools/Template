#if DEBUG
namespace GodotUtils.Debugging;

internal sealed class VisualDictionaryDisplayOrderTrackerFactory : IVisualDictionaryDisplayOrderTrackerFactory
{
    public IVisualDictionaryDisplayOrderTracker Create(bool useStableOrder)
    {
        return new VisualDictionaryDisplayOrderTracker(useStableOrder);
    }
}
#endif
