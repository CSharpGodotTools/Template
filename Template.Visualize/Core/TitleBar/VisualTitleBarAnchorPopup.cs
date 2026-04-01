#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Bundles anchor popup UI and the action that refreshes its selected-state visuals.
/// </summary>
/// <param name="Popup">Popup panel containing anchor selection buttons.</param>
/// <param name="RefreshSelection">Action that refreshes selected button styling.</param>
internal readonly record struct VisualTitleBarAnchorPopup(PopupPanel Popup, Action RefreshSelection);
#endif
