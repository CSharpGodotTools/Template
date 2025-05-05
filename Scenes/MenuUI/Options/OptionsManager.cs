using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Template.UI;

namespace Template;

[GlobalClass]
public partial class OptionsManager : Resource
{
    public event Action<WindowMode> WindowModeChanged;

    public Dictionary<StringName, Array<InputEvent>> DefaultHotkeys { get; set; }
    public ResourceHotkeys Hotkeys { get; private set; }
    public ResourceOptions Options { get; private set; }
    public string CurrentOptionsTab { get; set; } = "General";

    public OptionsManager()
    {
        LoadOptions();

        GetDefaultHotkeys();
        LoadHotkeys();

        SetWindowMode();
        SetVSyncMode();
        SetWinSize();
        SetMaxFPS();
        SetLanguage();
        SetAntialiasing();
    }

    public void ToggleFullscreen()
    {
        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed)
        {
            SwitchToFullscreen();
        }
        else
        {
            SwitchToWindow();
        }
    }

    public void SaveOptions()
    {
        SaveResource(Options, nameof(Options));
    }

    public void SaveHotkeys()
    {
        SaveResource(Hotkeys, nameof(Hotkeys));
    }

    private static void SaveResource(Resource resource, string name)
    {
        Error error = ResourceSaver.Save(
            resource: resource,
            path: $"user://{name.ToLower()}.tres");

        if (error != Error.Ok)
        {
            GD.Print(error);
        }
    }

    public void ResetHotkeys()
    {
        // Deep clone default hotkeys over
        Hotkeys.Actions = [];

        foreach (System.Collections.Generic.KeyValuePair<StringName, Array<InputEvent>> element in DefaultHotkeys)
        {
            Array<InputEvent> arr = [];

            foreach (InputEvent item in DefaultHotkeys[element.Key])
            {
                arr.Add((InputEvent)item.Duplicate());
            }

            Hotkeys.Actions.Add(element.Key, arr);
        }

        // Set input map
        LoadInputMap(DefaultHotkeys);
    }

    private void LoadOptions()
    {
        bool fileExists = FileAccess.FileExists("user://options.tres");

        if (!fileExists)
        {
            Options = new();
            return;
        }

        // The options.tres file will have a line that looks like this.
        // [ext_resource type="Script" path="res://Scripts/Resources/ResourceOptions.cs" id="1_xn0b0"]
        // If the script is the wrong spot then the options.tres will fail to load.
        // Lets verify that the file is not corrupt by checking the path to the script is valid.
        string text = System.IO.File.ReadAllText(ProjectSettings.GlobalizePath("user://options.tres"));

        Regex regex = GetResourcePathRegex();
        Match match = regex.Match(text);
        string path = match.Groups["path"].Value;

        // Options file is corrupt
        if (!FileAccess.FileExists(path))
        {
            GD.Print("Options file is corrupt. Deleting and creating a new one!");
            System.IO.File.Delete(ProjectSettings.GlobalizePath("user://options.tres"));
            Options = new();
            return;
        }

        Options = GD.Load<ResourceOptions>("user://options.tres");
    }

    private static void LoadInputMap(Dictionary<StringName, Array<InputEvent>> hotkeys)
    {
        Array<StringName> actions = InputMap.GetActions();

        foreach (StringName action in actions)
        {
            InputMap.EraseAction(action);
        }

        foreach (StringName action in hotkeys.Keys)
        {
            InputMap.AddAction(action);

            foreach (InputEvent @event in hotkeys[action])
            {
                InputMap.ActionAddEvent(action, @event);
            }
        }
    }

    private void GetDefaultHotkeys()
    {
        // Get all the default actions defined in the input map
        Dictionary<StringName, Array<InputEvent>> actions = [];

        foreach (StringName action in InputMap.GetActions())
        {
            actions.Add(action, []);

            foreach (InputEvent actionEvent in InputMap.ActionGetEvents(action))
            {
                actions[action].Add(actionEvent);
            }
        }

        DefaultHotkeys = actions;
    }

    private void LoadHotkeys()
    {
        bool fileExists = FileAccess.FileExists("user://hotkeys.tres");

        if (fileExists)
        {
            Hotkeys = GD.Load<ResourceHotkeys>("user://hotkeys.tres");

            // InputMap in project settings has changed so reset all saved hotkeys
            if (!ActionsAreEqual(DefaultHotkeys, Hotkeys.Actions))
            {
                Hotkeys = new();
                ResetHotkeys();
            }

            LoadInputMap(Hotkeys.Actions);
        }
        else
        {
            Hotkeys = new();
            ResetHotkeys();
        }
    }

    private static bool ActionsAreEqual(Dictionary<StringName, Array<InputEvent>> dict1,
                         Dictionary<StringName, Array<InputEvent>> dict2)
    {
        return dict1.Count == dict2.Count &&
        dict1.All(pair => dict2.ContainsKey(pair.Key));
    }

    private void SetWindowMode()
    {
        switch (Options.WindowMode)
        {
            case WindowMode.Windowed:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                break;
            case WindowMode.Borderless:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                break;
            case WindowMode.Fullscreen:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                break;
        }
    }

    private void SwitchToFullscreen()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
        Options.WindowMode = WindowMode.Fullscreen;
        WindowModeChanged?.Invoke(WindowMode.Fullscreen);
    }

    private void SwitchToWindow()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        Options.WindowMode = WindowMode.Windowed;
        WindowModeChanged?.Invoke(WindowMode.Windowed);
    }

    private void SetVSyncMode()
    {
        DisplayServer.WindowSetVsyncMode(Options.VSyncMode);
    }

    private void SetWinSize()
    {
        if (Options.WindowSize != Vector2I.Zero)
        {
            DisplayServer.WindowSetSize(Options.WindowSize);

            // center window
            Vector2I screenSize = DisplayServer.ScreenGetSize();
            Vector2I winSize = DisplayServer.WindowGetSize();
            DisplayServer.WindowSetPosition(screenSize / 2 - winSize / 2);
        }
    }

    private void SetMaxFPS()
    {
        if (DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Disabled)
        {
            Engine.MaxFps = Options.MaxFPS;
        }
    }

    private void SetLanguage()
    {
        TranslationServer.SetLocale(
        Options.Language.ToString().Substring(0, 2).ToLower());
    }

    private void SetAntialiasing()
    {
        // Set both 2D and 3D settings to the same value
        ProjectSettings.SetSetting(
            name: "rendering/anti_aliasing/quality/msaa_2d", 
            value: Options.Antialiasing);

        ProjectSettings.SetSetting(
            name: "rendering/anti_aliasing/quality/msaa_3d",
            value: Options.Antialiasing);
    }

    [GeneratedRegex("path=\"(?<path>.*?)\"")]
    private static partial Regex GetResourcePathRegex();
}

