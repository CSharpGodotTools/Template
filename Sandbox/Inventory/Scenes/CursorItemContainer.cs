using Godot;

namespace Template.Inventory;

[SceneTree]
[Service]
public partial class CursorItemContainer : ItemContainer
{
    public Inventory Inventory { get; private set; }

    private Vector2 _offset;

    public override void _Ready()
    {
        Inventory = new(1);
        Inventory.OnItemChanged += (index, item) => SetItem(item);

        IgnoreInputEvents(this);

        _offset = CustomMinimumSize * 0.5f;

        Show();
    }

    public override void _PhysicsProcess(double delta)
    {
        Position = GetGlobalMousePosition() - _offset;
    }

    private static void IgnoreInputEvents(Control control)
    {
        control.MouseFilter = MouseFilterEnum.Ignore;
        control.SetProcessInput(false);
    }
}
