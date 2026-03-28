namespace __TEMPLATE__.Ui;

/// <summary>
/// Class-based definition for a custom slider option.
/// Implement this in game code, then register with:
/// Game.OptionsManager.AddSlider(new YourSliderOption()).
/// </summary>
public abstract class SliderOptionDefinition : OptionDefinition
{
    /// <summary>
    /// Minimum value allowed by the UI slider.
    /// </summary>
    public abstract double MinValue { get; }

    /// <summary>
    /// Maximum value allowed by the UI slider.
    /// </summary>
    public abstract double MaxValue { get; }

    /// <summary>
    /// Increment step used by the UI slider.
    /// </summary>
    public virtual double Step => 1.0;

    /// <summary>
    /// Default value used when the option is first created.
    /// </summary>
    public virtual float DefaultValue => 0.0f;

    /// <summary>
    /// Reads the current value from your game settings source.
    /// </summary>
    public abstract float GetValue();

    /// <summary>
    /// Writes the value back to your game settings source.
    /// </summary>
    public abstract void SetValue(float value);
}
