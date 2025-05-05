using Godot;
using GodotUtils;
using System.Collections.Generic;
using System.Diagnostics;

namespace Template.UI;

[SceneTree]
public partial class ModLoader : Node
{
    private Label _uiName;
    private Label _uiModVersion;
    private Label _uiGameVersion;
    private Label _uiDependencies;
    private Label _uiDescription;
    private Label _uiAuthors;
    private Label _uiIncompatibilities;

    public override void _Ready()
    {
        Node uiMods = VBoxMods;
        
        _uiName = ModName;
        _uiModVersion = ModVersion;
        _uiGameVersion = GameVersion;
        _uiDependencies = Dependencies;
        _uiDescription = Description;
        _uiAuthors = Authors;
        _uiIncompatibilities = Incompatibilities;

        Dictionary<string, ModInfo> mods = ModLoaderUI.Mods;

        bool first = true;

        foreach (ModInfo modInfo in mods.Values)
        {
            Button btn = new();
            btn.ToggleMode = true;
            btn.Text = modInfo.Name;
            btn.Pressed += () =>
            {
                DisplayModInfo(modInfo);
            };

            uiMods.AddChild(btn);

            if (first)
            {
                first = false;
                btn.GrabFocus();
                DisplayModInfo(modInfo);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.UICancel))
        {
            Game.SwitchScene(Scene.MainMenu);
        }
    }

    private void DisplayModInfo(ModInfo modInfo)
    {
        _uiName.Text = modInfo.Name;
        _uiModVersion.Text = modInfo.ModVersion;
        _uiGameVersion.Text = modInfo.GameVersion;

        _uiDependencies.Text = modInfo.Dependencies.Count != 0 ? 
            modInfo.Dependencies.ToFormattedString() : "None";

        _uiIncompatibilities.Text = modInfo.Incompatibilities.Count != 0 ? 
            modInfo.Incompatibilities.ToFormattedString() : "None";

        _uiDescription.Text = !string.IsNullOrWhiteSpace(modInfo.Description) ? 
            modInfo.Description : "The author did not set a description for this mod";

        _uiAuthors.Text = modInfo.Author;
    }

    private async void _on_restart_game_pressed()
    {
        //OS.CreateProcess(OS.GetExecutablePath(), null);
        OS.CreateInstance(null);
        await Global.QuitAndCleanup();
    }

    private static void _on_open_mods_folder_pressed()
    {
        Process.Start(new ProcessStartInfo(@$"{ProjectSettings.GlobalizePath("res://Mods")}") { UseShellExecute = true });
    }
}

