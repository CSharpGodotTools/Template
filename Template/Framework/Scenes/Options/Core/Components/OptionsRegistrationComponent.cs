using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Registers option definitions and exposes typed registration events.
/// </summary>
internal sealed class OptionsRegistrationComponent
{
    private readonly OptionsValueStoreComponent _valueStore;

    /// <summary>
    /// Raised when a slider option is registered.
    /// </summary>
    public event Action<RegisteredSliderOption>? SliderOptionRegistered;

    /// <summary>
    /// Raised when a dropdown option is registered.
    /// </summary>
    public event Action<RegisteredDropdownOption>? DropdownOptionRegistered;

    /// <summary>
    /// Raised when a line-edit option is registered.
    /// </summary>
    public event Action<RegisteredLineEditOption>? LineEditOptionRegistered;

    /// <summary>
    /// Raised when a toggle option is registered.
    /// </summary>
    public event Action<RegisteredToggleOption>? ToggleOptionRegistered;

    /// <summary>
    /// Initializes a new registration component.
    /// </summary>
    /// <param name="valueStore">Backing store for registered option metadata.</param>
    public OptionsRegistrationComponent(OptionsValueStoreComponent valueStore)
    {
        _valueStore = valueStore;
    }

    /// <summary>
    /// Gets all registered slider options.
    /// </summary>
    /// <returns>Registered slider option sequence.</returns>
    public IEnumerable<RegisteredSliderOption> GetSliderOptions() => _valueStore.GetSliderOptions();

    /// <summary>
    /// Gets all registered dropdown options.
    /// </summary>
    /// <returns>Registered dropdown option sequence.</returns>
    public IEnumerable<RegisteredDropdownOption> GetDropdownOptions() => _valueStore.GetDropdownOptions();

    /// <summary>
    /// Gets all registered line-edit options.
    /// </summary>
    /// <returns>Registered line-edit option sequence.</returns>
    public IEnumerable<RegisteredLineEditOption> GetLineEditOptions() => _valueStore.GetLineEditOptions();

    /// <summary>
    /// Gets all registered toggle options.
    /// </summary>
    /// <returns>Registered toggle option sequence.</returns>
    public IEnumerable<RegisteredToggleOption> GetToggleOptions() => _valueStore.GetToggleOptions();

    /// <summary>
    /// Registers an option definition and emits the corresponding typed event.
    /// </summary>
    /// <param name="option">Option definition to register.</param>
    public void AddOption(OptionDefinition option)
    {
        ArgumentNullException.ThrowIfNull(option);


        // Dispatch by concrete definition type to preserve typed registration events.
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
