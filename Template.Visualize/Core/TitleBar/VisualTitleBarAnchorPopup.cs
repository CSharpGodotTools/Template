#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal readonly record struct VisualTitleBarAnchorPopup(PopupPanel Popup, Action RefreshSelection);
#endif
