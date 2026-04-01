using System;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Optional explicit order used by legacy reflection-based serialization fallback.
/// Members without this attribute retain metadata-token ordering.
/// </summary>
/// <param name="order">Explicit serialization order value for reflection fallback.</param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class NetOrderAttribute(int order) : Attribute
{
    /// <summary>
    /// Gets the explicit order value used during reflection fallback serialization.
    /// </summary>
    public int Order { get; } = order;
}
