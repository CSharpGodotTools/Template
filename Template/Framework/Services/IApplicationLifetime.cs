using System;
using System.Threading.Tasks;

namespace __TEMPLATE__;

/// <summary>
/// Defines application shutdown flow operations.
/// </summary>
public interface IApplicationLifetime
{
    /// <summary>
    /// Raised before application shutdown begins.
    /// </summary>
    event Func<Task>? PreQuit;

    /// <summary>
    /// Starts the async application exit flow.
    /// </summary>
    /// <returns>Task that completes when exit flow finishes.</returns>
    Task ExitGameAsync();
}
