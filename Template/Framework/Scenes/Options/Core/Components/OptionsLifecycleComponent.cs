using System;
using System.Threading.Tasks;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Coordinates options persistence across runtime lifecycle events.
/// </summary>
internal sealed class OptionsLifecycleComponent : IDisposable
{
    private readonly AutoloadsFramework _autoloads;
    private readonly OptionsValueStoreComponent _valueStore;
    private readonly OptionsHotkeysService _hotkeysService;
    private readonly OptionsDisplaySettingsComponent _displaySettings;

    /// <summary>
    /// Subscribes lifecycle hooks for window resize and pre-quit persistence.
    /// </summary>
    /// <param name="autoloads">Autoload access for app lifecycle events.</param>
    /// <param name="valueStore">Persistent options store.</param>
    /// <param name="hotkeysService">Hotkey persistence service.</param>
    /// <param name="displaySettings">Display settings component.</param>
    public OptionsLifecycleComponent(
        AutoloadsFramework autoloads,
        OptionsValueStoreComponent valueStore,
        OptionsHotkeysService hotkeysService,
        OptionsDisplaySettingsComponent displaySettings)
    {
        _autoloads = autoloads;
        _valueStore = valueStore;
        _hotkeysService = hotkeysService;
        _displaySettings = displaySettings;

        _autoloads.PreQuit += SaveSettingsOnQuit;
        _autoloads.GetTree().Root.SizeChanged += OnWindowResized;
    }

    /// <summary>
    /// Unsubscribes lifecycle hooks established by this component.
    /// </summary>
    public void Dispose()
    {
        _autoloads.PreQuit -= SaveSettingsOnQuit;
        _autoloads.GetTree().Root.SizeChanged -= OnWindowResized;
    }

    /// <summary>
    /// Persists runtime window size into settings during resize events.
    /// </summary>
    private void OnWindowResized()
    {
        _displaySettings.PersistWindowSizeFromRuntime();
    }

    /// <summary>
    /// Saves options and hotkey bindings before application shutdown.
    /// </summary>
    /// <returns>A completed task for the pre-quit callback contract.</returns>
    private Task SaveSettingsOnQuit()
    {
        _valueStore.Save();
        _hotkeysService.Save();
        return Task.CompletedTask;
    }
}
