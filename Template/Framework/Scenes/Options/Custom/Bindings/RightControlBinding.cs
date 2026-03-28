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

    private RightControlBinding(
        Control control,
        Control anchorControl,
        Action<Control, Control>? onDetaching)
    {
        _control = control;
        _anchorControl = anchorControl;
        _onDetaching = onDetaching;
    }

    internal static RightControlBinding Create(
        HBoxContainer controlsContainer,
        Control anchorControl,
        RegisteredRightControl rightControl)
    {
        OptionRightControlDefinition definition = rightControl.Definition;

        Control control = definition.CreateControl(anchorControl)
            ?? throw new InvalidOperationException($"Right control '{definition.Name}' returned null.");

        if (!string.IsNullOrWhiteSpace(definition.Name))
            control.Name = definition.Name;

        controlsContainer.AddChild(control);
        definition.OnAttached?.Invoke(control, anchorControl);

        return new RightControlBinding(control, anchorControl, definition.OnDetaching);
    }

    public void Dispose()
    {
        _onDetaching?.Invoke(_control, _anchorControl);

        if (GodotObject.IsInstanceValid(_control))
            _control.QueueFree();
    }
}
