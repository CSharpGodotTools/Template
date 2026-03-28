#if DEBUG
namespace GodotUtils.Debugging;

internal interface IVisualDictionaryDisplayOrderTrackerFactory
{
    IVisualDictionaryDisplayOrderTracker Create(bool useStableOrder);
}
#endif
