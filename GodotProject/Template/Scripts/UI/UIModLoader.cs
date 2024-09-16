using Godot;
using GodotUtils;
using System.Collections.Generic;
using System.Diagnostics;

namespace Template;

public partial class UIModLoader : Node
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
        Node uiMods = GetNode("%Mods");
        
        _uiName = GetNode<Label>("%Name");
        _uiModVersion = GetNode<Label>("%ModVersion");
        _uiGameVersion = GetNode<Label>("%GameVersion");
        _uiDependencies = GetNode<Label>("%Dependencies");
        _uiDescription = GetNode<Label>("%Description");
        _uiAuthors = GetNode<Label>("%Authors");
        _uiIncompatibilities = GetNode<Label>("%Incompatibilities");

        Dictionary<string, ModInfo> mods = Global.Services.Get<ModLoader>().Mods;

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
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Game.SwitchScene(Scene.UIMainMenu);
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
        await GetNode<Global>("/root/Global").QuitAndCleanup();
    }

    private void _on_open_mods_folder_pressed()
    {
        Process.Start(new ProcessStartInfo(@$"{ProjectSettings.GlobalizePath("res://Mods")}") { UseShellExecute = true });
    }
}

