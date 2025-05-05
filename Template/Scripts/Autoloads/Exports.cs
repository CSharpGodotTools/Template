using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Template;

public partial class Exports : Node
{
#if DEBUG
    public override void _Ready()
    {
        List<Node> nodes = GetTree().Root.GetChildren<Node>();

        foreach (Node node in nodes)
        {
            Type type = node.GetType();

            CheckFields(type, node);
            CheckProperties(type, node);
        }
    }

    private void CheckFields(Type type, Node node)
    {
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (field.GetCustomAttribute<ExportAttribute>() != null)
            {
                object value = field.GetValue(node);

                if (value == null)
                {
                    node.SetPhysicsProcess(false);
                    node.SetProcess(false);

                    string visibility = field.IsPublic ? "public" : "private";
                    LogError(visibility, field.FieldType.Name, field.Name, type.Name);
                }
            }
        }
    }

    private void CheckProperties(Type type, Node node)
    {
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (property.GetCustomAttribute<ExportAttribute>() != null && property.CanRead && property.CanWrite)
            {
                object value = property.GetValue(node);

                if (value == null)
                {
                    node.SetPhysicsProcess(false);
                    node.SetProcess(false);

                    string visibility = property.GetMethod.IsPublic ? "public" : "private";
                    LogError(visibility, property.PropertyType.Name, property.Name, type.Name);
                }
            }
        }
    }

    private void LogError(string visibilityModifier, string memberTypeName, string memberName, string typeName)
    {
        _pushWarningMessage = $"[Export] {visibilityModifier} {memberTypeName} {memberName}; in {typeName}.cs is null and needs to be set";
        ThrowIfNull();
    }

    private string _pushWarningMessage;

    /// <summary>
    /// GD.PushError() displays function params so we created a function with no params
    /// </summary>
    private void ThrowIfNull()
    {
        // Example of message shown:
        // void Template.Exports.ThrowIfNull(): [Export] private Node _test; in Player.cs is null and needs to be set
        GD.PushError(_pushWarningMessage);
    }
#endif
}
