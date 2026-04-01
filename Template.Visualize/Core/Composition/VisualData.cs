#if DEBUG
using Godot;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Aggregates anchor/object/member metadata required to build visualization UI.
/// </summary>
/// <param name="anchorNode">Scene node used as the visualization anchor.</param>
/// <param name="visualizedObject">Object instance whose members are visualized.</param>
/// <param name="properties">Annotated properties to expose in the panel.</param>
/// <param name="fields">Annotated fields to expose in the panel.</param>
/// <param name="methods">Annotated methods to expose as invokable actions.</param>
internal sealed class VisualData(Node anchorNode, object visualizedObject, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<FieldInfo> fields, IReadOnlyList<MethodInfo> methods)
{
    /// <summary>
    /// Scene node used as the anchor for positioning visual UI.
    /// </summary>
    public Node AnchorNode { get; } = anchorNode;

    /// <summary>
    /// Object instance whose members and methods are visualized.
    /// </summary>
    public object VisualizedObject { get; } = visualizedObject;

    /// <summary>
    /// Annotated properties exposed in the visualization panel.
    /// </summary>
    public IReadOnlyList<PropertyInfo> Properties { get; } = properties;

    /// <summary>
    /// Annotated fields exposed in the visualization panel.
    /// </summary>
    public IReadOnlyList<FieldInfo> Fields { get; } = fields;

    /// <summary>
    /// Annotated methods exposed as invokable actions in the visualization panel.
    /// </summary>
    public IReadOnlyList<MethodInfo> Methods { get; } = methods;
}
#endif
