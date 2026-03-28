
#nullable enable
namespace __TEMPLATE__.Ui;

/// <summary>
/// Lightweight descriptor used by <see cref="CustomOptionSetup"/> when
/// building and sorting the options UI. Each instance wraps exactly one
/// registered option type.
/// </summary>
internal readonly struct CustomOptionDescriptor
{
    public int Id { get; }
    public string Tab { get; }
    public CustomOptionType Type { get; }

    public RegisteredSliderOption? Slider { get; }
    public RegisteredDropdownOption? Dropdown { get; }
    public RegisteredLineEditOption? LineEdit { get; }
    public RegisteredToggleOption? Toggle { get; }

    // Shared constructor — every public overload delegates here
    private CustomOptionDescriptor(
        int id, string tab, CustomOptionType type,
        RegisteredSliderOption? slider = null,
        RegisteredDropdownOption? dropdown = null,
        RegisteredLineEditOption? lineEdit = null,
        RegisteredToggleOption? toggle = null)
    {
        Id = id;
        Tab = tab;
        Type = type;
        Slider = slider;
        Dropdown = dropdown;
        LineEdit = lineEdit;
        Toggle = toggle;
    }

    public CustomOptionDescriptor(RegisteredSliderOption s)
        : this(s.Id, s.Definition.Tab, CustomOptionType.Slider, slider: s) { }

    public CustomOptionDescriptor(RegisteredDropdownOption d)
        : this(d.Id, d.Definition.Tab, CustomOptionType.Dropdown, dropdown: d) { }

    public CustomOptionDescriptor(RegisteredLineEditOption l)
        : this(l.Id, l.Definition.Tab, CustomOptionType.LineEdit, lineEdit: l) { }

    public CustomOptionDescriptor(RegisteredToggleOption t)
        : this(t.Id, t.Definition.Tab, CustomOptionType.Toggle, toggle: t) { }
}
