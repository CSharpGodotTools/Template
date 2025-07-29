using Godot;
using __TEMPLATE__.UI;

namespace __TEMPLATE__.FPS3D;

public partial class SubView : SubViewportContainer
{
    public override void _Ready()
    {
        OptionsManager optionsManager = GetNode<OptionsManager>(Autoloads.OptionsManager);
        StretchShrink = optionsManager.Options.Resolution;

        SubViewport subViewport = GetNode<SubViewport>("SubViewport");
        subViewport.Msaa3D = (Viewport.Msaa)optionsManager.Options.Antialiasing;

        UIPopupMenu popupMenu = GetNode<UIPopupMenu>("%PopupMenu");

        OptionsDisplay display = popupMenu
            .Options.GetNode<OptionsDisplay>("%Display");

        display.OnResolutionChanged += _ =>
        {
            StretchShrink = optionsManager.Options.Resolution;
        };

        OptionsGraphics graphics = popupMenu.Options.GetNode<OptionsGraphics>("%Graphics");

        graphics.OnAntialiasingChanged += (aa) =>
        {
            subViewport.Msaa3D = (Viewport.Msaa)aa;
        };
    }
}
