using Godot;
using System;

namespace Template;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
public class VisualizeAttribute : Attribute
{
    public Vector2 InitialPosition { get; }
    public string[] VisualizeMembers { get; }

    public VisualizeAttribute()
    {
        InitialPosition = Vector2.Zero;
    }

    public VisualizeAttribute(params string[] visualizeMembers)
    {
        InitialPosition = Vector2.Zero;
        VisualizeMembers = visualizeMembers;
    }

    public VisualizeAttribute(float x, float y)
    {
        InitialPosition = new Vector2(x, y);
    }
}

