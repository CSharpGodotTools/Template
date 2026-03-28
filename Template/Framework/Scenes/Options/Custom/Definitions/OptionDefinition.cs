namespace __TEMPLATE__.Ui;

/// <summary>
/// Base type for a custom option that can be shown in the Options UI.
/// Game projects create small classes that inherit from this.
/// </summary>
public abstract class OptionDefinition
{
    /// <summary>
    /// Which tab this option appears under (for example: "Gameplay").
    /// </summary>
    public abstract string Tab { get; }

    /// <summary>
    /// Display text key used in the options UI (for example: "MOUSE_SENSITIVITY").
    /// </summary>
    public abstract string Label { get; }

    /// <summary>
    /// Optional explicit persistence key for options.json.
    /// Leave null to use the framework-generated key.
    /// </summary>
    public virtual string? SaveKey => null;
}
