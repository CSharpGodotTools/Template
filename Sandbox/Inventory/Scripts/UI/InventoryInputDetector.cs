using Godot;
using GodotUtils;

namespace Template.Inventory;

public class InventoryInputDetector
{
    public bool HoldingRightClick { get; private set; }
    public bool HoldingLeftClick { get; private set; }
    public bool HoldingShift { get; private set; }

    public void Update(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseBtn)
        {
            HoldingRightClick = mouseBtn.IsRightClickPressed();
            HoldingLeftClick = mouseBtn.IsLeftClickPressed();
        }
        else if (@event is InputEventKey key)
        {
            HoldingShift = key.Keycode == Key.Shift && key.Pressed;
        }
    }
}
