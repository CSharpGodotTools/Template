namespace Template.Inventory;

public abstract class InventoryAnimationBase
{
    protected InventoryContext _context;
    protected InventoryContainer _container;
    protected AnimHelperItemContainer vfxContainer = null;

    protected int itemFrame = 0;
    protected int cursorFrame = 0;

    public void Initialise(InventoryContext context, InventoryContainer container)
    {
        _context = context;
        _container = container;
    }

    public abstract void OnPreAnimate(InventoryActionEventArgs args);
    public abstract void OnPostAnimate(InventoryActionEventArgs args);
}
