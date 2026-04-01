using Godot;
using System.Runtime.InteropServices;

namespace GodotUtils;

/// <summary>
/// Helpers for editor and platform checks.
/// </summary>
public static class EditorUtils
{
    /// <summary>
    /// Returns true when running an exported release build.
    /// </summary>
    /// <returns><see langword="true"/> when the runtime is an exported template build.</returns>
    public static bool IsExportedRelease()
    {
        return OS.HasFeature("template");
    }

    /// <summary>
    /// Returns true when running inside the editor.
    /// </summary>
    /// <returns><see langword="true"/> when running in the Godot editor.</returns>
    public static bool IsEditor()
    {
        return !IsExportedRelease();
    }

    /// <summary>
    /// Returns true when running on Windows.
    /// </summary>
    /// <returns><see langword="true"/> on Windows platforms.</returns>
    public static bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    /// <summary>
    /// Returns true when running on Linux.
    /// </summary>
    /// <returns><see langword="true"/> on Linux platforms.</returns>
    public static bool IsLinux()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    /// <summary>
    /// Returns true when running on macOS.
    /// </summary>
    /// <returns><see langword="true"/> on macOS platforms.</returns>
    public static bool IsMac()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}
