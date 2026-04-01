#if DEBUG
namespace GodotUtils.Debugging;

/// <summary>
/// Builds anchor-selection popup components used by the visual title bar.
/// </summary>
internal interface IVisualTitleBarAnchorPopupBuilder
{
    /// <summary>
    /// Creates a configured anchor popup instance.
    /// </summary>
    /// <returns>Anchor popup component ready to be attached to title-bar controls.</returns>
    VisualTitleBarAnchorPopup Build();
}
#endif
