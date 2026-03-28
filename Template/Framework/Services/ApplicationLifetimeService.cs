using System;
using System.Threading.Tasks;

namespace __TEMPLATE__;

internal sealed class ApplicationLifetimeService(AutoloadsFramework autoloads) : IApplicationLifetime
{
    private readonly AutoloadsFramework _autoloads = autoloads;

    public event Func<Task>? PreQuit
    {
        add => _autoloads.PreQuit += value;
        remove => _autoloads.PreQuit -= value;
    }

    public Task ExitGameAsync()
    {
        return _autoloads.ExitGame();
    }
}
