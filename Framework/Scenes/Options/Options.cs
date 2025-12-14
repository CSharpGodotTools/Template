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
        _optionsNav = new OptionsNav(this);
        _optionsGeneral = new OptionsGeneral(this, _optionsNav.GeneralButton);
        _optionsGameplay = new OptionsGameplay(this, _optionsNav.GameplayButton);
        _optionsDisplay = new OptionsDisplay(this, _optionsNav.DisplayButton);
        _optionsGraphics = new OptionsGraphics(this, _optionsNav.GraphicsButton);
        _optionsAudio = new OptionsAudio(this);
        _optionsInput = new OptionsInput(this, _optionsNav.InputButton);
    }

    public override void _Input(InputEvent @event)
    {
        _optionsInput.HandleInput(@event);
    }
}
