using Godot;
using GodotUtils;
using GodotUtils.UI;
using System.Collections.Generic;

namespace __TEMPLATE__.UI;

public partial class OptionsNav : Control
{
    private readonly Dictionary<string, Control> _tabs = [];
    private readonly Dictionary<string, Button> _buttons = [];

    private OptionsManager _optionsManager;

    public override void _Ready()
    {
        _optionsManager = GetNode<OptionsManager>(AutoloadPaths.OptionsManager);

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

        _buttons[_optionsManager.CurrentOptionsTab].GrabFocus();

        HideAllTabs();
        ShowTab(_optionsManager.CurrentOptionsTab);
    }

    private void ShowTab(string tabName)
    {
        _optionsManager.CurrentOptionsTab = tabName;
        HideAllTabs();
        _tabs[tabName].Show();
    }

    private void HideAllTabs()
    {
        _tabs.Values.ForEach(x => x.Hide());
    }
}
