using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Ui;

public class OptionsNav : IDisposable
{
    // Nodes
    public Button GeneralButton => _generalButton;
    public Button GameplayButton => _gameplayButton;
    public Button DisplayButton => _displayButton;
    public Button GraphicsButton => _graphicsButton;
    public Button AudioButton => _audioButton;
    public Button InputButton => _inputButton;

    private Button _generalButton = null!;
    private Button _gameplayButton = null!;
    private Button _displayButton = null!;
    private Button _graphicsButton = null!;
    private Button _audioButton = null!;
    private Button _inputButton = null!;

    // Fields
    private readonly Godot.Collections.Array<Node> _navBtns;
    private readonly Dictionary<string, Control> _tabs = [];
    private readonly Dictionary<string, Button> _buttons = [];
    private readonly Dictionary<Button, Action> _focusEnteredHandlers = [];
    private readonly Dictionary<Button, Action> _pressedHandlers = [];
    private readonly Options _options;
    private readonly Label _titleLabel;

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

    // Private Methods
    private void SetupContent()
    {
        Node content = _options.GetNode("%Content");

        foreach (Control child in content.GetChildren().Cast<Control>())
        {
            _tabs.Add(child.Name, child);
        }
    }

    private void SubscribeToNavBtns(Label titleLabel)
    {
        foreach (Button button in _navBtns.Cast<Button>())
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
        foreach (Button button in _navBtns.Cast<Button>())
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
        _buttons[GameFramework.Options.GetCurrentTab()].GrabFocus();
    }

    private void ShowCurrentTab(Label titleLabel)
    {
        ShowTab(titleLabel, GameFramework.Options.GetCurrentTab());
    }

    private void ShowTab(Label titleLabel, string tabName)
    {
        titleLabel.Text = tabName;
        GameFramework.Options.SetCurrentTab(tabName);
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

    public bool TryGetTabContainer(OptionsTab tab, out VBoxContainer container)
    {
        if (_tabs.TryGetValue(tab.ToString(), out Control? tabControl) && tabControl is VBoxContainer tabContainer)
        {
            container = tabContainer;
            return true;
        }

        container = null!;
        return false;
    }

    // Dispose
    public void Dispose()
    {
        UnsubscribeFromNavBtns();
    }
}
