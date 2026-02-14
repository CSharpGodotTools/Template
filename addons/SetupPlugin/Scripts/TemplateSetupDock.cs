#if TOOLS
using Framework.Setup;
using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Framework.Setup;

[Tool]
public partial class TemplateSetupDock : VBoxContainer
{
    private const string SetupPluginName = "SetupPlugin";
    private const string MainSceneName = "Level";
    private const string DefaultClearColorPath = "rendering/environment/defaults/default_clear_color";
    private const int Padding = 120;
    private const string MainScenesPath = "res://addons/SetupPlugin/MainScenes/";

    Dictionary<string, List<string>> _sceneTypes = new();
    private ConfirmationDialog _confirmRestartDialog;
    private GameNameValidator _gameNameValidator;
    private ProjectSetup _projectSetup;
    private OptionButton _templateType;
    private OptionButton _projectType;
    private LineEdit _gameNameLineEdit;
    private VBoxContainer _vbox;
    private Label _gameNamePreview;
    private string _prevGameName = "";
    private string _projectTypeStr;
    private string _templateTypeStr;

    public override void _Ready()
    {
        // Restart dialog
        _confirmRestartDialog = new ConfirmationDialog
        {
            Title = "Setup Confirmation",
            DialogText = "Godot will restart with your changes. This cannot be undone",
            OkButtonText = "Yes",
            CancelButtonText = "No"
        };

        // Check if signal is actually connected. (Godot will show error on re-enabling this plugin if this check is not here)
        if (!_confirmRestartDialog.IsConnected(ConfirmationDialog.SignalName.Confirmed, new Callable(this, nameof(OnConfirmed))))
            _confirmRestartDialog.Connect(ConfirmationDialog.SignalName.Confirmed, new Callable(this, nameof(OnConfirmed)));

        // Feedback reset timer
        Timer feedbackResetTimer = new Timer();
        feedbackResetTimer.Connect(Timer.SignalName.Timeout, new Callable(this, nameof(OnFeedbackResetTimerTimeout)));

        // Game name preview
        _gameNamePreview = new Label()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(200, 0)
        };

