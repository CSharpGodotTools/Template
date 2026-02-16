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
    public static bool IsExportedRelease()
    {
        return OS.HasFeature("template");
    }

    /// <summary>
    /// Returns true when running inside the editor.
    /// </summary>
    public static bool IsEditor()
    {
        return !IsExportedRelease();
    }

    /// <summary>
    /// Returns true when running on Windows.
    /// </summary>
    public static bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    /// <summary>
    /// Returns true when running on Linux.
    /// </summary>
    public static bool IsLinux()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    /// <summary>
    /// Returns true when running on macOS.
    /// </summary>
    public static bool IsMac()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}
