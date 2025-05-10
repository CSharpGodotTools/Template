using Godot;
using System;

namespace Template;

public interface IDraggable
{
    Vector2 Position { get; set; }

    void AnimateDrop(Action finished, double duration);
    void FollowCursor();
    void Reparent(Node newParent);
    void SetCollisionActive(bool active);
}