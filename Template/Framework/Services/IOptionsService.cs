using __TEMPLATE__.Ui;
using GodotUtils;
using System;

namespace __TEMPLATE__;

public interface IOptionsService
{
    event Action<WindowMode> WindowModeChanged;

    OptionsSettings Settings { get; }

    string GetCurrentTab();
    void SetCurrentTab(string tab);

    ResourceHotkeys GetHotkeys();
    void ResetHotkeys();

    void AddOption(OptionDefinition option);
    void AddRightControl(OptionRightControlDefinition definition);
}
