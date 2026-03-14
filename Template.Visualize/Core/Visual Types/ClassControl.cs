#if DEBUG
using Godot;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class ClassControl(Control container, List<MemberControlBinding> bindings) : IVisualControl
{
    public void SetValue(object value)
    {
        foreach (MemberControlBinding binding in bindings)
        {
            object? memberValue = VisualHandler.GetMemberValue(binding.Member, value);
            binding.Control.SetValue(memberValue!);
        }
    }

    public Control Control => container;

    public void SetEditable(bool editable)
    {
        foreach (MemberControlBinding binding in bindings)
        {
            binding.Control.SetEditable(editable && binding.IsEditable);
        }
    }
}
#endif
