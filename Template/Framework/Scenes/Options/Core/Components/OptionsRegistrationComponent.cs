using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

internal sealed class OptionsRegistrationComponent
{
    private readonly OptionsValueStoreComponent _valueStore;

    public event Action<RegisteredSliderOption>? SliderOptionRegistered;
    public event Action<RegisteredDropdownOption>? DropdownOptionRegistered;
    public event Action<RegisteredLineEditOption>? LineEditOptionRegistered;
    public event Action<RegisteredToggleOption>? ToggleOptionRegistered;

    public OptionsRegistrationComponent(OptionsValueStoreComponent valueStore)
    {
        _valueStore = valueStore;
    }

    public IEnumerable<RegisteredSliderOption> GetSliderOptions() => _valueStore.GetSliderOptions();
    public IEnumerable<RegisteredDropdownOption> GetDropdownOptions() => _valueStore.GetDropdownOptions();
    public IEnumerable<RegisteredLineEditOption> GetLineEditOptions() => _valueStore.GetLineEditOptions();
    public IEnumerable<RegisteredToggleOption> GetToggleOptions() => _valueStore.GetToggleOptions();

    public void AddOption(OptionDefinition option)
    {
        ArgumentNullException.ThrowIfNull(option);

        switch (option)
        {
            case SliderOptionDefinition slider:
                SliderOptionRegistered?.Invoke(_valueStore.AddSlider(slider));
                break;
            case DropdownOptionDefinition dropdown:
                DropdownOptionRegistered?.Invoke(_valueStore.AddDropdown(dropdown));
                break;
            case LineEditOptionDefinition lineEdit:
                LineEditOptionRegistered?.Invoke(_valueStore.AddLineEdit(lineEdit));
                break;
            case ToggleOptionDefinition toggle:
                ToggleOptionRegistered?.Invoke(_valueStore.AddToggle(toggle));
                break;
            default:
                throw new NotSupportedException($"Unsupported option definition type: {option.GetType().Name}");
        }
    }
}
