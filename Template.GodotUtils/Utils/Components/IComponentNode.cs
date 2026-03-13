using Godot;
using System;

namespace GodotUtils;

public interface IComponentNode
{
    event Action Ready;
    event Action TreeExited;

    SceneTree GetTree();
}
