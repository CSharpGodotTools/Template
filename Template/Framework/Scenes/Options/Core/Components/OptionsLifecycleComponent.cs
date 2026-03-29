using System;
using System.Threading.Tasks;

namespace __TEMPLATE__.Ui;

internal sealed class OptionsLifecycleComponent : IDisposable
{
    private readonly AutoloadsFramework _autoloads;
    private readonly OptionsValueStoreComponent _valueStore;
    private readonly OptionsHotkeysService _hotkeysService;
    private readonly OptionsDisplaySettingsComponent _displaySettings;

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

    public void Dispose()
    {
        _autoloads.PreQuit -= SaveSettingsOnQuit;
        _autoloads.GetTree().Root.SizeChanged -= OnWindowResized;
    }

    private void OnWindowResized()
    {
        _displaySettings.PersistWindowSizeFromRuntime();
    }

    private Task SaveSettingsOnQuit()
    {
        _valueStore.Save();
        _hotkeysService.Save();
        return Task.CompletedTask;
    }
}
