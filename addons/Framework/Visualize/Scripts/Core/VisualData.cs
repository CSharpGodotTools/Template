using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace __TEMPLATE__;

/// <summary>
/// Represents a node to be visualized
/// </summary>
public class VisualData(Node node, Vector2 initialPosition, string[] readonlyMembers, IEnumerable<PropertyInfo> properties, IEnumerable<FieldInfo> fields, IEnumerable<MethodInfo> methods)
{
    public Node Node { get; } = node;
    public string[] ReadonlyMembers { get; } = readonlyMembers;
    public Vector2 InitialPosition { get; } = initialPosition;
    public IEnumerable<PropertyInfo> Properties { get; } = properties;
    public IEnumerable<FieldInfo> Fields { get; } = fields;
    public IEnumerable<MethodInfo> Methods { get; } = methods;
}

public class VisualSpinBox
{
    public SpinBox SpinBox { get; set; }
    public Type Type { get; set; }
}
