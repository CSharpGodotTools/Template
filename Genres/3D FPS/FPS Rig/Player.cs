using Godot;
using System;

namespace __TEMPLATE__.FPS3D;

public partial class Player : CharacterBody3D
{
    private event Action OnFinishedReload;

    public override void _Ready()
    {
        OnReadyUI();
        OnReadyAnimation();
    }

    public override void _PhysicsProcess(double delta)
    {
        OnPhysicsProcessUI();
        OnPhysicsProcessMotion(delta);
        OnPhysicsProcessAnimation();
    }

    public override void _Input(InputEvent @event)
    {
        OnInputUI(@event);
    }
}
