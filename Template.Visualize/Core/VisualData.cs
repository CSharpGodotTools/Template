#if DEBUG
using Godot;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Represents a node to be visualized
/// </summary>
internal sealed class VisualData(Node anchorNode, object visualizedObject, IEnumerable<PropertyInfo> properties, IEnumerable<FieldInfo> fields, IEnumerable<MethodInfo> methods)
{
    public Node AnchorNode { get; } = anchorNode;
    public object VisualizedObject { get; } = visualizedObject;
    public IEnumerable<PropertyInfo> Properties { get; } = properties;
    public IEnumerable<FieldInfo> Fields { get; } = fields;
    public IEnumerable<MethodInfo> Methods { get; } = methods;
}
#endif
