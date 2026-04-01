#if DEBUG
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Marks members that should be exposed in the runtime visualization panel.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
public sealed class VisualizeAttribute : Attribute
{
}
#endif
