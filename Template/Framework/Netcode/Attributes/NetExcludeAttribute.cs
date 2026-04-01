using System;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Excludes a property from legacy reflection-based packet serialization fallback.
/// </summary>
/// <remarks>
/// This attribute only affects reflection fallback paths and does not change generated serializer behavior.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class NetExcludeAttribute : Attribute
{
}
