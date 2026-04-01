using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

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
    private readonly Action<RegisteredRightControl> _rightControlHandler;

    // Properties
    /// <summary>
    /// Gets navigation helper used to resolve tabs and tab containers.
    /// </summary>
    internal OptionsNav Nav { get; }

    /// <summary>
    /// Gets options manager that provides registration events and data.
    /// </summary>
    internal OptionsManager OptionsManager { get; }

    /// <summary>
    /// Gets active custom option bindings keyed by option id.
    /// </summary>
    internal Dictionary<int, IDisposable> Bindings { get; } = [];

    /// <summary>
    /// Gets active right-control bindings keyed by right-control id.
    /// </summary>
    internal Dictionary<int, IDisposable> RightControlBindings { get; } = [];

    /// <summary>
    /// Constructs the custom options UI manager and subscribes to option registration events.
    /// </summary>
    /// <param name="nav">Navigation helper for tab lookup/selection.</param>
    /// <param name="optionsManager">Options manager providing registrations/events.</param>
    public OptionsCustom(OptionsNav nav, OptionsManager optionsManager)
    {
        Nav = nav;
        OptionsManager = optionsManager;

        // Wire event handlers that delegate to the setup helper
        _sliderHandler = s => CustomOptionSetup.OnSliderRegistered(this, s);
        _dropdownHandler = d => CustomOptionSetup.OnDropdownRegistered(this, d);
        _lineEditHandler = l => CustomOptionSetup.OnLineEditRegistered(this, l);
        _toggleHandler = t => CustomOptionSetup.OnToggleRegistered(this, t);
        _rightControlHandler = r => CustomOptionSetup.OnRightControlRegistered(this, r);

        // Build UI for options that were registered before this screen opened
        CustomOptionSetup.Setup(this);

        // React to options registered at runtime
        OptionsManager.SliderOptionRegistered += _sliderHandler;
        OptionsManager.DropdownOptionRegistered += _dropdownHandler;
        OptionsManager.LineEditOptionRegistered += _lineEditHandler;
        OptionsManager.ToggleOptionRegistered += _toggleHandler;
        OptionsManager.RightControlRegistered += _rightControlHandler;
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
        OptionsManager.RightControlRegistered -= _rightControlHandler;

        foreach (IDisposable binding in Bindings.Values)
            binding.Dispose();

        foreach (IDisposable binding in RightControlBindings.Values)
            binding.Dispose();

        Bindings.Clear();
        RightControlBindings.Clear();
    }
}
