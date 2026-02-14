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
    private const string MainScenePath = "application/run/main_scene";
    private const int Padding = 120;
    private const string MainScenesPath = "res://addons/SetupPlugin/MainScenes/";

    // Not really sure what to call this. The key can be either "2D" or "3D" and the list can be like ["Minimal"] or ["Minimal", "FPS"]
    // Also these fields are kind of clumped together. Maybe need to group / organize them.
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
    private string _projectTypeStr; // not really sure what to call this
    private string _templateTypeStr; // not really sure what to call this

    public override void _Ready()
    {
        // There is a lot going on in this _Ready() method and I'm not really sure what kind of OOP should be done here
        // without making it too hard to expand on later

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

        // Traverse the specific folders "2D" and "3D" (more could be added in the future like "VR" as an example I guess idk
        DirectoryUtils.Traverse(ProjectSettings.GlobalizePath(MainScenesPath), templateTypeEntry =>
        {
            _sceneTypes[templateTypeEntry.FileName] = new List<string>();

            // Traverse the specific template folder types
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

        //GD.Print("Project type is initially set to " + _projectTypeStr);
        //GD.Print("Template type is initially set to " + _templateTypeStr);

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

        // Actually start adding stuff to the tree now
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

    /// <summary>
    /// Pairs a label with <paramref name="text"/> next to a <paramref name="control"/> using a specific padding and right alignment.
    /// </summary>
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

    /// <summary>
    /// The project type that was selected ("2D" / "3D")
    /// </summary>
    private void OnProjectTypeSelected(long index)
    {
        // Clear template types
        _templateType.Clear();

        string projectType = _projectType.GetItemText((int)index);
        _projectTypeStr = projectType;
        //GD.Print("Project type was changed to " + projectType);

        // Re-populate template types depending on what project type was selected
        foreach (string templateType in _sceneTypes[projectType])
        {
            _templateType.AddItem(templateType);
        }

        _templateTypeStr = _templateType.GetItemText(0);
        //GD.Print("Template type was changed to " + _templateTypeStr);
    }

    /// <summary>
    /// The template type that was selected ("Minimal", "FPS")
    /// </summary>
    private void OnTemplateTypeSelected(long index)
    {
        _templateTypeStr = _templateType.GetItemText((int)index);
        //GD.Print("Template type was changed to " + _templateTypeStr);
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

    // This should probably go in its own file
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

            // Move the appropriate template files to root
            // This Path.Combine is really long but I'm afraid to do a/b/c because I remember doing this broke on linux systems (but works fine on windows)
            string templateFolder = Path.Combine(_projectRoot, "addons", "SetupPlugin", "MainScenes", projectType, templateType);

            //Console.WriteLine("Setup ran with following settings:");
            //Console.WriteLine(projectType);
            //Console.WriteLine(templateType);

            // Copy all files and folders from the respective template folder to root
            // Maybe DirectoryUtils could have a method that copies files from one folder to another that preserves file structure like 
            // what is being done here, but what would such a method be called?
            DirectoryUtils.Traverse(templateFolder, entry =>
            {
                string relativePath = Path.GetRelativePath(templateFolder, entry.FullPath);
                string dest = Path.Combine(_projectRoot, relativePath);

                //Console.WriteLine($"Copying from {entry.FullPath} to {dest}");

                if (entry.IsDirectory)
                    Directory.CreateDirectory(dest);
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(entry.FullPath, dest, overwrite: true);
                }

                return TraverseDecision.Continue;
            });

            ProjectSettings.SetSetting(MainScenePath, "res://Level.tscn");
            ProjectSettings.Save(); // This has to be set before SetupUtils.RenameProjectFiles(...) or assembly wont get set for some reason

            SetupUtils.RenameProjectFiles(_projectRoot, formattedGameName); // This needs to be set after ProjectSettings.Save() or assembly wont get set for some reason
            SetupUtils.RenameAllNamespaces(_projectRoot, formattedGameName); // We rename all namespaces after all files have been moved / copied
            SetupUtils.EnsureGDIgnoreFilesInGDUnitTestFolders(_projectRoot);

            // After the editor restarts the following errors and warnigns will appear and can safely be ignored:
            // WARNING: editor/editor_node.cpp:4320 - Addon 'res://addons/SetupPlugin/plugin.cfg' failed to load. No directory found. Removing from enabled plugins.
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
