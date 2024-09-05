using CSharpUtils;
using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Template;

public abstract partial class RigidBody : RigidBody2D, IBaseEntity
{
    public EntityComponent EntityComponent { get; set; }

    public override void _Ready()
    {
        EntityComponent = this.GetNode<EntityComponent>();
        EntityComponent.SwitchState(Idle());
    }

    public virtual void IdleState(State state) { }

    protected State Idle()
    {
        State state = new(nameof(Idle));

        IdleState(state);

        return state;
    }
}

