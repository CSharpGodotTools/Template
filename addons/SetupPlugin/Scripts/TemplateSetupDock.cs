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

    private ConfirmationDialog _confirmRestartDialog;
    private LineEdit _projectNameEdit;
    private Button _applyButton;
    private Label _gameNamePreview;
    private Timer _feedbackResetTimer;
    private string _prevGameName = "";

    public override void _Ready()
    {
        _confirmRestartDialog = new ConfirmationDialog
        {
            Title = "Setup Confirmation",
            DialogText = "Godot will restart with your changes. This cannot be undone",
            OkButtonText = "Yes",
            CancelButtonText = "No"
        };

        _confirmRestartDialog.Confirmed += OnConfirmed;

        EditorInterface.Singleton.GetEditorMainScreen().AddChild(_confirmRestartDialog);

        AddChild(_feedbackResetTimer = new Timer());
        _feedbackResetTimer.Timeout += OnFeedbackResetTimerTimeout;

        MarginContainer margin = MarginContainerFactory.Create(30);

        VBoxContainer vbox = new();

        vbox.AddChild(_gameNamePreview = new()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        });

        HBoxContainer hbox = new();

        hbox.AddChild(new Label { Text = "Project Name:" });

        hbox.AddChild(_projectNameEdit = new()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            CustomMinimumSize = new Vector2(200, 0)
        });

        _projectNameEdit.TextChanged += OnProjectNameChanged;

        vbox.AddChild(hbox);

        margin.AddChild(vbox);

        AddChild(margin);

        _applyButton = new Button()
        {
            Text = "Apply Setup",
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(200, 0)
        };

        _applyButton.Pressed += OnApplyPressed;
        AddChild(_applyButton);
    }

    private void OnConfirmed()
    {
        string formattedGameName = SetupUtils.FormatGameName(_projectNameEdit.Text);
        string projectRoot = ProjectSettings.GlobalizePath("res://");

        // The IO functions ran below will break if empty folders exist
        DirectoryUtils.DeleteEmptyDirectories(projectRoot);

        // Run the setup process
        SetupUtils.SetMainScene(projectRoot, MainSceneName);
        SetupUtils.RenameProjectFiles(projectRoot, formattedGameName);
        SetupUtils.RenameAllNamespaces(projectRoot, formattedGameName);
        SetupUtils.EnsureGDIgnoreFilesInGDUnitTestFolders(projectRoot);

        // Restart the editor
        // Godot will try to find the setup plugin on startup and show a popup, this happens because
        // the assembly name is being changed in the project.godot file. There is no known fix around
        // this.
        EditorInterface.Singleton.SetPluginEnabled(SetupPluginName, false);
        Directory.Delete(Path.Combine(projectRoot, "addons", SetupPluginName), recursive: true);
        EditorInterface.Singleton.SaveScene(); // SaveScene works and SaveAllScenes does not work
        EditorInterface.Singleton.RestartEditor(save: false);
    }

    public override void _ExitTree()
    {
        _feedbackResetTimer.Timeout -= OnFeedbackResetTimerTimeout;
        _applyButton.Pressed -= OnApplyPressed;
        _confirmRestartDialog.Confirmed -= OnConfirmed;
    }

    private void OnFeedbackResetTimerTimeout()
    {
        _gameNamePreview.Text = _prevGameName;
    }

    private void OnProjectNameChanged(string gameName)
    {
        _feedbackResetTimer.Stop();

        if (string.IsNullOrWhiteSpace(gameName))
        {
            _gameNamePreview.Text = "";
            _prevGameName = "";
            return;
        }

        if (char.IsNumber(gameName.Trim()[0]))
        {
            _gameNamePreview.Text = "The first character cannot be a number";
            _feedbackResetTimer.Start(FeedbackResetTime);
            ResetNameEdit();
            return;
        }

        if (!SetupUtils.IsAlphaNumericAndAllowSpaces(gameName))
        {
            _gameNamePreview.Text = "Special characters are not allowed";
            _feedbackResetTimer.Start(FeedbackResetTime);
            ResetNameEdit();
            return;
        }

        _gameNamePreview.Text = SetupUtils.FormatGameName(gameName);
        _prevGameName = gameName;
        return;

        void ResetNameEdit()
        {
            _projectNameEdit.Text = _prevGameName;
            _projectNameEdit.CaretColumn = _prevGameName.Length;
        }
    }

    private void OnApplyPressed()
    {
        if (SetupUtils.IsGameNameBad(_projectNameEdit.Text))
            return;

        _confirmRestartDialog.PopupCentered();
    }
}
#endif
