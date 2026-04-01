#if DEBUG
using Godot;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Composite visual control that forwards value/update operations to bound class members.
/// </summary>
/// <param name="container">Root container hosting member controls.</param>
/// <param name="bindings">Member-to-control bindings.</param>
internal sealed class ClassControl(Control container, List<MemberControlBinding> bindings) : IVisualControl
{
    /// <summary>
    /// Pushes member values from <paramref name="value"/> to each bound child control.
    /// </summary>
    /// <param name="value">Object instance containing bound member values.</param>
    public void SetValue(object value)
    {
        foreach (MemberControlBinding binding in bindings)
        {
            object? memberValue = VisualHandler.GetMemberValue(binding.Member, value);
            binding.Control.SetValue(memberValue!);
        }
    }

    /// <summary>
    /// Root container for this composite control.
    /// </summary>
    public Control Control => container;

    /// <summary>
    /// Applies editable state to bound child controls while respecting per-member editability.
    /// </summary>
    /// <param name="editable">True to allow edits where supported.</param>
    public void SetEditable(bool editable)
    {
        foreach (MemberControlBinding binding in bindings)
        {
            binding.Control.SetEditable(editable && binding.IsEditable);
        }
    }
}
#endif
