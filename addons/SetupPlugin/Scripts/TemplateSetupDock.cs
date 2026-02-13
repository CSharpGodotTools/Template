#if TOOLS
using Framework.Setup;
using Godot;
using GodotUtils;
using System.IO;

namespace Framework.Setup;

[Tool]
public partial class TemplateSetupDock : VBoxContainer
{
    private const double FeedbackResetTime = 2.0;
    private const string SetupPluginName = "SetupPlugin";
    private const string MainSceneName = "Level";
    private const string DefaultClearColorPath = "rendering/environment/defaults/default_clear_color";

    private ConfirmationDialog _confirmRestartDialog;
    private GameNameValidator _gameNameValidator;
    private ProjectSetup _projectSetup;
    private LineEdit _gameNameLineEdit;
    private Label _gameNamePreview;
    private string _prevGameName = "";

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
        _confirmRestartDialog.Confirmed += OnConfirmed;

        // Feedback reset timer
        Timer feedbackResetTimer = new Timer();
        feedbackResetTimer.Timeout += OnFeedbackResetTimerTimeout;

        // Game name preview
        _gameNamePreview = new()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };

        // Game name line edit
        _gameNameLineEdit = new()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            CustomMinimumSize = new Vector2(200, 0)
        };
        _gameNameLineEdit.TextChanged += OnProjectNameChanged;

        // Default clear color
        ColorPickerButton defaultClearColorPicker = new()
        {
            CustomMinimumSize = new Vector2(75, 35),
            Color = ProjectSettings.GetSetting(DefaultClearColorPath).AsColor()
        };
        defaultClearColorPicker.ColorChanged += OnDefaultClearColorChanged;

        // Apply button
        Button applyButton = new Button()
        {
            Text = "Run Setup",
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(200, 0)
        };
        applyButton.Pressed += OnApplyPressed;

        // Validator and Setup
        _gameNameValidator = new GameNameValidator(_gameNamePreview, feedbackResetTimer, _gameNameLineEdit);
        _projectSetup = new ProjectSetup();

        // Layout
        MarginContainer margin = MarginContainerFactory.Create(30);
        VBoxContainer vbox = new();
        HBoxContainer defaultClearColorHbox = new();
        HBoxContainer gameNameHbox = new();

        AddChild(feedbackResetTimer);

        vbox.AddChild(_gameNamePreview);

        gameNameHbox.AddChild(new Label { Text = "Project Name:" });
        gameNameHbox.AddChild(_gameNameLineEdit);
        vbox.AddChild(gameNameHbox);

        defaultClearColorHbox.AddChild(new Label { Text = "Default Clear Color" });
        defaultClearColorHbox.AddChild(defaultClearColorPicker);
        vbox.AddChild(defaultClearColorHbox);

        margin.AddChild(vbox);

        AddChild(margin);
        AddChild(applyButton);

        EditorInterface.Singleton.GetEditorMainScreen().AddChild(_confirmRestartDialog);
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

        public ProjectSetup()
        {
            _projectRoot = ProjectSettings.GlobalizePath("res://");
        }

        public void Run(string formattedGameName)
        {
            // The IO functions ran below will break if empty folders exist
            DirectoryUtils.DeleteEmptyDirectories(_projectRoot);

            // Run the setup process
            SetupUtils.RenameProjectFiles(_projectRoot, formattedGameName);
            SetupUtils.RenameAllNamespaces(_projectRoot, formattedGameName);
            SetupUtils.EnsureGDIgnoreFilesInGDUnitTestFolders(_projectRoot);

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
