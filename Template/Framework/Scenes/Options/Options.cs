using Godot;
using GodotUtils;

namespace Framework.UI;

public partial class Options : PanelContainer
{
    // Fields
    private OptionsNav _optionsNav;
    private OptionsGeneral _optionsGeneral;
    private OptionsGameplay _optionsGameplay;
    private OptionsDisplay _optionsDisplay;
    private OptionsGraphics _optionsGraphics;
    private OptionsAudio _optionsAudio;
    private OptionsInput _optionsInput;
    private Node _navNode;

    // Godot Overrides
    public override void _Ready()
    {
        _navNode = GetNode("%Nav");
        _optionsNav = new OptionsNav(this, GetNode<Label>("%Title"));
        _optionsGeneral = new OptionsGeneral(this, _optionsNav.GeneralButton);
        _optionsGameplay = new OptionsGameplay(this, _optionsNav.GameplayButton);
        _optionsDisplay = new OptionsDisplay(this, _optionsNav.DisplayButton);
        _optionsGraphics = new OptionsGraphics(this, _optionsNav.GraphicsButton);
        _optionsAudio = new OptionsAudio(this);
        _optionsInput = new OptionsInput(this, _optionsNav.InputButton);

        VisibilityChanged += OnVisibilityChanged;

        GameFramework.Scene.PostSceneChanged += OnPostSceneChanged;
    }

    public override void _Input(InputEvent @event)
    {
        _optionsInput.HandleInput(@event);
    }

    public override void _ExitTree()
    {
        _optionsNav.Dispose();
        _optionsGeneral.Dispose();
        _optionsGameplay.Dispose();
        _optionsDisplay.Dispose();
        _optionsGraphics.Dispose();
        _optionsAudio.Dispose();
        _optionsInput.Dispose();

        GameFramework.Scene.PostSceneChanged -= OnPostSceneChanged;
        VisibilityChanged -= OnVisibilityChanged;
    }

    // Subscribers
    private void OnPostSceneChanged()
    {
        if (Visible)
        {
            GameFramework.FocusOutline.Focus(_navNode.GetNode<Button>(GameFramework.Options.GetCurrentTab()));
        }
    }

    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            _navNode.GetNode<Button>(GameFramework.Options.GetCurrentTab()).GrabFocus();
        }
    }
}
