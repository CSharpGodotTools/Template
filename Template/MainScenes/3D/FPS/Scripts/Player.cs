using __TEMPLATE__;
using Godot;
using GodotUtils;
using System;
using PopupMenu = __TEMPLATE__.Ui.PopupMenu;

namespace __TEMPLATE__.FPS;

public partial class Player : CharacterBody3D, ISceneDependencyReceiver
{
    [Export] private Camera3D _camera = null!;
    [Export] private PopupMenu _popupMenu = null!;
    [Export] private PlayerMovementConfig _movementConfig = null!;

    private readonly ComponentList _components = new();
    private IOptionsService _optionsService = null!;
    private bool _isConfigured;

    public void Configure(GameServices services)
    {
        _optionsService = services.Options;
        _isConfigured = true;
    }

    public override void _Ready()
    {
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(Player)} was not configured before _Ready.");

        FpsOptions.Register(_optionsService);

        PlayerRotation rotationComponent = new(this, _camera, _optionsService);

        _components.Add(rotationComponent);
        _components.Add(new PlayerMovement(this, _movementConfig, rotationComponent));
        _components.Add(new PlayerMouseCapture(this, _popupMenu));
    }
}
