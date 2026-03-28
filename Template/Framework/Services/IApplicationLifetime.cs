using System;
using System.Threading.Tasks;

namespace __TEMPLATE__;

public interface IApplicationLifetime
{
    event Func<Task>? PreQuit;

    Task ExitGameAsync();
}
