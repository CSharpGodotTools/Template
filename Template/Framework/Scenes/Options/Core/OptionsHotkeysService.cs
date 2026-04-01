using Godot;
using GodotUtils;
using GodotUtils.RegEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FileAccess = Godot.FileAccess;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Manages hotkey defaults, load/save, and reset behavior.
/// </summary>
internal sealed class OptionsHotkeysService
{
    private const string PathHotkeys = "user://hotkeys.tres";

    private Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> _defaultHotkeys = null!;
    private ResourceHotkeys _hotkeys = null!;

    /// <summary>
    /// Gets the current hotkeys resource.
    /// </summary>
    public ResourceHotkeys Hotkeys => _hotkeys;

    /// <summary>
    /// Captures default input actions and loads persisted hotkeys.
    /// </summary>
    public void Initialize()
    {
        CaptureDefaultHotkeys();
        LoadHotkeys();
    }

    /// <summary>
    /// Saves current hotkeys resource to user storage.
    /// </summary>
    public void Save()
    {
        Error error = ResourceSaver.Save(_hotkeys, PathHotkeys);

        // Report save failures for troubleshooting user storage permissions.
        if (error != Error.Ok)
        {
            GD.Print($"Failed to save hotkeys: {error}");
        }
    }

    /// <summary>
    /// Restores hotkeys to default input-map values.
    /// </summary>
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

    /// <summary>
    /// Replaces current InputMap actions with provided hotkeys mapping.
    /// </summary>
    /// <param name="hotkeys">Action-to-input-event mapping to apply.</param>
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

    /// <summary>
    /// Captures the current project InputMap as hotkey defaults.
    /// </summary>
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

    /// <summary>
    /// Loads saved hotkeys, validating resource compatibility with current scripts.
    /// </summary>
    private void LoadHotkeys()
    {
        // Load and validate persisted hotkeys when user file exists.
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
    /// <summary>
    /// Validates and updates stored script paths in saved hotkey resources.
    /// </summary>
    /// <param name="localUserPath">User-local path to the hotkeys resource.</param>
    /// <param name="localResPath">Expected script path in project resources.</param>
    private static void ValidateResourceFile(string localUserPath, string localResPath)
    {
        string userGlobalPath = ProjectSettings.GlobalizePath(localUserPath);
        string content = File.ReadAllText(userGlobalPath);

        Match match = RegexUtils.ScriptPath().Match(content);

        // Abort path rewrite when no script path token is found.
        if (!match.Success)
        {
            GD.PrintErr($"Script path not found in {localUserPath}");
            return;
        }

        string currentPath = match.Value;

        // Skip rewrite when stored path already matches current resource path.
        if (currentPath == localResPath)
            return;

        string updatedContent = RegexUtils.ScriptPath().Replace(content, localResPath);
        File.WriteAllText(userGlobalPath, updatedContent);

        GD.Print($"Script path in {Path.GetFileName(userGlobalPath)} was invalid and has been readjusted to: {localResPath}");
    }

    /// <summary>
    /// Compares action dictionaries for structural and event equality.
    /// </summary>
    /// <param name="dict1">First action dictionary.</param>
    /// <param name="dict2">Second action dictionary.</param>
    /// <returns><see langword="true"/> when dictionaries are equivalent.</returns>
    private static bool ActionsAreEqual(
        Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> dict1,
        Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> dict2)
    {
        // Dictionaries with different key counts cannot be equal.
        if (dict1.Count != dict2.Count)
            return false;

        foreach (KeyValuePair<StringName, Godot.Collections.Array<InputEvent>> pair in dict1)
        {
            // Missing action key on right-hand side means mismatch.
            if (!dict2.TryGetValue(pair.Key, out Godot.Collections.Array<InputEvent>? dict2Events))
                return false;

            // Compare event arrays for each shared action key.
            if (!InputEventsAreEqual(pair.Value, dict2Events))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Compares two input-event arrays using textual event representation.
    /// </summary>
    /// <param name="events1">First event array.</param>
    /// <param name="events2">Second event array.</param>
    /// <returns><see langword="true"/> when all events match in order.</returns>
    private static bool InputEventsAreEqual(Godot.Collections.Array<InputEvent> events1, Godot.Collections.Array<InputEvent> events2)
    {
        // Arrays with different lengths cannot be equal.
        if (events1.Count != events2.Count)
            return false;

        for (int index = 0; index < events1.Count; index++)
        {
            string event1 = events1[index].AsText();
            string event2 = events2[index].AsText();

            // Compare canonical event text to avoid reference-based mismatches.
            if (!event1.Equals(event2, StringComparison.Ordinal))
                return false;
        }

        return true;
    }
}
