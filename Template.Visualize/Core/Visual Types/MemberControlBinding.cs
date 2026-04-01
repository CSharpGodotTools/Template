#if DEBUG
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Binds a reflected member to its visual control and editability state.
/// </summary>
/// <param name="member">Field or property metadata.</param>
/// <param name="control">Control used to display/edit the member.</param>
/// <param name="isEditable">Whether edits should be allowed for this member.</param>
internal sealed class MemberControlBinding(MemberInfo member, IVisualControl control, bool isEditable)
{
    /// <summary>
    /// Reflected member metadata.
    /// </summary>
    public MemberInfo Member { get; } = member;

    /// <summary>
    /// Bound visual control.
    /// </summary>
    public IVisualControl Control { get; } = control;

    /// <summary>
    /// Indicates whether member edits are allowed.
    /// </summary>
    public bool IsEditable { get; } = isEditable;
}
#endif
