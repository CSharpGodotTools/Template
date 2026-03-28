using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Ui;

public class OptionsNav : IDisposable
{
    // Fields
    private readonly Godot.Collections.Array<Node> _navBtns;
    private readonly Dictionary<string, Control> _tabs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Button> _buttons = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Button, Action> _focusEnteredHandlers = [];
    private readonly Dictionary<Button, Action> _pressedHandlers = [];
    private readonly Options _options;
    private readonly Label _titleLabel;
    private readonly OptionsManager _optionsManager;

    public OptionsNav(Options options, Label titleLabel, OptionsManager optionsManager)
    {
        _options = options;
        _titleLabel = titleLabel;
        _optionsManager = optionsManager;
        _navBtns = options.GetNode("%Nav").GetChildren();

        SetupContent();
        SubscribeToNavBtns();
        HideAllTabs();
        EnsureCurrentTabSelection();
        FocusCurrentTabButton();
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

    private void SubscribeToNavBtns()
    {
        foreach (Button button in _navBtns.Cast<Button>())
        {
            string btnName = button.Name;

            button.FocusEntered += FocusEntered;
            button.Pressed += Pressed;

            _focusEnteredHandlers[button] = FocusEntered;
            _pressedHandlers[button] = Pressed;

            _buttons.Add(btnName, button);

            void FocusEntered() => ShowTab(btnName);
            void Pressed() => ShowTab(btnName);
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

    private void FocusCurrentTabButton()
    {
        if (_buttons.TryGetValue(_optionsManager.GetCurrentTab(), out Button? current) && IsTabSelectable(current.Name))
        {
            current.GrabFocus();
            return;
        }

        string? fallback = GetFirstSelectableTabName();

        if (fallback != null && _buttons.TryGetValue(fallback, out Button? first))
            first.GrabFocus();
    }

    public void EnsureCurrentTabSelection()
    {
        string currentTab = _optionsManager.GetCurrentTab();

        if (IsTabSelectable(currentTab))
        {
            ShowTab(currentTab);
            return;
        }

        string? fallback = GetFirstSelectableTabName();

        if (fallback != null)
            ShowTab(fallback);
    }

    private void ShowTab(string tabName)
    {
        if (!IsTabSelectable(tabName))
            return;

        _titleLabel.Text = tabName;
        _optionsManager.SetCurrentTab(tabName);
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

    public IEnumerable<string> GetTabNames()
    {
        return _tabs.Keys;
    }

    public void SetTabEnabled(string tabName, bool enabled, bool ensureSelection = true)
    {
        if (!_tabs.TryGetValue(tabName, out Control? tab))
            return;

        if (_buttons.TryGetValue(tabName, out Button? button))
        {
            button.Visible = enabled;
            button.Disabled = !enabled;
        }

        if (!enabled)
            tab.Hide();

        if (ensureSelection)
            EnsureCurrentTabSelection();
    }

    public void RefreshOptionalTabs(string alwaysVisibleTabName)
    {
        foreach ((string tabName, Control tabControl) in _tabs)
        {
            bool keepVisible = string.Equals(tabName, alwaysVisibleTabName, StringComparison.OrdinalIgnoreCase);
            bool hasContent = tabControl.GetChildCount() > 0;
            SetTabEnabled(tabName, keepVisible || hasContent, ensureSelection: false);
        }

        EnsureCurrentTabSelection();
        FocusCurrentTabButton();
    }

    private bool IsTabSelectable(string tabName)
    {
        return _tabs.ContainsKey(tabName)
            && _buttons.TryGetValue(tabName, out Button? button)
            && button.Visible
            && !button.Disabled;
    }

    private string? GetFirstSelectableTabName()
    {
        foreach (KeyValuePair<string, Button> pair in _buttons)
        {
            if (IsTabSelectable(pair.Key))
                return pair.Key;
        }

        return null;
    }

    public bool TryGetTabContainer(string tabName, out VBoxContainer container)
    {
        if (_tabs.TryGetValue(tabName, out Control? tabControl) && tabControl is VBoxContainer tabContainer)
        {
            container = tabContainer;
            return true;
        }

        container = null!;
        return false;
    }

    public bool TryGetTabButton(string tabName, out Button button)
    {
        if (_buttons.TryGetValue(tabName, out Button? tabButton))
        {
            button = tabButton;
            return true;
        }

        button = null!;
        return false;
    }

    public bool TryGetTab(string tabName, out VBoxContainer container, out Button button)
    {
        if (TryGetTabContainer(tabName, out container) && TryGetTabButton(tabName, out button))
            return true;

        container = null!;
        button = null!;
        return false;
    }

    // Dispose
    public void Dispose()
    {
        UnsubscribeFromNavBtns();
    }
}
