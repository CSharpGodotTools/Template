using Godot;

namespace GodotUtils.UI;

public partial class Options : PanelContainer
{
    private OptionsNav _optionsNav;
    private OptionsGeneral _optionsGeneral;
    private OptionsGameplay _optionsGameplay;
    private OptionsDisplay _optionsDisplay;
    private OptionsGraphics _optionsGraphics;
    private OptionsAudio _optionsAudio;
    private OptionsInput _optionsInput;

    public override void _EnterTree()
    {
        _optionsNav = new OptionsNav(this);
        _optionsGeneral = new OptionsGeneral(this);
        _optionsGameplay = new OptionsGameplay(this);
        _optionsDisplay = new OptionsDisplay(this);
        _optionsGraphics = new OptionsGraphics(this);
        _optionsAudio = new OptionsAudio(this);
        _optionsInput = new OptionsInput(this);
    }

    public override void _Ready()
    {
        _optionsNav.Initialize();
        _optionsGeneral.Initialize();
        _optionsGameplay.Initialize();
        _optionsDisplay.Initialize();
        _optionsGraphics.Initialize();
        _optionsAudio.Initialize();
        _optionsInput.Initialize();
    }

    public override void _Input(InputEvent @event)
    {
        _optionsInput.HandleInput(@event);
    }
}
