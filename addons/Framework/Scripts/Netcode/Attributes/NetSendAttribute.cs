using System;

namespace __TEMPLATE__.Netcode;

[AttributeUsage(AttributeTargets.Property)]
public class NetSendAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}

