using Godot;

namespace Template.Inventory;

[SceneTree]
public partial class InventoryContainer : PanelContainer
{
    public Inventory Inventory { get; private set; }
    public ItemContainer[] ItemContainers { get; private set; }

    private InventoryInputDetector _inputDetector = new();
    private InventoryInputHandler _inputHandler;
    private CanvasLayer _ui;
    private int _columns;

    [OnInstantiate]
    private void Init(Inventory inventory, int columns = 10)
    {
        GridContainer.Columns = columns;
        Inventory = inventory;
        _columns = columns;
    }

    public override void _Ready()
    {
        _ui = GetTree().CurrentScene.GetNode<CanvasLayer>("%UI");

        AddItemContainers(Inventory);
    }

    public override void _PhysicsProcess(double delta)
    {
        _inputHandler.Update();
    }

    public override void _Input(InputEvent @event)
    {
        _inputDetector.Update(@event);
    }

    public int GetHotbarSlot(int index)
    {
        int totalSlotCount = Inventory.GetSlotCount();
        int hotbarIndex = totalSlotCount - _columns + index;

        return hotbarIndex;
    }

    private void AddItemContainers(Inventory inventory)
    {
        ItemContainers = new ItemContainer[inventory.GetItemSlotCount()];

        InventoryContext invContext = new(this, _inputDetector, _ui, ItemContainers, inventory);
        InventoryVFXManager vfxManager = new();
        _inputHandler = new(_columns, invContext);

        vfxManager.RegisterEvents(_inputHandler, invContext, this);
        _inputHandler.RegisterInput();

        for (int i = 0; i < ItemContainers.Length; i++)
        {
            ItemContainer itemContainer = AddItemContainer();
            itemContainer.SetItem(inventory.GetItem(i));
            ItemContainers[i] = itemContainer;

            int index = i; // Capture i

            itemContainer.GuiInput += @event =>
            {
                _inputHandler.HandleGuiInput(@event, index);
            };

            itemContainer.MouseEntered += () =>
            {
                _inputHandler.HandleMouseEntered(vfxManager, index, GetGlobalMousePosition());
            };

            itemContainer.MouseExited += () =>
            {
                _inputHandler.HandleMouseExited();
            };
        }

        inventory.OnItemChanged += (index, item) =>
        {
            ItemContainers[index].SetItem(item);
        };
    }

    private ItemContainer AddItemContainer()
    {
        ItemContainer itemContainer = ItemContainer.Instantiate();
        GridContainer.AddChild(itemContainer);
        return itemContainer;
    }
}
