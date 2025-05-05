using System;

namespace Template.Netcode;

[AttributeUsage(AttributeTargets.Property)]
public class NetSendAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}

