#if DEBUG
using System;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Wraps readonly member metadata and value retrieval for visualization binding.
/// </summary>
/// <param name="name">Display/name key for the member.</param>
/// <param name="member">Reflection metadata for the underlying field or property.</param>
/// <param name="memberType">Declared CLR type of the underlying member.</param>
internal sealed class ReadonlyMemberAccessor(string name, MemberInfo member, Type memberType)
{
    /// <summary>
    /// Member display/name key.
    /// </summary>
    /// <value>Member display/name key.</value>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <summary>
    /// Reflection metadata for the underlying field or property.
    /// </summary>
    /// <value>Reflection metadata for the underlying field or property.</value>
    public MemberInfo Member { get; } = member ?? throw new ArgumentNullException(nameof(member));

    /// <summary>
    /// Declared type of the underlying member.
    /// </summary>
    /// <value>Declared type of the underlying member.</value>
    public Type MemberType { get; } = memberType ?? throw new ArgumentNullException(nameof(memberType));

    /// <summary>
    /// Reads the member value from a target object.
    /// </summary>
    /// <param name="target">Target object owning the member.</param>
    /// <returns>Current member value.</returns>
    public object? GetValue(object target) => VisualHandler.GetMemberValue(Member, target);
}
#endif
