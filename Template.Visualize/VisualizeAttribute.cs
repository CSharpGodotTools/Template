#if DEBUG
using System;

namespace GodotUtils.Debugging;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
public sealed class VisualizeAttribute : Attribute
{
}
#endif
