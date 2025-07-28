using Godot;
using GodotUtils;
using System.Collections.Generic;
using System.Linq;

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

        _buttons[OptionsManager.CurrentOptionsTab].GrabFocus();

        HideAllTabs();
        ShowTab(OptionsManager.CurrentOptionsTab);
    }

    private void ShowTab(string tabName)
    {
        OptionsManager.CurrentOptionsTab = tabName;
        HideAllTabs();
        _tabs[tabName].Show();
    }

    private void HideAllTabs()
    {
        _tabs.Values.ForEach(x => x.Hide());
    }
}

