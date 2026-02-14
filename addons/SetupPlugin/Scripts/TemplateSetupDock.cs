#if TOOLS
using Framework.Setup;
using Godot;
using GodotUtils;
using System;
using System.IO;

namespace Framework.Setup;

[Tool]
public partial class TemplateSetupDock : VBoxContainer
{
    private const double FeedbackResetTime = 2.0;
    private const string SetupPluginName = "SetupPlugin";
    private const string MainSceneName = "Level";
    private const string DefaultClearColorPath = "rendering/environment/defaults/default_clear_color";
    private const int Padding = 120;

    private ConfirmationDialog _confirmRestartDialog;
    private GameNameValidator _gameNameValidator;
    private ProjectSetup _projectSetup;
    private OptionButton _templateType;
    private OptionButton _projectType;
    private LineEdit _gameNameLineEdit;
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
        _projectType.AddItem("2D");
        _projectType.AddItem("3D");
        _projectType.Select(0);
        _projectType.ItemSelected += OnProjectTypeSelected;
        _projectTypeStr = "2D";

        _templateType = new OptionButton();
        _templateType.AddItem("Minimal");
        _templateType.Select(0);
        _templateType.ItemSelected += OnTemplateTypeSelected;
        _templateTypeStr = "Minimal";

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
        _projectSetup = new ProjectSetup(_projectTypeStr, _templateTypeStr);

        // Layout
        MarginContainer margin = MarginContainerFactory.Create(30);
        VBoxContainer vbox = new();
        HBoxContainer defaultClearColorHbox = new();
        HBoxContainer gameNameHbox = new();
        HBoxContainer projectTypeHbox = new();
        HBoxContainer templateTypeHbox = new();

        AddChild(feedbackResetTimer);

        vbox.AddChild(_gameNamePreview);

        gameNameHbox.AddChild(new Label { 
            Text = "Project Name:", 
            HorizontalAlignment = HorizontalAlignment.Right, 
            CustomMinimumSize = new Vector2(Padding, 0) });
        gameNameHbox.AddChild(_gameNameLineEdit);
        vbox.AddChild(gameNameHbox);

        projectTypeHbox.AddChild(new Label { 
            Text = "Project:",
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(Padding, 0) });
        projectTypeHbox.AddChild(_projectType);
        vbox.AddChild(projectTypeHbox);

        templateTypeHbox.AddChild(new Label { 
            Text = "Template:",
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(Padding, 0) });
        templateTypeHbox.AddChild(_templateType);
        vbox.AddChild(templateTypeHbox);

        defaultClearColorHbox.AddChild(new Label { 
            Text = "Clear Color:",
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(Padding, 0) });
        defaultClearColorHbox.AddChild(defaultClearColorPicker);
        vbox.AddChild(defaultClearColorHbox);

        margin.AddChild(vbox);

        AddChild(margin);
        AddChild(applyButton);

        EditorInterface.Singleton.GetEditorMainScreen().AddChild(_confirmRestartDialog);
    }

    private void OnProjectTypeSelected(long index)
    {
        _templateType.Clear();

        switch (_projectType.GetItemText((int)index))
        {
            case "2D":
                _templateType.AddItem("Minimal");
                break;
            case "3D":
                _templateType.AddItem("Minimal");
                _templateType.AddItem("FPS");
                break;
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
        _projectSetup.Run(SetupUtils.FormatGameName(_gameNameLineEdit.Text));
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
        private readonly string _projectType;
        private readonly string _templateType;

        public ProjectSetup(string projectType, string templateType)
        {
            _projectRoot = ProjectSettings.GlobalizePath("res://");
            _projectType = projectType;
            _templateType = templateType;
        }

        public void Run(string formattedGameName)
        {
            // The IO functions ran below will break if empty folders exist
            DirectoryUtils.DeleteEmptyDirectories(_projectRoot);

            // Run the setup process
            SetupUtils.RenameProjectFiles(_projectRoot, formattedGameName);
            SetupUtils.RenameAllNamespaces(_projectRoot, formattedGameName);
            SetupUtils.EnsureGDIgnoreFilesInGDUnitTestFolders(_projectRoot);

            // Move the appropriate template files to root
            string templateFolder = Path.Combine("addons", "SetupPlugin", "MainScenes", _projectType, _templateType);
            string fullPath = Path.Combine(_projectRoot, templateFolder);

            GD.Print($"Searching {fullPath}");
            GD.Print($"Project root: {_projectRoot}");
            DirectoryUtils.Traverse(fullPath, entry =>
            {
                GD.Print(entry.FullPath);
                File.Move(entry.FullPath, _projectRoot);
                return TraverseDecision.Continue;
            });

            ProjectSettings.SetSetting("application/run/main_scene", "res://Level.tscn");

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

    private class GameNameValidator(Label gameNamePreview, Timer feedbackResetTimer, LineEdit projectNameEdit)
    {
        private string _prevGameName = "";

        public void Validate(string gameName)
        {
            feedbackResetTimer.Stop();

            if (string.IsNullOrWhiteSpace(gameName))
            {
                gameNamePreview.Text = "";
                _prevGameName = "";
                return;
            }

            if (char.IsNumber(gameName.Trim()[0]))
            {
                gameNamePreview.Text = "The first character cannot be a number";
                feedbackResetTimer.Start(FeedbackResetTime);
                ResetNameEdit();
                return;
            }

            if (!SetupUtils.IsAlphaNumericAndAllowSpaces(gameName))
            {
                gameNamePreview.Text = "Special characters are not allowed";
                feedbackResetTimer.Start(FeedbackResetTime);
                ResetNameEdit();
                return;
            }

            gameNamePreview.Text = SetupUtils.FormatGameName(gameName);
            _prevGameName = gameName;
            return;

            void ResetNameEdit()
            {
                projectNameEdit.Text = _prevGameName;
                projectNameEdit.CaretColumn = _prevGameName.Length;
            }
        }
    }
}
#endif
