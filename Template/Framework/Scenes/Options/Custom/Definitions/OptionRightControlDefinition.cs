using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Describes a UI control that should be attached to the right side of another option row.
/// </summary>
public sealed class OptionRightControlDefinition
{

    /// <summary>
    /// Initializes a right-control definition.
    /// </summary>
    /// <param name="tab">Target options tab name.</param>
    /// <param name="targetLabel">Target option label in that tab.</param>
    /// <param name="name">Optional control name.</param>
    /// <param name="createControl">Factory that creates the right-side control.</param>
    /// <param name="onAttached">Optional callback invoked after control is attached.</param>
    /// <param name="onDetaching">Optional callback invoked before control disposal.</param>
    public OptionRightControlDefinition(
        string tab,
        string targetLabel,
        string name,
        Func<Control, Control> createControl,
        Action<Control, Control>? onAttached = null,
        Action<Control, Control>? onDetaching = null)
    {
        Tab = tab;
        TargetLabel = targetLabel;
        Name = name;
        CreateControl = createControl;
        OnAttached = onAttached;
        OnDetaching = onDetaching;
    }

    /// <summary>
    /// Gets target tab name.
    /// </summary>
    public string Tab { get; }

    /// <summary>
    /// Gets target option label.
    /// </summary>
    public string TargetLabel { get; }

    /// <summary>
    /// Gets right control name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets control factory delegate.
    /// </summary>
    public Func<Control, Control> CreateControl { get; }

    /// <summary>
    /// Gets callback invoked after control attachment.
    /// </summary>
    public Action<Control, Control>? OnAttached { get; }

    /// <summary>
    /// Gets callback invoked before control detaches.
    /// </summary>
    public Action<Control, Control>? OnDetaching { get; }
}
