using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils;

/// <summary>
/// Helpers for formatted printing.
/// </summary>
public static class PrintExtensions
{
    /// <summary>
    /// Prints a collection using a formatted string.
    /// </summary>
    /// <typeparam name="T">Element type of the collection.</typeparam>
    /// <param name="value">Collection to print.</param>
    /// <param name="newLine">Whether items should be formatted on separate lines.</param>
    public static void PrintFormatted<T>(this IEnumerable<T> value, bool newLine = true)
    {
        GD.Print(value.ToFormattedString(newLine));
    }

    /// <summary>
    /// Prints an object as formatted JSON.
    /// </summary>
    /// <param name="v">Object to serialize and print.</param>
    public static void PrintFormatted(this object v)
    {
        GD.Print(v.ToFormattedString());
    }

    /// <summary>
    /// Converts a collection to a formatted string.
    /// </summary>
    /// <typeparam name="T">Element type of the collection.</typeparam>
    /// <param name="value">Collection to format.</param>
    /// <param name="newLine">Whether items should be formatted on separate lines.</param>
    /// <returns>Formatted collection string, or <see langword="null"/> when input is null.</returns>
    public static string? ToFormattedString<T>(this IEnumerable<T> value, bool newLine = true)
    {
        // Null collections produce null output.
        if (value == null)
            return null;

        // Choose multiline or single-line format based on caller preference.
        if (newLine)
        {
            return "[\n    " + string.Join(",\n    ", value) + "\n]";
        }
        else
        {
            return "[" + string.Join(", ", value) + "]";
        }
    }

    /// <summary>
    /// Converts an object to formatted JSON.
    /// </summary>
    /// <param name="v">Object to serialize.</param>
    /// <returns>JSON string formatted with indentation.</returns>
    public static string ToFormattedString(this object v)
    {
        string json = JsonConvert.SerializeObject(v, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new IgnorePropsResolver()
        });

        // Normalize line endings to avoid double-spacing in Godot logs
        return json.Replace("\r\n", "\n");
    }

    /// <summary>
    /// A custom contract resolver used to ignore certain Godot properties during JSON serialization.
    /// </summary>
    private class IgnorePropsResolver : DefaultContractResolver
    {
        /// <summary>
        /// Creates a JsonProperty for the given MemberInfo.
        /// </summary>
        /// <param name="member">The MemberInfo to create a JsonProperty for.</param>
        /// <param name="memberSerialization">The MemberSerialization mode for the member.</param>
        /// <returns>A JsonProperty for the given MemberInfo.</returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop =
                base.CreateProperty(member, memberSerialization);

            // Ignored properties (prevents crashes)
            Type[] ignoredProps =
            [
                typeof(GodotObject),
                typeof(Node),
                typeof(NodePath),
                typeof(ENet.Packet)
            ];

            foreach (Type ignoredProp in ignoredProps)
            {
                // Ignore members declared on Godot engine types.
                if (member.DeclaringType != null &&
                    (member.DeclaringType == ignoredProp || member.DeclaringType.IsSubclassOf(ignoredProp)))
                {
                    prop.Ignored = true;
                }

                // Ignore properties whose type is a Godot engine type.
                if (prop.PropertyType != null && (prop.PropertyType == ignoredProp || prop.PropertyType.IsSubclassOf(ignoredProp)))
                    prop.Ignored = true;
            }

            return prop;
        }
    }
}
