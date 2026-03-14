using System;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Excludes a property from legacy reflection-based packet serialization fallback.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NetExcludeAttribute : Attribute
{
}
