#if DEBUG
namespace GodotUtils.Debugging;

/// <summary>
/// Layout and timing constants shared across visualization UI components.
/// </summary>
internal static class VisualUiLayout
{
    /// <summary>
    /// Scale multiplier used for log labels.
    /// </summary>
    public const float LogScaleFactor = 0.6f;

    /// <summary>
    /// Scale multiplier for the main visualization panel.
    /// </summary>
    public const float PanelScaleFactor = 0.9f;

    /// <summary>
    /// Minimum vertical size for the members scroll area.
    /// </summary>
    public const int MinScrollViewDistance = 350;

    /// <summary>
    /// Font size for title rows.
    /// </summary>
    public const int TitleFontSize = 20;

    /// <summary>
    /// Font size for member labels/values.
    /// </summary>
    public const int MemberFontSize = 18;

    /// <summary>
    /// Minimum width reserved for member labels.
    /// </summary>
    public const float MemberLabelMinWidth = 120f;

    /// <summary>
    /// Outline thickness used for panel text.
    /// </summary>
    public const int FontOutlineSize = 6;

    /// <summary>
    /// Minimum square size for title-bar buttons.
    /// </summary>
    public const int MinButtonSize = 25;

    /// <summary>
    /// Delay before button focus is released after press.
    /// </summary>
    public const double ReleaseFocusOnPressDelay = 0.1;

    /// <summary>
    /// Max seconds to wait for initial readonly member values.
    /// </summary>
    public const int MaxSecondsToWaitForInitialValues = 3;
}
#endif
