using System;

namespace Template;

[AttributeUsage(AttributeTargets.Class)]
public class DraggableAttribute(DragClick dragClick = DragClick.Left, DragType dragType = DragType.Hold, DragConstraints dragConstraints = DragConstraints.None) : Attribute
{
    public DragClick DragClick { get; } = dragClick;
	public DragType DragType { get; } = dragType;
    public DragConstraints DragConstraints { get; } = dragConstraints;
}

public enum DragClick
{
    Left,
    Right,
    Both
}

public enum DragType
{
    Hold,
    Click
}

public enum DragConstraints
{
    None,
    Horizontal,
    Vertical
}
