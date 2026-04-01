using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Disposable helper that owns a right-side control attached to an existing option row.
/// </summary>
internal sealed class RightControlBinding : IDisposable
{
    private readonly Control _control;
    private readonly Control _anchorControl;
    private readonly Action<Control, Control>? _onDetaching;

    /// <summary>
    /// Initializes a right-control binding wrapper.
    /// </summary>
    /// <param name="control">Created right-side control instance.</param>
    /// <param name="anchorControl">Anchor control this right control augments.</param>
    /// <param name="onDetaching">Optional callback invoked before disposal.</param>
    private RightControlBinding(
        Control control,
        Control anchorControl,
        Action<Control, Control>? onDetaching)
    {
        _control = control;
        _anchorControl = anchorControl;
        _onDetaching = onDetaching;
    }

    /// <summary>
    /// Creates and attaches a right-side control to an existing options row.
    /// </summary>
    /// <param name="controlsContainer">Container that receives the created control.</param>
    /// <param name="anchorControl">Anchor control for callbacks and context.</param>
    /// <param name="rightControl">Registered right-control metadata.</param>
    /// <returns>Disposable binding for the created control.</returns>
    internal static RightControlBinding Create(
        HBoxContainer controlsContainer,
        Control anchorControl,
        RegisteredRightControl rightControl)
    {
        OptionRightControlDefinition definition = rightControl.Definition;

        Control control = definition.CreateControl(anchorControl)
            ?? throw new InvalidOperationException($"Right control '{definition.Name}' returned null.");

        // Apply explicit control name only when definition provides one.
        if (!string.IsNullOrWhiteSpace(definition.Name))
            control.Name = definition.Name;

        controlsContainer.AddChild(control);
        definition.OnAttached?.Invoke(control, anchorControl);

        return new RightControlBinding(control, anchorControl, definition.OnDetaching);
    }

    /// <summary>
    /// Invokes detach callback and frees the attached right-side control.
    /// </summary>
    public void Dispose()
    {
        _onDetaching?.Invoke(_control, _anchorControl);

        // Free control only while the control instance remains valid.
        if (GodotObject.IsInstanceValid(_control))
            _control.QueueFree();
    }
}
