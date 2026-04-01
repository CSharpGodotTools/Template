
#nullable enable
namespace __TEMPLATE__.Ui;

/// <summary>
/// Lightweight descriptor used by <see cref="CustomOptionSetup"/> when
/// building and sorting the options UI. Each instance wraps exactly one
/// registered option type.
/// </summary>
internal readonly struct CustomOptionDescriptor
{
    /// <summary>
    /// Gets stable option identifier.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets target tab name.
    /// </summary>
    public string Tab { get; }

    /// <summary>
    /// Gets descriptor option kind.
    /// </summary>
    public CustomOptionType Type { get; }

    /// <summary>
    /// Gets slider registration when <see cref="Type"/> is slider.
    /// </summary>
    public RegisteredSliderOption? Slider { get; }

    /// <summary>
    /// Gets dropdown registration when <see cref="Type"/> is dropdown.
    /// </summary>
    public RegisteredDropdownOption? Dropdown { get; }

    /// <summary>
    /// Gets line-edit registration when <see cref="Type"/> is line edit.
    /// </summary>
    public RegisteredLineEditOption? LineEdit { get; }

    /// <summary>
    /// Gets toggle registration when <see cref="Type"/> is toggle.
    /// </summary>
    public RegisteredToggleOption? Toggle { get; }

    // Shared constructor — every public overload delegates here
    /// <summary>
    /// Initializes descriptor state for a registered custom option.
    /// </summary>
    /// <param name="id">Stable option id.</param>
    /// <param name="tab">Target tab name.</param>
    /// <param name="type">Custom option kind.</param>
    /// <param name="slider">Slider registration when applicable.</param>
    /// <param name="dropdown">Dropdown registration when applicable.</param>
    /// <param name="lineEdit">Line-edit registration when applicable.</param>
    /// <param name="toggle">Toggle registration when applicable.</param>
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

    /// <summary>
    /// Creates descriptor from a slider registration.
    /// </summary>
    /// <param name="s">Registered slider option.</param>
    public CustomOptionDescriptor(RegisteredSliderOption s)
        : this(s.Id, s.Definition.Tab, CustomOptionType.Slider, slider: s) { }

    /// <summary>
    /// Creates descriptor from a dropdown registration.
    /// </summary>
    /// <param name="d">Registered dropdown option.</param>
    public CustomOptionDescriptor(RegisteredDropdownOption d)
        : this(d.Id, d.Definition.Tab, CustomOptionType.Dropdown, dropdown: d) { }

    /// <summary>
    /// Creates descriptor from a line-edit registration.
    /// </summary>
    /// <param name="l">Registered line-edit option.</param>
    public CustomOptionDescriptor(RegisteredLineEditOption l)
        : this(l.Id, l.Definition.Tab, CustomOptionType.LineEdit, lineEdit: l) { }

    /// <summary>
    /// Creates descriptor from a toggle registration.
    /// </summary>
    /// <param name="t">Registered toggle option.</param>
    public CustomOptionDescriptor(RegisteredToggleOption t)
        : this(t.Id, t.Definition.Tab, CustomOptionType.Toggle, toggle: t) { }
}
