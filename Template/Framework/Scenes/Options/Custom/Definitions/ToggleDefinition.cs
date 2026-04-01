namespace __TEMPLATE__.Ui;

/// <summary>
/// Class‑based definition for a custom boolean toggle option (displayed as a checkbox).
/// Implement this in game code, then register with:
/// Game.OptionsManager.AddOption(new YourToggleOption()).
/// </summary>
public abstract class ToggleOptionDefinition : OptionDefinition
{
    /// <summary>
    /// Default checked state when the option is first created.
    /// </summary>
    public virtual bool DefaultValue => false;

    /// <summary>
    /// Reads the current boolean value from your game settings source.
    /// </summary>
    /// <returns>Current toggle state.</returns>
    public abstract bool GetValue();

    /// <summary>
    /// Writes the boolean value back to your game settings source.
    /// </summary>
    /// <param name="value">Toggle state to persist.</param>
    public abstract void SetValue(bool value);
}
