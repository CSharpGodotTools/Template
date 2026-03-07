using System;
using System.Collections.Generic;

namespace Framework.Ui;

/// <summary>
/// Creates and manages UI controls for custom options registered via OptionsManager.
/// </summary>
public sealed class OptionsCustom : IDisposable
{
    // Event handlers stored for unsubscription
    private readonly Action<RegisteredSliderOption> _sliderHandler;
    private readonly Action<RegisteredDropdownOption> _dropdownHandler;
    private readonly Action<RegisteredLineEditOption> _lineEditHandler;
    private readonly Action<RegisteredToggleOption> _toggleHandler;

    // Properties
    internal OptionsNav Nav { get; }
    internal OptionsManager OptionsManager { get; } = GameFramework.Options;
    internal Dictionary<int, IDisposable> Bindings { get; } = [];

    /// <summary>
    /// Constructs the custom options UI manager and subscribes to option registration events.
    /// </summary>
    public OptionsCustom(OptionsNav nav)
    {
        Nav = nav;

        // Wire event handlers that delegate to the setup helper
        _sliderHandler = s => CustomOptionSetup.OnSliderRegistered(this, s);
        _dropdownHandler = d => CustomOptionSetup.OnDropdownRegistered(this, d);
        _lineEditHandler = l => CustomOptionSetup.OnLineEditRegistered(this, l);
        _toggleHandler = t => CustomOptionSetup.OnToggleRegistered(this, t);

        // Build UI for options that were registered before this screen opened
        CustomOptionSetup.Setup(this);

        // React to options registered at runtime
        OptionsManager.SliderOptionRegistered += _sliderHandler;
        OptionsManager.DropdownOptionRegistered += _dropdownHandler;
        OptionsManager.LineEditOptionRegistered += _lineEditHandler;
        OptionsManager.ToggleOptionRegistered += _toggleHandler;
    }

    /// <summary>
    /// Unsubscribes from events and disposes all UI bindings.
    /// </summary>
    public void Dispose()
    {
        OptionsManager.SliderOptionRegistered -= _sliderHandler;
        OptionsManager.DropdownOptionRegistered -= _dropdownHandler;
        OptionsManager.LineEditOptionRegistered -= _lineEditHandler;
        OptionsManager.ToggleOptionRegistered -= _toggleHandler;

        foreach (IDisposable binding in Bindings.Values)
            binding.Dispose();

        Bindings.Clear();
    }
}
