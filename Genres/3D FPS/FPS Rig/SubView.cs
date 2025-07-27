using Godot;
using __TEMPLATE__.UI;

namespace __TEMPLATE__.FPS3D;

public partial class SubView : SubViewportContainer
{
    [Export] private OptionsManager _options;

    public override void _Ready()
    {
        StretchShrink = _options.Options.Resolution;

        SubViewport subViewport = GetNode<SubViewport>("SubViewport");
        subViewport.Msaa3D = (Viewport.Msaa)_options.Options.Antialiasing;

        UIPopupMenu popupMenu = GetNode<UIPopupMenu>("%PopupMenu");

        OptionsDisplay display = popupMenu
            .Options.GetNode<OptionsDisplay>("%Display");

        display.OnResolutionChanged += _ =>
        {
            StretchShrink = _options.Options.Resolution;
        };

        OptionsGraphics graphics = popupMenu.Options.GetNode<OptionsGraphics>("%Graphics");

        graphics.OnAntialiasingChanged += (aa) =>
        {
            subViewport.Msaa3D = (Viewport.Msaa)aa;
        };
    }
}

