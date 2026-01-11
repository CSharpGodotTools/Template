using Godot;

namespace __TEMPLATE__.UI;

public partial class Options : PanelContainer
{
    private OptionsNav _optionsNav;
    private OptionsGeneral _optionsGeneral;
    private OptionsGameplay _optionsGameplay;
    private OptionsDisplay _optionsDisplay;
    private OptionsGraphics _optionsGraphics;
    private OptionsAudio _optionsAudio;
    private OptionsInput _optionsInput;

    public override void _Ready()
    {
        _optionsNav = new OptionsNav(this, GetNode<Label>("%Title"));
        _optionsGeneral = new OptionsGeneral(this, _optionsNav.GeneralButton);
        _optionsGameplay = new OptionsGameplay(this, _optionsNav.GameplayButton);
        _optionsDisplay = new OptionsDisplay(this, _optionsNav.DisplayButton);
        _optionsGraphics = new OptionsGraphics(this, _optionsNav.GraphicsButton);
        _optionsAudio = new OptionsAudio(this);
        _optionsInput = new OptionsInput(this, _optionsNav.InputButton);

        VisibilityChanged += OnVisibilityChanged;

        Game.Scene.PostSceneChanged += OnPostSceneChanged;
    }

    public override void _Input(InputEvent @event)
    {
        _optionsInput.HandleInput(@event);
    }

    public override void _ExitTree()
    {
        _optionsGeneral.Dispose();
        Game.Scene.PostSceneChanged -= OnPostSceneChanged;
        VisibilityChanged -= OnVisibilityChanged;
    }

    private void OnPostSceneChanged()
    {
        if (Visible)
            Game.FocusOutline.Focus(GetNode("%Nav").GetChild<Button>(0));
    }

    private void OnVisibilityChanged()
    {
        if (Visible)
            GetNode("%Nav").GetChild<Button>(0).GrabFocus();
    }
}
