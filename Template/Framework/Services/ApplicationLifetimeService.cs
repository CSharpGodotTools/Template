using System;
using System.Threading.Tasks;

namespace __TEMPLATE__;

/// <summary>
/// Adapts app-lifetime operations to the autoload framework implementation.
/// </summary>
/// <param name="autoloads">Autoload framework instance that owns quit flow.</param>
internal sealed class ApplicationLifetimeService(AutoloadsFramework autoloads) : IApplicationLifetime
{
    private readonly AutoloadsFramework _autoloads = autoloads;

    /// <summary>
    /// Raised before the application begins shutdown.
    /// </summary>
    public event Func<Task>? PreQuit
    {
        add => _autoloads.PreQuit += value;
        remove => _autoloads.PreQuit -= value;
    }

    /// <summary>
    /// Starts the async game-exit flow.
    /// </summary>
    /// <returns>Task that completes when exit flow finishes.</returns>
    public Task ExitGameAsync()
    {
        return _autoloads.ExitGame();
    }
}