        // Game name line edit
        _gameNameLineEdit = new LineEdit()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            CustomMinimumSize = new Vector2(200, 0)
        };
        _gameNameLineEdit.Connect(LineEdit.SignalName.TextChanged, new Callable(this, nameof(OnProjectNameChanged)));

        _projectType = new();

        DirectoryUtils.Traverse(ProjectSettings.GlobalizePath(MainScenesPath), templateTypeEntry =>
        {
            _sceneTypes[templateTypeEntry.FileName] = new List<string>();

            DirectoryUtils.Traverse(ProjectSettings.GlobalizePath(Path.Combine(MainScenesPath, templateTypeEntry.FileName)), entry =>
            {
                // Add specific template types "Minimal" and "FPS"
                _sceneTypes[templateTypeEntry.FileName].Add(entry.FileName);
                return TraverseDecision.SkipChildren;
            });

            // Add project types "2D" and "3D"
            _projectType.AddItem(templateTypeEntry.FileName);
            return TraverseDecision.SkipChildren;
        });

        var firstSetupType = _sceneTypes.First();
        _projectTypeStr = firstSetupType.Key; // e.g. "3D"
        _templateTypeStr = firstSetupType.Value[0]; // e.g. ["Minimal", "FPS"]

        _projectType.Select(0);
        _projectType.ItemSelected += OnProjectTypeSelected;

        _templateType = new OptionButton();

        foreach (string templateType in firstSetupType.Value)
            _templateType.AddItem(templateType);

        _templateType.Select(0);
        _templateType.ItemSelected += OnTemplateTypeSelected;

        // Default clear color
        ColorPickerButton defaultClearColorPicker = new()
        {
            CustomMinimumSize = new Vector2(75, 35),
            Color = ProjectSettings.GetSetting(DefaultClearColorPath).AsColor()
        };
        defaultClearColorPicker.Connect(ColorPickerButton.SignalName.ColorChanged, new Callable(this, nameof(OnDefaultClearColorChanged)));

        // Apply button
        Button applyButton = new Button()
        {
            Text = "Run Setup",
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(200, 0)
        };
        applyButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnApplyPressed)));

        // Validator and Setup
        _gameNameValidator = new GameNameValidator(_gameNamePreview, feedbackResetTimer, _gameNameLineEdit);
        _projectSetup = new ProjectSetup();

        // Layout
        MarginContainer margin = MarginContainerFactory.Create(30);
        _vbox = new VBoxContainer();

        AddChild(feedbackResetTimer);

        _vbox.AddChild(_gameNamePreview);

        AddLabelH("Project Name", _gameNameLineEdit);
        AddLabelH("Project", _projectType);
        AddLabelH("Template", _templateType);
        AddLabelH("Clear Color", defaultClearColorPicker);

        margin.AddChild(_vbox);

        AddChild(margin);
        AddChild(applyButton);

        EditorInterface.Singleton.GetEditorMainScreen().AddChild(_confirmRestartDialog);
    }

    private void AddLabelH(string text, Control control)
    {
        HBoxContainer hbox = new();
        hbox.AddChild(new Label
        {
            Text = $"{text}:",
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(Padding, 0)
        });
        hbox.AddChild(control);
        _vbox.AddChild(hbox);
    }

    private void OnProjectTypeSelected(long index)
    {
        _templateType.Clear();

        string projectType = _projectType.GetItemText((int)index);
        _projectTypeStr = projectType;

        foreach (string templateType in _sceneTypes[projectType])
        {
            _templateType.AddItem(templateType);
        }
    }

    private void OnTemplateTypeSelected(long index)
    {
        _templateTypeStr = _templateType.GetItemText((int)index);
    }

    private void OnDefaultClearColorChanged(Color color)
    {
        ProjectSettings.SetSetting(DefaultClearColorPath, color);
        ProjectSettings.Save();
    }

    private void OnConfirmed()
    {
        _projectSetup.Run(SetupUtils.FormatGameName(_gameNameLineEdit.Text), _projectTypeStr, _templateTypeStr);
    }

    private void OnFeedbackResetTimerTimeout()
    {
        _gameNamePreview.Text = _prevGameName;
    }

    private void OnProjectNameChanged(string gameName)
    {
        _gameNameValidator.Validate(gameName);
    }

    private void OnApplyPressed()
    {
        if (SetupUtils.IsGameNameBad(_gameNameLineEdit.Text))
            return;

        _confirmRestartDialog.PopupCentered();
    }

    private class ProjectSetup
    {
        private readonly string _projectRoot;

        public ProjectSetup()
        {
            _projectRoot = ProjectSettings.GlobalizePath("res://");
        }

        public void Run(string formattedGameName, string projectType, string templateType)
        {
            // The IO functions ran below will break if empty folders exist
            DirectoryUtils.DeleteEmptyDirectories(_projectRoot);

            // Run the setup process
            SetupUtils.RenameProjectFiles(_projectRoot, formattedGameName);
            SetupUtils.RenameAllNamespaces(_projectRoot, formattedGameName);
            SetupUtils.EnsureGDIgnoreFilesInGDUnitTestFolders(_projectRoot);

            // Move the appropriate template files to root
            string templateFolder = Path.Combine("addons", "SetupPlugin", "MainScenes", projectType, templateType);
            string fullPath = Path.Combine(_projectRoot, templateFolder);

            Console.WriteLine(projectType);
            Console.WriteLine(templateType);

            DirectoryUtils.Traverse(fullPath, entry =>
            {
                string relativePath = Path.GetRelativePath(fullPath, entry.FullPath);
                string destPath = Path.Combine(_projectRoot, relativePath);

                // Ensure destination folder exists
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                File.Move(entry.FullPath, destPath);
                return TraverseDecision.Continue;
            });

            ProjectSettings.SetSetting("application/run/main_scene", "res://Level.tscn");
            ProjectSettings.Save();

            // After the editor restarts the following errors and warnigns will appear and can safely be ignored:
            // WARNING: editor/editor_node.cpp:4320 - Addon 'res://addons/SetupPlugin/plugin.cfg' failed to load. No directory found. Removing from enabled plugins.
            // ERROR: Cannot navigate to 'res://addons/SetupPlugin/' as it has not been found in the file system!
            DeleteSetupPlugin();
            SaveActiveScene();
            RestartEditor();
        }

        private void DeleteSetupPlugin()
        {
            EditorInterface.Singleton.SetPluginEnabled(SetupPluginName, false);
            Directory.Delete(Path.Combine(_projectRoot, "addons", SetupPluginName), recursive: true);
        }

        private void SaveActiveScene()
        {
            EditorInterface.Singleton.SaveScene(); // SaveScene works and SaveAllScenes does not work
        }

        private void RestartEditor()
        {
            EditorInterface.Singleton.RestartEditor(save: false);
        }
    }
}
#endif
