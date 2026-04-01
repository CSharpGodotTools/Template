using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Builds the mod list UI and manages mod-loader scene actions.
/// </summary>
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

    /// <summary>
    /// Injects runtime services needed by this scene.
    /// </summary>
    /// <param name="services">Resolved scene services.</param>
    public void Configure(GameServices services)
    {
        _runtimeServices = services;
        _sceneManager = services.SceneManager;
        _applicationLifetime = services.ApplicationLifetime;
        _logger = services.Logger;
        _services = services.ScopedServices;
        _isConfigured = true;
    }

    public override void _EnterTree()
    {
        SceneComposition.ConfigureNodeFromGame(this);
    }

    // Godot Overrides
    public override void _Ready()
    {
        // Scene composition must inject dependencies before this node becomes ready.
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


            // Capture handlers so each dynamic button can clean itself up.
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

            // Select and display the first mod by default.
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
        // Return to main menu when the cancel action is pressed.
        if (Input.IsActionJustPressed(InputActions.UICancel))
        {
            _sceneManager.SwitchToMainMenu();
        }
    }

    // Private Methods
    /// <summary>
    /// Updates mod metadata labels with values from selected mod.
    /// </summary>
    /// <param name="modInfo">Selected mod metadata.</param>
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

    /// <summary>
    /// Handles restart button press and starts restart flow.
    /// </summary>
    private void OnRestartGamePressed()
    {
        _ = RestartGameAsync();
    }

    /// <summary>
    /// Starts a new game process instance and exits current process.
    /// </summary>
    /// <returns>A task that completes when exit has been requested.</returns>
    private async Task RestartGameAsync()
    {
        try
        {

            // Launch replacement instance first so restart feels instantaneous.
            //OS.CreateProcess(OS.GetExecutablePath(), null);
            OS.CreateInstance(null);
            await _applicationLifetime.ExitGameAsync();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            _logger.LogErr(exception, nameof(ModLoader));
        }
    }

    /// <summary>
    /// Opens the project Mods directory in the OS file explorer.
    /// </summary>
    private static void OnOpenModsFolderPressed()
    {
        Process.Start(new ProcessStartInfo(@$"{ProjectSettings.GlobalizePath("res://Mods")}") { UseShellExecute = true });
    }
}
