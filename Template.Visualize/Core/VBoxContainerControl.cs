#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal sealed class VBoxContainerControl(VBoxContainer vboxContainer) : IVisualControl
{
    public void SetValue(object value)
    {
        // No specific value setting for VBoxContainer
    }

    public Control Control => vboxContainer;

    public void SetEditable(bool editable)
    {
        // No specific editable setting for VBoxContainer
    }
}
#endif
