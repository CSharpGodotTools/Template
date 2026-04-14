using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Manages options tab navigation state, selection, and visibility.
/// </summary>
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

    /// <summary>
    /// Initializes tab navigation and focuses the current or fallback tab.
    /// </summary>
    /// <param name="options">Options scene root.</param>
    /// <param name="titleLabel">Label that displays selected tab name.</param>
    /// <param name="optionsManager">Options manager for tab persistence.</param>
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
    /// <summary>
    /// Builds tab-name to container mappings from content children.
    /// </summary>
    private void SetupContent()
    {
        Node content = _options.GetNode("%Content");

        foreach (Control child in content.GetChildren().Cast<Control>())
            _tabs.Add(child.Name, child);
    }

    /// <summary>
    /// Subscribes focus/pressed handlers for each navigation button.
    /// </summary>
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

    /// <summary>
    /// Unsubscribes all navigation button handlers.
    /// </summary>
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

    /// <summary>
    /// Focuses the current tab button or first selectable fallback tab.
    /// </summary>
    private void FocusCurrentTabButton()
    {
        // Focus persisted tab when it is still selectable.
        if (_buttons.TryGetValue(_optionsManager.GetCurrentTab(), out Button? current) && IsTabSelectable(current.Name))
        {
            current.GrabFocus();
            return;
        }

        string? fallback = GetFirstSelectableTabName();

        // Fall back to the first selectable tab button when current tab is invalid.
        if (fallback != null && _buttons.TryGetValue(fallback, out Button? first))
            first.GrabFocus();
    }

    /// <summary>
    /// Ensures a valid selectable tab is shown.
    /// </summary>
    public void EnsureCurrentTabSelection()
    {
        string currentTab = _optionsManager.GetCurrentTab();

        // Keep current tab when it is still selectable.
        if (IsTabSelectable(currentTab))
        {
            ShowTab(currentTab);
            return;
        }

        string? fallback = GetFirstSelectableTabName();

        // Switch to fallback tab when current tab is no longer selectable.
        if (fallback != null)
            ShowTab(fallback);
    }

    /// <summary>
    /// Shows the requested tab when selectable and updates selection state.
    /// </summary>
    /// <param name="tabName">Tab name to display.</param>
    private void ShowTab(string tabName)
    {
        // Ignore requests for hidden or disabled tabs.
        if (!IsTabSelectable(tabName))
            return;

        _titleLabel.Text = tabName;
        _optionsManager.SetCurrentTab(tabName);
        HideAllTabs();
        _tabs[tabName].Show();
    }

    /// <summary>
    /// Hides all tab containers.
    /// </summary>
    private void HideAllTabs()
    {
        foreach (Control tab in _tabs.Values)
            tab.Hide();
    }

    /// <summary>
    /// Gets available tab names.
    /// </summary>
    /// <returns>Tab name sequence.</returns>
    public IEnumerable<string> GetTabNames()
    {
        return _tabs.Keys;
    }

    /// <summary>
    /// Enables or disables a tab and optionally revalidates current selection.
    /// </summary>
    /// <param name="tabName">Tab to enable or disable.</param>
    /// <param name="enabled">Whether tab should be visible/selectable.</param>
    /// <param name="ensureSelection">Whether to revalidate current selection.</param>
    public void SetTabEnabled(string tabName, bool enabled, bool ensureSelection = true)
    {
        // Ignore unknown tab names.
        if (!_tabs.TryGetValue(tabName, out Control? tab))
            return;

        // Toggle corresponding nav button when present.
        if (_buttons.TryGetValue(tabName, out Button? button))
        {
            button.Visible = enabled;
            button.Disabled = !enabled;
        }

        // Hide content immediately when disabling a tab.
        if (!enabled)
            tab.Hide();

        // Revalidate active tab selection when requested.
        if (ensureSelection)
            EnsureCurrentTabSelection();
    }

    /// <summary>
    /// Hides optional tabs without content unless explicitly pinned.
    /// </summary>
    /// <param name="alwaysVisibleTabNames">Tabs that must stay visible.</param>
    public void RefreshOptionalTabs(params string[] alwaysVisibleTabNames)
    {
        HashSet<string> pinnedTabs = new(
            alwaysVisibleTabNames ?? [],
            StringComparer.OrdinalIgnoreCase);

        foreach ((string tabName, Control tabControl) in _tabs)
        {
            bool keepVisible = pinnedTabs.Contains(tabName);
            bool hasContent = tabControl.GetChildCount() > 0;
            SetTabEnabled(tabName, keepVisible || hasContent, ensureSelection: false);
        }

        EnsureCurrentTabSelection();
        FocusCurrentTabButton();
    }

    /// <summary>
    /// Determines whether a tab is currently selectable.
    /// </summary>
    /// <param name="tabName">Tab name to evaluate.</param>
    /// <returns><see langword="true"/> when tab exists and button is enabled/visible.</returns>
    private bool IsTabSelectable(string tabName)
    {
        return _tabs.ContainsKey(tabName)
            && _buttons.TryGetValue(tabName, out Button? button)
            && button.Visible
            && !button.Disabled;
    }

    /// <summary>
    /// Gets the first selectable tab name in button order.
    /// </summary>
    /// <returns>First selectable tab name, or <see langword="null"/>.</returns>
    private string? GetFirstSelectableTabName()
    {
        foreach (KeyValuePair<string, Button> pair in _buttons)
        {
            // Return the first tab that can currently be focused and shown.
            if (IsTabSelectable(pair.Key))
                return pair.Key;
        }

        return null;
    }

    /// <summary>
    /// Gets a tab container when the tab exists and is a <see cref="VBoxContainer"/>.
    /// </summary>
    /// <param name="tabName">Tab name to resolve.</param>
    /// <param name="container">Resolved tab container on success.</param>
    /// <returns><see langword="true"/> when tab container exists.</returns>
    public bool TryGetTabContainer(string tabName, out VBoxContainer container)
    {
        // Return container only when tab exists and is a VBoxContainer.
        if (_tabs.TryGetValue(tabName, out Control? tabControl) && tabControl is VBoxContainer tabContainer)
        {
            container = tabContainer;
            return true;
        }

        container = null!;
        return false;
    }

    /// <summary>
    /// Gets a tab button by tab name.
    /// </summary>
    /// <param name="tabName">Tab name to resolve.</param>
    /// <param name="button">Resolved tab button on success.</param>
    /// <returns><see langword="true"/> when tab button exists.</returns>
    public bool TryGetTabButton(string tabName, out Button button)
    {
        // Return button only when tab-to-button mapping exists.
        if (_buttons.TryGetValue(tabName, out Button? tabButton))
        {
            button = tabButton;
            return true;
        }

        button = null!;
        return false;
    }

    /// <summary>
    /// Resolves both tab container and tab button for a tab name.
    /// </summary>
    /// <param name="tabName">Tab name to resolve.</param>
    /// <param name="container">Resolved container on success.</param>
    /// <param name="button">Resolved button on success.</param>
    /// <returns><see langword="true"/> when both container and button exist.</returns>
    public bool TryGetTab(string tabName, out VBoxContainer container, out Button button)
    {
        // Succeed only when both tab container and nav button are available.
        if (TryGetTabContainer(tabName, out container) && TryGetTabButton(tabName, out button))
            return true;

        container = null!;
        button = null!;
        return false;
    }

    // Dispose
    /// <summary>
    /// Unsubscribes navigation button handlers.
    /// </summary>
    public void Dispose()
    {
        UnsubscribeFromNavBtns();
    }
}
