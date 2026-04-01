using __TEMPLATE__.Ui;
using GodotUtils;
using System;

namespace __TEMPLATE__;

/// <summary>
/// Defines options/settings and hotkey management operations.
/// </summary>
public interface IOptionsService
{
    /// <summary>
    /// Raised when window mode changes.
    /// </summary>
    event Action<WindowMode> WindowModeChanged;

    /// <summary>
    /// Current options settings instance.
    /// </summary>
    OptionsSettings Settings { get; }

    /// <summary>
    /// Gets current options tab identifier.
    /// </summary>
    /// <returns>Current tab key.</returns>
    string GetCurrentTab();

    /// <summary>
    /// Sets current options tab identifier.
    /// </summary>
    /// <param name="tab">Tab key to select.</param>
    void SetCurrentTab(string tab);

    /// <summary>
    /// Returns current hotkey resource.
    /// </summary>
    /// <returns>Hotkey resource.</returns>
    ResourceHotkeys GetHotkeys();

    /// <summary>
    /// Resets hotkeys to defaults.
    /// </summary>
    void ResetHotkeys();

    /// <summary>
    /// Adds an option definition to the options registry.
    /// </summary>
    /// <param name="option">Option definition to add.</param>
    void AddOption(OptionDefinition option);

    /// <summary>
    /// Adds a right-side control definition to options UI.
    /// </summary>
    /// <param name="definition">Right-control definition to add.</param>
    void AddRightControl(OptionRightControlDefinition definition);
}
