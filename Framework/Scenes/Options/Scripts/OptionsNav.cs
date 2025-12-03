using Godot;
using System.Collections.Generic;

namespace __TEMPLATE__.UI;

public class OptionsNav(Options options)
{
    private readonly Dictionary<string, Control> _tabs = [];
    private readonly Dictionary<string, Button> _buttons = [];

    public void Initialize()
    {
        Node content = options.GetNode("%Content");

        foreach (Control child in content.GetChildren())
        {
            _tabs.Add(child.Name, child);
        }

        foreach (Button button in options.GetNode("%Nav").GetChildren())
        {
            button.FocusEntered += () => ShowTab(button.Name);
            button.Pressed += () => ShowTab(button.Name);

            _buttons.Add(button.Name, button);
        }

        _buttons[Game.Options.GetCurrentTab()].GrabFocus();

        HideAllTabs();
        ShowTab(Game.Options.GetCurrentTab());
    }

    private void ShowTab(string tabName)
    {
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
