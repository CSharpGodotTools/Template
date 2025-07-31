using Godot;
using __TEMPLATE__.UI;

namespace __TEMPLATE__.FPS3D;

public partial class LevelUI : Node
{
    private UIPopupMenu _popupMenu;

    public override void _Ready()
    {
        _popupMenu = GetNode<UIPopupMenu>("%PopupMenu");
        _popupMenu.OnOpened += () =>
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        };
        _popupMenu.OnClosed += () =>
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        };

        Input.MouseMode = Input.MouseModeEnum.Captured;

        UIConsole console = GetNode<UIConsole>(AutoloadPaths.Console);

        console.VisibilityToggled += HandleConsoleToggled;

        _popupMenu.OnMainMenuBtnPressed += () =>
        {
            // No longer need to listen for this
            console.VisibilityToggled -= HandleConsoleToggled;
        };
    }

    private void HandleConsoleToggled(bool visible)
    {
        SetPhysicsProcess(!visible);
        SetProcessInput(!visible);

        if (visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else
        {
            if (!_popupMenu.Visible)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }
}
