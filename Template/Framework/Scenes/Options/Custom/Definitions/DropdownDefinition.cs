using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Class-based definition for a custom dropdown option.
/// Implement this in game code, then register with:
/// Game.OptionsManager.AddOption(new YourDropdownOption()).
/// </summary>
public abstract class DropdownOptionDefinition : OptionDefinition
{
    /// <summary>
    /// Default minimum width of dropdown controls in the options UI.
    /// </summary>
    public const float DefaultControlMinWidth = 125f;

    /// <summary>
    /// Display items for the dropdown, in index order.
    /// </summary>
    public abstract IReadOnlyList<string> Items { get; }

    /// <summary>
    /// Minimum width of the rendered dropdown control.
    /// </summary>
    public virtual float ControlMinWidth => DefaultControlMinWidth;

    /// <summary>
    /// Default selected index used when first created.
    /// </summary>
    public virtual int DefaultValue => 0;

    /// <summary>
    /// Reads the current selected item index from your game settings source.
    /// </summary>
    public abstract int GetValue();

    /// <summary>
    /// Writes the selected item index back to your game settings source.
    /// </summary>
    public abstract void SetValue(int value);
}
