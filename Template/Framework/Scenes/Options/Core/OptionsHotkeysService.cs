using Godot;
using GodotUtils;
using GodotUtils.RegEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FileAccess = Godot.FileAccess;

namespace Framework.Ui;

/// <summary>
/// Manages hotkey defaults, load/save, and reset behavior.
/// </summary>
internal sealed class OptionsHotkeysService
{
    private const string PathHotkeys = "user://hotkeys.tres";

    private Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> _defaultHotkeys;
    private ResourceHotkeys _hotkeys;

    public ResourceHotkeys Hotkeys => _hotkeys;

    public void Initialize()
    {
        CaptureDefaultHotkeys();
        LoadHotkeys();
    }

    public void Save()
    {
        Error error = ResourceSaver.Save(_hotkeys, PathHotkeys);

        if (error != Error.Ok)
        {
            GD.Print($"Failed to save hotkeys: {error}");
        }
    }

    public void ResetToDefaults()
    {
        // Deep clone default hotkeys over.
        _hotkeys.Actions = [];

        foreach (KeyValuePair<StringName, Godot.Collections.Array<InputEvent>> element in _defaultHotkeys)
        {
            Godot.Collections.Array<InputEvent> clonedEvents = [];

            foreach (InputEvent item in _defaultHotkeys[element.Key])
            {
                clonedEvents.Add((InputEvent)item.Duplicate());
            }

            _hotkeys.Actions.Add(element.Key, clonedEvents);
        }

        ApplyInputMap(_defaultHotkeys);
    }

    private static void ApplyInputMap(Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> hotkeys)
    {
        Godot.Collections.Array<StringName> actions = InputMap.GetActions();

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

    private void CaptureDefaultHotkeys()
    {
        // Snapshot default actions from project input map.
        Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> actions = [];

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
            ValidateResourceFile(PathHotkeys, localResPath);
            _hotkeys = GD.Load<ResourceHotkeys>(PathHotkeys);

            // InputMap in project settings changed: reset stale saved hotkeys.
            if (!ActionsAreEqual(_defaultHotkeys, _hotkeys.Actions))
            {
                _hotkeys = new();
                ResetToDefaults();
            }

            ApplyInputMap(_hotkeys.Actions);
            return;
        }

        _hotkeys = new();
        ResetToDefaults();
    }

    // *.tres files store script paths. If scripts are moved, fix outdated path on load.
    private static void ValidateResourceFile(string localUserPath, string localResPath)
    {
        string userGlobalPath = ProjectSettings.GlobalizePath(localUserPath);
        string content = File.ReadAllText(userGlobalPath);

        Match match = RegexUtils.ScriptPath().Match(content);
        if (!match.Success)
        {
            GD.PrintErr($"Script path not found in {localUserPath}");
            return;
        }

        string currentPath = match.Value;

        if (currentPath == localResPath)
            return;

        string updatedContent = RegexUtils.ScriptPath().Replace(content, localResPath);
        File.WriteAllText(userGlobalPath, updatedContent);

        GD.Print($"Script path in {Path.GetFileName(userGlobalPath)} was invalid and has been readjusted to: {localResPath}");
    }

    private static bool ActionsAreEqual(
        Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> dict1,
        Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> dict2)
    {
        if (dict1.Count != dict2.Count)
            return false;

        foreach (KeyValuePair<StringName, Godot.Collections.Array<InputEvent>> pair in dict1)
        {
            if (!dict2.TryGetValue(pair.Key, out Godot.Collections.Array<InputEvent> dict2Events))
                return false;

            if (!InputEventsAreEqual(pair.Value, dict2Events))
                return false;
        }

        return true;
    }

    private static bool InputEventsAreEqual(Godot.Collections.Array<InputEvent> events1, Godot.Collections.Array<InputEvent> events2)
    {
        if (events1.Count != events2.Count)
            return false;

        for (int index = 0; index < events1.Count; index++)
        {
            string event1 = events1[index].AsText();
            string event2 = events2[index].AsText();
            if (!event1.Equals(event2, StringComparison.Ordinal))
                return false;
        }

        return true;
    }
}
