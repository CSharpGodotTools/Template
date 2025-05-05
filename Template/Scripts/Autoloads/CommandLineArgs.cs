using Godot;
using System.Collections.Generic;

namespace Template.UI;

/// <summary>
/// Handles custom command line arguments set for each instance.
/// </summary>
public partial class CommandLineArgs : Node
{
    public override void _Ready()
    {
        // Get command-line arguments
        string[] args = OS.GetCmdlineArgs();

        Dictionary<string, WindowSettings> windowSettings = GetWindowSettingsMap();

        // Loop through arguments to find the position argument
        foreach (string arg in args)
        {
            if (windowSettings.TryGetValue(arg, out WindowSettings settings))
            {
                // Set the window size
                DisplayServer.WindowSetSize(settings.Size);

                // Set the window position
                DisplayServer.WindowSetPosition(settings.Position);
                break;
            }
        }
    }

    private static Dictionary<string, WindowSettings> GetWindowSettingsMap()
    {
        // Define the vertical space for the window bar
        int windowBarHeight = 30; // Adjust this value based on your actual window bar height

        // Get screen size
        Vector2 screenSize = DisplayServer.ScreenGetSize();

        return new Dictionary<string, WindowSettings>()
        {
            {
                "top_left",
                new WindowSettings(
                    new Vector2I(0, windowBarHeight),
                    new Vector2I((int)(screenSize.X / 2), (int)((screenSize.Y - windowBarHeight) / 2)))
            },
            {
                "top_right",
                new WindowSettings(
                    new Vector2I((int)(screenSize.X / 2), windowBarHeight),
                    new Vector2I((int)(screenSize.X / 2), (int)((screenSize.Y - windowBarHeight) / 2)))
            },
            {
                "bottom_left",
                new WindowSettings(
                    new Vector2I(0, (int)(screenSize.Y / 2) + windowBarHeight),
                    new Vector2I((int)(screenSize.X / 2), (int)((screenSize.Y - windowBarHeight) / 2)))
            },
            {
                "bottom_right",
                new WindowSettings(
                    new Vector2I((int)(screenSize.X / 2), (int)(screenSize.Y / 2) + windowBarHeight),
                    new Vector2I((int)(screenSize.X / 2), (int)((screenSize.Y - windowBarHeight) / 2)))
            },
            {
                "middle_left",
                new WindowSettings(
                    new Vector2I(0, windowBarHeight),
                    new Vector2I((int)(screenSize.X / 2), (int)(screenSize.Y - windowBarHeight)))
            },
            {
                "middle_right",
                new WindowSettings(
                    new Vector2I((int)(screenSize.X / 2), windowBarHeight),
                    new Vector2I((int)(screenSize.X / 2), (int)(screenSize.Y - windowBarHeight)))
            },
            {
                "middle_top",
                new WindowSettings(
                    new Vector2I(0, windowBarHeight),
                    new Vector2I((int)screenSize.X, (int)((screenSize.Y - windowBarHeight) / 2)))
            },
            {
                "middle_bottom",
                new WindowSettings(
                    new Vector2I(0, (int)(screenSize.Y / 2) + windowBarHeight),
                    new Vector2I((int)screenSize.X, (int)((screenSize.Y - windowBarHeight) / 2)))
            }
        };
    }

    private class WindowSettings(Vector2I position, Vector2I size)
    {
        public Vector2I Position { get; } = position;
        public Vector2I Size { get; } = size;
    }
}
