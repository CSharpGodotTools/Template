using Framework;
using Godot;
using GodotUtils;
using System;
using PopupMenu = Framework.UI.PopupMenu;

namespace __TEMPLATE__.FPS;

public partial class Player : CharacterBody3D
{
    [Export] private Camera3D _camera;
    [Export] private PopupMenu _popupMenu;
    [Export] private PlayerMovementConfig _movementConfig;

    private ComponentHost _components = new();

    public override void _Ready()
    {
        _components.Add(new PlayerMovement(this, _movementConfig, _camera));
        _components.Add(new PlayerMouseCapture(this, _popupMenu));
    }
}
