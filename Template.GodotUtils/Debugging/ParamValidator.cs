using Godot;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace GodotUtils.Debugging;

/// <summary>
/// Validation helpers for runtime parameters.
/// </summary>
public static class ParamValidator
{
    /// <summary>
    /// Throws when <paramref name="obj"/> is null and disables the node.
    /// </summary>
    public static void ThrowIfNull(Node node, object obj, [CallerArgumentExpression(nameof(obj))] string paramName = "")
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        if (obj == null)
        {
            Script script = node.GetScript().As<Script>();
            string scriptName = script == null ? node.Name : Path.GetFileName(script.ResourcePath);

            node.ProcessMode = Node.ProcessModeEnum.Disabled;

            throw new Exception($"Value cannot be null. (Parameter '{paramName}' in {scriptName})");
        }
    }
}
