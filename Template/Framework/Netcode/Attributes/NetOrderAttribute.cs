using System;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Optional explicit order used by legacy reflection-based serialization fallback.
/// Members without this attribute retain metadata-token ordering.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class NetOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
