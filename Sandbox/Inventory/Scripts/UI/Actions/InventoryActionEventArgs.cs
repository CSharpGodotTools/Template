using System;

namespace Template.Inventory;

public class InventoryActionEventArgs(InventoryAction action) : EventArgs
{
    public InventoryAction Action { get; } = action;

    public InventoryContainer TargetInventoryContainer { get; set; }

    // Specific to Transfer Actions
    public int FromIndex { get; set; }
    public int ToIndex { get; set; }
    public bool AreSameType { get; set; }
}
