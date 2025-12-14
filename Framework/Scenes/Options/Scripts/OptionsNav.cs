using Godot;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.UI;

public class OptionsNav(Options options)
{
    private readonly Dictionary<string, Control> _tabs = [];
    private readonly Dictionary<string, Button> _buttons = [];

    public void Initialize()
    {
        SetupContent();
        SubscribeToNavBtns();
        FocusOnLastClickedNavBtn();
        HideAllTabs();
        ShowCurrentTab();
    }

    private void SetupContent()
    {
        Node content = options.GetNode("%Content");

        foreach (Control child in content.GetChildren())
        {
            _tabs.Add(child.Name, child);
        }
    }

    private void SubscribeToNavBtns()
    {
        foreach (Button button in options.GetNode("%Nav").GetChildren())
        {
            string btnName = button.Name;
            button.FocusEntered += () => ShowTab(btnName);
            button.Pressed += () => ShowTab(btnName);
            
            _buttons.Add(btnName, button);
        }
    }

    private void FocusOnLastClickedNavBtn()
    {
        _buttons[Game.Options.GetCurrentTab()].GrabFocus();
    }

    private void ShowCurrentTab()
    {
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
