using Framework.UI;
using Godot;
using GodotUtils;
using PopupMenu = Framework.UI.PopupMenu;

namespace __TEMPLATE__.FPS;

public class PlayerMouseCapture(Player player, PopupMenu popupMenu) : Component(player)
{
    protected override void Ready()
    {
        CaptureCursor();

        popupMenu.Opened += OnPopupMenuOpened;
        popupMenu.Closed += OnPopupMenuClosed;
    }

    protected override void OnDispose()
    {
        popupMenu.Opened -= OnPopupMenuOpened;
        popupMenu.Closed -= OnPopupMenuClosed;
    }

    private void OnPopupMenuClosed()
    {
        CaptureCursor();
    }

    private void OnPopupMenuOpened()
    {
        ShowCursor();
    }

    private void CaptureCursor() => Input.MouseMode = Input.MouseModeEnum.Captured;
    private void ShowCursor() => Input.MouseMode = Input.MouseModeEnum.Visible;
}
