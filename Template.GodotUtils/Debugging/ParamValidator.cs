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
    /// Validates an object argument and disables the node before throwing when the value is null.
    /// </summary>
    /// <param name="node">Node to disable when validation fails.</param>
    /// <param name="obj">Argument value to validate.</param>
    /// <param name="paramName">Name of the validated argument.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null.</exception>
    public static void ThrowIfNull(Node node, object obj, [CallerArgumentExpression(nameof(obj))] string paramName = "")
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        // Disable processing before throwing so invalid nodes stop running immediately.
        if (obj == null)
        {
            Script script = node.GetScript().As<Script>();
            string scriptName = script == null ? node.Name : Path.GetFileName(script.ResourcePath);

            node.ProcessMode = Node.ProcessModeEnum.Disabled;

            throw new ArgumentNullException(paramName, $"Value cannot be null. (In {scriptName})");
        }
    }
}
