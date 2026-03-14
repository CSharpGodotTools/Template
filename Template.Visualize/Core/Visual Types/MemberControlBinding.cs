#if DEBUG
using System.Reflection;

namespace GodotUtils.Debugging;

internal sealed class MemberControlBinding(MemberInfo member, IVisualControl control, bool isEditable)
{
    public MemberInfo Member { get; } = member;
    public IVisualControl Control { get; } = control;
    public bool IsEditable { get; } = isEditable;
}
#endif
