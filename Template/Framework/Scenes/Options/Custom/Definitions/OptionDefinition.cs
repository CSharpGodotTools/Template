namespace Framework.Ui;

/// <summary>
/// Base type for a custom option that can be shown in the Options UI.
/// Game projects create small classes that inherit from this.
/// </summary>
public abstract class OptionDefinition
{
    /// <summary>
    /// Which tab this option appears under (Gameplay, Audio, etc).
    /// </summary>
    public abstract OptionsTab Tab { get; }

    /// <summary>
    /// Display text key used in the options UI (for example: "MOUSE_SENSITIVITY").
    /// </summary>
    public abstract string Label { get; }

    /// <summary>
    /// Optional ordering inside the tab. Lower numbers are shown first.
    /// </summary>
    public virtual int Order => 0;
}
