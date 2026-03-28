using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace __TEMPLATE__.Ui;

public partial class ModLoader : Node, ISceneDependencyReceiver
{
    // Nodes
    private Label _uiName = null!;
    private Label _uiModVersion = null!;
    private Label _uiGameVersion = null!;
    private Label _uiDependencies = null!;
    private Label _uiDescription = null!;
    private Label _uiAuthors = null!;
    private Label _uiIncompatibilities = null!;
    private SceneManager _sceneManager = null!;
    private IApplicationLifetime _applicationLifetime = null!;
    private ILoggerService _logger = null!;
    private Services _services = null!;
    private GameServices _runtimeServices = null!;
    private bool _isConfigured;

    public void Configure(GameServices services)
    {
        _runtimeServices = services;
        _sceneManager = services.SceneManager;
        _applicationLifetime = services.ApplicationLifetime;
        _logger = services.Logger;
        _services = services.ScopedServices;
        _isConfigured = true;
    }

    // Godot Overrides
    public override void _Ready()
    {
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(ModLoader)} was not configured before _Ready.");

        Node uiMods = GetNode<VBoxContainer>("%VBoxMods");

        _uiName = GetNode<Label>("%ModName");
        _uiModVersion = GetNode<Label>("%ModVersion");
        _uiGameVersion = GetNode<Label>("%GameVersion");
        _uiDependencies = GetNode<Label>("%Dependencies");
        _uiDescription = GetNode<Label>("%Description");
        _uiAuthors = GetNode<Label>("%Authors");
        _uiIncompatibilities = GetNode<Label>("%Incompatibilities");

        ModLoaderUi modLoaderUi = new(_logger, _services, _runtimeServices);
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
            _sceneManager.SwitchToMainMenu();
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

    private async void OnRestartGamePressed()
    {
        //OS.CreateProcess(OS.GetExecutablePath(), null);
        OS.CreateInstance(null);
        await _applicationLifetime.ExitGameAsync();
    }

    private static void OnOpenModsFolderPressed()
    {
        Process.Start(new ProcessStartInfo(@$"{ProjectSettings.GlobalizePath("res://Mods")}") { UseShellExecute = true });
    }
}
