using Godot;
using System.Collections.Generic;

namespace __TEMPLATE__.UI;

public class OptionsNav
{
    public Button GeneralButton => _generalButton;
    public Button GameplayButton => _gameplayButton;
    public Button DisplayButton => _displayButton;
    public Button GraphicsButton => _graphicsButton;
    public Button AudioButton => _audioButton;
    public Button InputButton => _inputButton;

    private readonly Dictionary<string, Control> _tabs = [];
    private readonly Dictionary<string, Button> _buttons = [];
    private readonly Options options;

    private Button _generalButton;
    private Button _gameplayButton;
    private Button _displayButton;
    private Button _graphicsButton;
    private Button _audioButton;
    private Button _inputButton;

    public OptionsNav(Options options, Label titleLabel)
    {
        this.options = options;

        SetupContent();
        SubscribeToNavBtns(titleLabel);
        FocusOnLastClickedNavBtn();
        HideAllTabs();
        ShowCurrentTab(titleLabel);
    }

    private void SetupContent()
    {
        Node content = options.GetNode("%Content");

        foreach (Control child in content.GetChildren())
        {
            _tabs.Add(child.Name, child);
        }
    }

    private void SubscribeToNavBtns(Label titleLabel)
    {
        foreach (Button button in options.GetNode("%Nav").GetChildren())
        {
            string btnName = button.Name;
            button.FocusEntered += () => ShowTab(titleLabel, btnName);
            button.Pressed += () => ShowTab(titleLabel, btnName);
            
            _buttons.Add(btnName, button);
        }

        _generalButton = _buttons["General"];
        _gameplayButton = _buttons["Gameplay"];
        _displayButton = _buttons["Display"];
        _graphicsButton = _buttons["Graphics"];
        _audioButton = _buttons["Audio"];
        _inputButton = _buttons["Input"];
    }

    private void FocusOnLastClickedNavBtn()
    {
        _buttons[Game.Options.GetCurrentTab()].GrabFocus();
    }

    private void ShowCurrentTab(Label titleLabel)
    {
        ShowTab(titleLabel, Game.Options.GetCurrentTab());
    }

    private void ShowTab(Label titleLabel, string tabName)
    {
        titleLabel.Text = tabName;
        Game.Options.SetCurrentTab(tabName);
        HideAllTabs();
        _tabs[tabName].Show();
    }

    private void HideAllTabs()
    {
        foreach (Control tab in _tabs.Values)
        {
            tab.Hide();
        }
    }
}
