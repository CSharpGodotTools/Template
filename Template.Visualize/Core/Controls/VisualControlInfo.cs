#if DEBUG
namespace GodotUtils.Debugging;

internal sealed class VisualControlInfo(IVisualControl? visualControl)
{
    public IVisualControl? VisualControl { get; } = visualControl;
}
#endif
