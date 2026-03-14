#if DEBUG
using System;
using System.Reflection;

namespace GodotUtils.Debugging;

internal sealed class ReadonlyMemberAccessor(string name, MemberInfo member, Type memberType)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public MemberInfo Member { get; } = member ?? throw new ArgumentNullException(nameof(member));
    public Type MemberType { get; } = memberType ?? throw new ArgumentNullException(nameof(memberType));

    public object? GetValue(object target) => VisualHandler.GetMemberValue(Member, target);
}
#endif
