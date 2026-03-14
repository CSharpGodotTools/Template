#if DEBUG
using Godot;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Represents a node to be visualized
/// </summary>
internal sealed class VisualData(Node anchorNode, object visualizedObject, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<FieldInfo> fields, IReadOnlyList<MethodInfo> methods)
{
    public Node AnchorNode { get; } = anchorNode;
    public object VisualizedObject { get; } = visualizedObject;
    public IReadOnlyList<PropertyInfo> Properties { get; } = properties;
    public IReadOnlyList<FieldInfo> Fields { get; } = fields;
    public IReadOnlyList<MethodInfo> Methods { get; } = methods;
}
#endif
