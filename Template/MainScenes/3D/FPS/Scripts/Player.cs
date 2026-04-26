using Godot;
using GodotUtils;
using PopupMenu = __TEMPLATE__.Ui.PopupMenu;

namespace __TEMPLATE__.FPS;

public partial class Player : CharacterBody3D
{
    [Export] private Camera3D _camera = null!;
    [Export] private PopupMenu _popupMenu = null!;
    [Export] private PlayerMovementConfig _movementConfig = null!;

    private readonly ComponentList _components = new();

    public override void _Ready()
    {
        IOptionsService optionsService = Game.Options;
        FpsOptions.Register(optionsService);

        PlayerRotation rotationComponent = new(this, _camera, optionsService);

        _components.Add(rotationComponent);
        _components.Add(new PlayerMovement(this, _movementConfig, rotationComponent));
        _components.Add(new PlayerMouseCapture(this, _popupMenu));
    }
}
