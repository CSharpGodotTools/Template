namespace __TEMPLATE__.Ui;

/// <summary>
/// Class-based definition for a custom line edit option.
/// Implement this in game code, then register with:
/// Game.OptionsManager.AddOption(new YourLineEditOption()).
/// </summary>
public abstract class LineEditOptionDefinition : OptionDefinition
{
    /// <summary>
    /// Placeholder text shown in the input field when empty.
    /// </summary>
    public virtual string Placeholder => string.Empty;

    /// <summary>
    /// Default text used when first created.
    /// </summary>
    public virtual string DefaultValue => string.Empty;

    /// <summary>
    /// Reads the current text from your game settings source.
    /// </summary>
    /// <returns>Current text value.</returns>
    public abstract string GetValue();

    /// <summary>
    /// Writes text back to your game settings source.
    /// </summary>
    /// <param name="value">Text value to persist.</param>
    public abstract void SetValue(string value);
}
