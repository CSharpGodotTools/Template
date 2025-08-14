using Godot;
using GodotUtils.UI;
using System.Collections.Generic;

namespace __TEMPLATE__.UI;

public partial class OptionsNav : Control
{
    private readonly Dictionary<string, Control> _tabs = [];
    private readonly Dictionary<string, Button> _buttons = [];

    public override void _Ready()
    {
        Node content = GetParent().GetNode("Content");

        foreach (Control child in content.GetChildren())
        {
            _tabs.Add(child.Name, child);
        }

        foreach (Button button in GetChildren())
        {
            button.FocusEntered += () => ShowTab(button.Name);
            button.Pressed += () => ShowTab(button.Name);

            _buttons.Add(button.Name, button);
        }

        _buttons[OptionsManager.GetCurrentTab()].GrabFocus();

        HideAllTabs();
        ShowTab(OptionsManager.GetCurrentTab());
    }

    private void ShowTab(string tabName)
    {
        OptionsManager.SetCurrentTab(tabName);
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
