#if DEBUG
namespace GodotUtils.Debugging;

/// <summary>
/// Wraps an optional visual control result from control-factory helpers.
/// </summary>
/// <param name="visualControl">Created visual control, or null when unsupported.</param>
internal sealed class VisualControlInfo(IVisualControl? visualControl)
{
    /// <summary>
    /// Created visual control instance.
    /// </summary>
    public IVisualControl? VisualControl { get; } = visualControl;
}
#endif
