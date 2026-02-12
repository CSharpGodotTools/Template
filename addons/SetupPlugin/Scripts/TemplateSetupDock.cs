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

    private ConfirmationDialog _confirmDialog;
    private LineEdit _projectNameEdit;
    private Button _applyButton;
    private Label _gameNamePreview;
    private Timer _feedbackResetTimer;
    private string _prevGameName = "";

    public override void _Ready()
    {
        _confirmDialog = new ConfirmationDialog
        {
            Title = "Setup Confirmation",
            DialogText = "Godot will restart with your changes. This cannot be undone",
            OkButtonText = "Yes",
            CancelButtonText = "No"
        };

        _confirmDialog.Confirmed += OnConfirmed;

        EditorInterface.Singleton.GetEditorMainScreen().AddChild(_confirmDialog);

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
        string rawGameName = _projectNameEdit.Text;
        string formattedGameName = SetupUtils.FormatGameName(rawGameName);
        string projectRoot = ProjectSettings.GlobalizePath("res://");

        if (SetupUtils.IsGameNameBad(rawGameName))
            return;

        // The IO functions ran below will break if empty folders exist
        DirectoryUtils.DeleteEmptyDirectories(projectRoot);

        // Run the setup process
        SetupUtils.SetMainScene(projectRoot, MainSceneName);
        SetupUtils.RenameProjectFiles(projectRoot, formattedGameName);
        SetupUtils.RenameAllNamespaces(projectRoot, formattedGameName);
        SetupUtils.EnsureGDIgnoreFilesInGDUnitTestFolders(projectRoot);

        // Ensure all empty folders are deleted when finished
        DirectoryUtils.DeleteEmptyDirectories(projectRoot);

        // Delete the temp folder
        Directory.Delete(Path.Combine(projectRoot, "addons", SetupPluginName, "Temp"), recursive: true);

        // Restart the editor
        EditorInterface.Singleton.SetPluginEnabled(SetupPluginName, false);
        EditorInterface.Singleton.SaveAllScenes();
        EditorInterface.Singleton.RestartEditor(save: false);
    }

    public override void _ExitTree()
    {
        _feedbackResetTimer.Timeout -= OnFeedbackResetTimerTimeout;
        _applyButton.Pressed -= OnApplyPressed;
        _confirmDialog.Confirmed -= OnConfirmed;
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
        _confirmDialog.PopupCentered();
    }
}
#endif
