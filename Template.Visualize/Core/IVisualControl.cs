#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal interface IVisualControl
{
    void SetValue(object value);
    Control Control { get; }
    void SetEditable(bool editable);
}
#endif
