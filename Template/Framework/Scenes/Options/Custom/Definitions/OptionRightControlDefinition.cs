using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Describes a UI control that should be attached to the right side of another option row.
/// </summary>
public sealed class OptionRightControlDefinition
{
    private readonly string _tab;
    private readonly string _targetLabel;
    private readonly string _name;
    private readonly Func<Control, Control> _createControl;
    private readonly Action<Control, Control>? _onAttached;
    private readonly Action<Control, Control>? _onDetaching;

    public OptionRightControlDefinition(
        string tab,
        string targetLabel,
        string name,
        Func<Control, Control> createControl,
        Action<Control, Control>? onAttached = null,
        Action<Control, Control>? onDetaching = null)
    {
        _tab = tab;
        _targetLabel = targetLabel;
        _name = name;
        _createControl = createControl;
        _onAttached = onAttached;
        _onDetaching = onDetaching;
    }

    public string Tab => _tab;
    public string TargetLabel => _targetLabel;
    public string Name => _name;
    public Func<Control, Control> CreateControl => _createControl;
    public Action<Control, Control>? OnAttached => _onAttached;
    public Action<Control, Control>? OnDetaching => _onDetaching;
}
