using System;

namespace Framework.Netcode;

/// <summary>
/// Excludes a property from legacy reflection-based packet serialization fallback.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NetExcludeAttribute : Attribute
{
}
