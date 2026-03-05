
#nullable enable
namespace Framework.Ui;

/// <summary>
/// Lightweight descriptor used by <see cref="CustomOptionSetup"/> when
/// building and sorting the options UI. Each instance wraps exactly one
/// registered option type.
/// </summary>
internal readonly struct CustomOptionDescriptor
{
    public int Id { get; }
    public OptionsTab Tab { get; }
    public int Order { get; }
    public CustomOptionType Type { get; }

    public RegisteredSliderOption? Slider { get; }
    public RegisteredDropdownOption? Dropdown { get; }
    public RegisteredLineEditOption? LineEdit { get; }
    public RegisteredToggleOption? Toggle { get; }

    // Shared constructor — every public overload delegates here
    private CustomOptionDescriptor(
        int id, OptionsTab tab, int order, CustomOptionType type,
        RegisteredSliderOption? slider = null,
        RegisteredDropdownOption? dropdown = null,
        RegisteredLineEditOption? lineEdit = null,
        RegisteredToggleOption? toggle = null)
    {
        Id = id;
        Tab = tab;
        Order = order;
        Type = type;
        Slider = slider;
        Dropdown = dropdown;
        LineEdit = lineEdit;
        Toggle = toggle;
    }

    public CustomOptionDescriptor(RegisteredSliderOption s)
        : this(s.Id, s.Definition.Tab, s.Definition.Order, CustomOptionType.Slider, slider: s) { }

    public CustomOptionDescriptor(RegisteredDropdownOption d)
        : this(d.Id, d.Definition.Tab, d.Definition.Order, CustomOptionType.Dropdown, dropdown: d) { }

    public CustomOptionDescriptor(RegisteredLineEditOption l)
        : this(l.Id, l.Definition.Tab, l.Definition.Order, CustomOptionType.LineEdit, lineEdit: l) { }

    public CustomOptionDescriptor(RegisteredToggleOption t)
        : this(t.Id, t.Definition.Tab, t.Definition.Order, CustomOptionType.Toggle, toggle: t) { }
}
