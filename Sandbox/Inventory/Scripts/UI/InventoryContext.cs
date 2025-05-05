using Godot;

namespace Template.Inventory;

public class InventoryContext(InventoryContainer inventoryContainer, InventoryInputDetector inputDetector, CanvasLayer ui, ItemContainer[] itemContainers, Inventory inventory)
{
    public InventoryContainer InventoryContainer { get; } = inventoryContainer;
    public InventoryInputDetector InputDetector { get; } = inputDetector;
    public CanvasLayer UI { get; } = ui;
    public InventoryVFX VFX { get; } = new();
    public ItemContainer[] ItemContainers { get; } = itemContainers;
    public CursorItemContainer CursorItemContainer { get; } = Services.Get<CursorItemContainer>();
    public Inventory Inventory { get; } = inventory;
    public Inventory CursorInventory { get; } = Services.Get<CursorItemContainer>().Inventory;
}
