using System.Runtime.CompilerServices;

namespace __TEMPLATE__;

public interface IProfiler
{
    /// <summary>
    /// Starts timing for a call site and creates its entry on first use.
    /// </summary>
    void Begin(string id = "", [CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "");

    /// <summary>
    /// Stops timing for a call site if it is being tracked.
    /// </summary>
    void End(string id = "", [CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "");

    /// <summary>
    /// Prints the formatted profiling summary to the Godot console.
    /// </summary>
    void Summary();
}
