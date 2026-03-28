using Godot;
using GodotUtils;
using System.Collections.Generic;
using System.Diagnostics;

namespace __TEMPLATE__.Ui;

public partial class ModLoader : Node
{
    // Nodes
    private Label _uiName = null!;
    private Label _uiModVersion = null!;
    private Label _uiGameVersion = null!;
    private Label _uiDependencies = null!;
    private Label _uiDescription = null!;
    private Label _uiAuthors = null!;
    private Label _uiIncompatibilities = null!;

    // Godot Overrides
    public override void _Ready()
    {
        Node uiMods = GetNode<VBoxContainer>("%VBoxMods");

        _uiName = GetNode<Label>("%ModName");
        _uiModVersion = GetNode<Label>("%ModVersion");
        _uiGameVersion = GetNode<Label>("%GameVersion");
        _uiDependencies = GetNode<Label>("%Dependencies");
        _uiDescription = GetNode<Label>("%Description");
        _uiAuthors = GetNode<Label>("%Authors");
        _uiIncompatibilities = GetNode<Label>("%Incompatibilities");

        ModLoaderUi modLoaderUi = new();
        modLoaderUi.LoadMods(this);
        Dictionary<string, ModInfo> mods = modLoaderUi.GetMods();

        bool first = true;

        foreach (ModInfo modInfo in mods.Values)
        {
            Button btn = new()
            {
                ToggleMode = true,
                Text = modInfo.Name
            };

            void OnPressed()
            {
                DisplayModInfo(modInfo);
            }

            void OnExitedTree()
            {
                btn.Pressed -= OnPressed;
                btn.TreeExited -= OnExitedTree;
            }

            btn.Pressed += OnPressed;
            btn.TreeExited += OnExitedTree;

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
            Game.SceneManager.SwitchToMainMenu();
        }
    }

    // Private Methods
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

    private async static void OnRestartGamePressed()
    {
        //OS.CreateProcess(OS.GetExecutablePath(), null);
        OS.CreateInstance(null);
        await Game.Application.ExitGameAsync();
    }

    private static void OnOpenModsFolderPressed()
    {
        Process.Start(new ProcessStartInfo(@$"{ProjectSettings.GlobalizePath("res://Mods")}") { UseShellExecute = true });
    }
}
