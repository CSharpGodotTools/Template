using Godot;
using Godot.Collections;
using GodotUtils.RegEx;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileAccess = Godot.FileAccess;

namespace GodotUtils.UI;

// Autoload
public partial class OptionsManager : IDisposable
{
    public event Action<WindowMode> WindowModeChanged;

    public static OptionsManager Instance { get; private set; }

    private const string PathOptions = "user://options.json";
    private const string PathHotkeys = "user://hotkeys.tres";

    private Dictionary<StringName, Array<InputEvent>> _defaultHotkeys;
    private JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private ResourceOptions _options;
    private ResourceHotkeys _hotkeys;
    private string _currentOptionsTab = "General";
    private Global _global;

    public OptionsManager(Global global)
    {
        if (Instance != null)
            throw new InvalidOperationException($"{nameof(OptionsManager)} was initialized already");

        Instance = this;
        _global = global;
        _global.PreQuit += SaveSettingsOnQuit;

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

    public void Dispose()
    {
        _global.PreQuit -= SaveSettingsOnQuit;

        WindowModeChanged = null;
        Instance = null;
    }

    public void Update()
    {
        if (Input.IsActionJustPressed(InputActions.Fullscreen))
        {
            ToggleFullscreen();
        }
    }

    public static string GetCurrentTab()
    {
        return Instance._currentOptionsTab;
    }

    public static void SetCurrentTab(string tab)
    {
        Instance._currentOptionsTab = tab;
    }

    public static ResourceOptions GetOptions()
    {
        return Instance._options;
    }

    public static ResourceHotkeys GetHotkeys()
    {
        return Instance._hotkeys;
    }

    private void ToggleFullscreen()
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

    private void SaveOptions()
    {
        string json = JsonSerializer.Serialize(_options, _jsonOptions);

        FileAccess file = FileAccess.Open(PathOptions, FileAccess.ModeFlags.Write);

        file.StoreString(json);
        file.Close();
    }

    private void SaveHotkeys()
    {
        Error error = ResourceSaver.Save(_hotkeys, PathHotkeys);

        if (error != Error.Ok)
        {
            GD.Print($"Failed to save hotkeys: {error}");
        }
    }

    public static void ResetHotkeys()
    {
        // Deep clone default hotkeys over
        Instance._hotkeys.Actions = [];

        foreach (System.Collections.Generic.KeyValuePair<StringName, Array<InputEvent>> element in Instance._defaultHotkeys)
        {
            Array<InputEvent> arr = [];

            foreach (InputEvent item in Instance._defaultHotkeys[element.Key])
            {
                arr.Add((InputEvent)item.Duplicate());
            }

            Instance._hotkeys.Actions.Add(element.Key, arr);
        }

        // Set input map
        LoadInputMap(Instance._defaultHotkeys);
    }

    private void LoadOptions()
    {
        if (FileAccess.FileExists(PathOptions))
        {
            FileAccess file = FileAccess.Open(PathOptions, FileAccess.ModeFlags.Read);

            _options = JsonSerializer.Deserialize<ResourceOptions>(file.GetAsText());

            file.Close();
        }
        else
        {
            _options = new();
        }
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

        _defaultHotkeys = actions;
    }

    private void LoadHotkeys()
    {
        if (FileAccess.FileExists(PathHotkeys))
        {
            string localResPath = ProjectSettings.LocalizePath(DirectoryUtils.FindFile("res://", "ResourceHotkeys.cs"));
            ValdiateResourceFile(PathHotkeys, localResPath);
            _hotkeys = GD.Load<ResourceHotkeys>(PathHotkeys);

            // InputMap in project settings has changed so reset all saved hotkeys
            if (!ActionsAreEqual(_defaultHotkeys, _hotkeys.Actions))
            {
                _hotkeys = new();
                ResetHotkeys();
            }

            LoadInputMap(_hotkeys.Actions);
        }
        else
        {
            _hotkeys = new();
            ResetHotkeys();
        }
    }

    // *.tres files store the path to their script in res:// and as a result if that script is moved then the
    // path in *.tres will point to an invalid path and so this function corrects the path again.
    private void ValdiateResourceFile(string localUserPath, string localResPath)
    {
        string userGlobalPath = ProjectSettings.GlobalizePath(localUserPath);
        string content = File.ReadAllText(userGlobalPath);

        // Find current path in the resource file
        Match match = RegexUtils.ScriptPath().Match(content);

        if (!match.Success)
        {
            GD.PrintErr($"Script path not found in {localUserPath}");
            return;
        }

        string currentPath = match.Value;

        if (currentPath == localResPath)
            return; // Resource path is correct. No update needed.

        // Path is incorrect, proceed to rewrite.
        string updatedContent = RegexUtils.ScriptPath().Replace(content, localResPath);

        File.WriteAllText(userGlobalPath, updatedContent);

        GD.Print($"Script path in {Path.GetFileName(userGlobalPath)} in was invalid and has been readjusted to the proper path: {localResPath}");
    }

    private static bool ActionsAreEqual(Dictionary<StringName, Array<InputEvent>> dict1, Dictionary<StringName, Array<InputEvent>> dict2)
    {
        return dict1.Count == dict2.Count && dict1.All(pair => dict2.ContainsKey(pair.Key));
    }

    private void SetWindowMode()
    {
        switch (_options.WindowMode)
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
        _options.WindowMode = WindowMode.Fullscreen;
        WindowModeChanged?.Invoke(WindowMode.Fullscreen);
    }

    private void SwitchToWindow()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        _options.WindowMode = WindowMode.Windowed;
        WindowModeChanged?.Invoke(WindowMode.Windowed);
    }

    private void SetVSyncMode()
    {
        DisplayServer.WindowSetVsyncMode(_options.VSyncMode);
    }

    private void SetWinSize()
    {
        Vector2I windowSize = new(_options.WindowWidth, _options.WindowHeight);

        if (windowSize != Vector2I.Zero)
        {
            DisplayServer.WindowSetSize(windowSize);

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
            Engine.MaxFps = _options.MaxFPS;
        }
    }

    private void SetLanguage()
    {
        TranslationServer.SetLocale(
        _options.Language.ToString().Substring(0, 2).ToLower());
    }

    private void SetAntialiasing()
    {
        // Set both 2D and 3D settings to the same value
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_2d", _options.Antialiasing);
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_3d", _options.Antialiasing);
    }

    private Task SaveSettingsOnQuit()
    {
        SaveOptions();
        SaveHotkeys();

        return Task.CompletedTask;
    }
}
