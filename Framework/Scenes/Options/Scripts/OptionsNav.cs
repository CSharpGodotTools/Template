using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.UI;

public class OptionsNav : IDisposable
{
    #region Nodes
    public Button GeneralButton => _generalButton;
    public Button GameplayButton => _gameplayButton;
    public Button DisplayButton => _displayButton;
    public Button GraphicsButton => _graphicsButton;
    public Button AudioButton => _audioButton;
    public Button InputButton => _inputButton;

    private Button _generalButton;
    private Button _gameplayButton;
    private Button _displayButton;
    private Button _graphicsButton;
    private Button _audioButton;
    private Button _inputButton;
    #endregion

    #region Fields
    private readonly Godot.Collections.Array<Node> _navBtns;
    private readonly Dictionary<string, Control> _tabs = [];
    private readonly Dictionary<string, Button> _buttons = [];
    private readonly Dictionary<Button, Action> _focusEnteredHandlers = [];
    private readonly Dictionary<Button, Action> _pressedHandlers = [];
    private readonly Options _options;
    private readonly Label _titleLabel;
    #endregion

    public OptionsNav(Options options, Label titleLabel)
    {
        _options = options;
        _titleLabel = titleLabel;
        _navBtns = options.GetNode("%Nav").GetChildren();

        SetupContent();
        SubscribeToNavBtns(_titleLabel);
        SetButtonFields();
        FocusOnLastClickedNavBtn();
        HideAllTabs();
        ShowCurrentTab(_titleLabel);
    }
    
    #region Private Methods
    private void SetupContent()
    {
        Node content = _options.GetNode("%Content");

        foreach (Control child in content.GetChildren())
        {
            _tabs.Add(child.Name, child);
        }
    }

    private void SubscribeToNavBtns(Label titleLabel)
    {
        foreach (Button button in _navBtns)
        {
            string btnName = button.Name;

            button.FocusEntered += FocusEntered;
            button.Pressed += Pressed;

            _focusEnteredHandlers[button] = FocusEntered;
            _pressedHandlers[button] = Pressed;

            _buttons.Add(btnName, button);

            void FocusEntered() => ShowTab(titleLabel, btnName);
            void Pressed() => ShowTab(titleLabel, btnName);
        }
    }

    private void UnsubscribeFromNavBtns()
    {
        foreach (Button button in _navBtns)
        {
            button.FocusEntered -= _focusEnteredHandlers[button];
            button.Pressed -= _pressedHandlers[button];
        }

        _focusEnteredHandlers.Clear();
        _pressedHandlers.Clear();
    }

    private void SetButtonFields()
    {
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
    #endregion

    public void Dispose()
    {
        UnsubscribeFromNavBtns();
    }
}
