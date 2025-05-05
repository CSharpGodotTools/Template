using System;

namespace Template.Inventory;

public class InventoryActionFactory
{
    public InventoryActionBase GetAction(InventoryAction actionType)
    {
        return actionType switch
        {
            InventoryAction.DoubleClick => new InventoryActionPickupAll(),
            InventoryAction.Transfer => new InventoryActionTransfer(),
            InventoryAction.Pickup => new InventoryActionPickup(),
            InventoryAction.Place => new InventoryActionPlace(),
            InventoryAction.Stack => new InventoryActionStack(),
            InventoryAction.Swap => new InventoryActionSwap(),
            _ => throw new NotImplementedException($"No action defined for {actionType}")
        };
    }
}

public enum InventoryAction
{
    Pickup,
    Place,
    Stack,
    Swap,
    Transfer,
    DoubleClick
}
