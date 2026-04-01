using Godot;
using GodotUtils;
using PopupMenu = __TEMPLATE__.Ui.PopupMenu;

namespace __TEMPLATE__.FPS;

public class PlayerMouseCapture(Player player, PopupMenu popupMenu) : Component(player)
{
    protected override void Ready()
    {
        CaptureCursor();

        popupMenu.Opened += OnPopupMenuOpened;
        popupMenu.Closed += OnPopupMenuClosed;
    }

    protected override void ExitTree()
    {
        popupMenu.Opened -= OnPopupMenuOpened;
        popupMenu.Closed -= OnPopupMenuClosed;
    }

    /// <summary>
    /// Restores captured cursor mode when the popup menu closes.
    /// </summary>
    private void OnPopupMenuClosed()
    {
        CaptureCursor();
    }

    /// <summary>
    /// Releases the cursor when the popup menu opens.
    /// </summary>
    private void OnPopupMenuOpened()
    {
        ShowCursor();
    }

    /// <summary>
    /// Sets mouse mode to captured for gameplay camera control.
    /// </summary>
    private static void CaptureCursor() => Input.MouseMode = Input.MouseModeEnum.Captured;

    /// <summary>
    /// Sets mouse mode to visible for menu interaction.
    /// </summary>
    private static void ShowCursor() => Input.MouseMode = Input.MouseModeEnum.Visible;
}
