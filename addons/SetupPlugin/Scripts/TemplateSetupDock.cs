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

    private SetupConfirmationDialog _confirmRestartDialog;
    private GameNamePreview _gameNamePreview;
    private GameNameControl _gameNameControl;
    private ProjectSetup _projectSetup;
    private ApplyButton _applyButton;
    private bool _initialized;

    public override void _Ready()
    {
        if (_initialized) // Sanity check (tool scripts are something else..)
            return;

        _initialized = true;
        _gameNamePreview = new GameNamePreview();
        _gameNameControl = new GameNameControl(_gameNamePreview);
        _projectSetup = new ProjectSetup();
        _confirmRestartDialog = new SetupConfirmationDialog(_projectSetup, _gameNameControl);
        _applyButton = new ApplyButton();
        _applyButton.Init(_gameNameControl, _confirmRestartDialog);

        MarginContainer margin = MarginContainerFactory.Create(30);
        VBoxContainer vbox = new();

        vbox.AddChild(_gameNamePreview);
        vbox.AddChild(_gameNameControl);

        margin.AddChild(vbox);

        AddChild(margin);
        AddChild(_applyButton);

        EditorInterface.Singleton.GetEditorMainScreen().AddChild(_confirmRestartDialog);
    }

    private class ApplyButton : Button
    {
        private GameNameControl _projectNameContorl;
        private SetupConfirmationDialog _confirmRestartDialog;

        public void Init(GameNameControl projectNameControl, SetupConfirmationDialog confirmRestartDialog)
        {
            _projectNameContorl = projectNameControl;
            _confirmRestartDialog = confirmRestartDialog;
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            CustomMinimumSize = new Vector2(200, 0);
            Pressed += OnPressed;
        }

        public override void _Ready()
        {
            base._Ready();
            GD.Print("REDY");
            Text = "Apply Setup";
        }

        public override void _ExitTree()
        {
            GD.Print("EXT TREE");
            Pressed -= OnPressed;
        }

        private void OnPressed()
        {
            if (SetupUtils.IsGameNameBad(_projectNameContorl.LineEdit.Text))
                return;

            _confirmRestartDialog.PopupCentered();
        }
    }

    private class FeedbackResetTimer(GameNamePreview gameNamePreview, Func<string> getPrevGameName) : Timer
    {
        public override void _Ready()
        {
            Timeout += OnTimeout;
        }

        public override void _ExitTree()
        {
            Timeout -= OnTimeout;
        }

        private void OnTimeout()
        {
            gameNamePreview.Text = getPrevGameName();
        }
    }

    private class GameNamePreview : Label
    {
        public override void _Ready()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        }
    }

    private class SetupConfirmationDialog(ProjectSetup projectSetup, GameNameControl projectNameControl) : ConfirmationDialog
    {
        public override void _Ready()
        {
            Title = "Setup Confirmation";
            DialogText = "Godot will restart with your changes. This cannot be undone";
            OkButtonText = "Yes";
            CancelButtonText = "No";
            Confirmed += OnConfirmed;
        }

        public override void _ExitTree()
        {
            Confirmed -= OnConfirmed;
        }

        private void OnConfirmed()
        {
            projectSetup.Run(SetupUtils.FormatGameName(projectNameControl.LineEdit.Text));
        }
    }

    private class GameNameControl : HBoxContainer
    {
        public GameNameValidator Validator { get; private set; }
        public LineEdit LineEdit { get; private set; }

        private readonly GameNamePreview _gameNamePreview;

        public GameNameControl(GameNamePreview gameNamePreview)
        {
            _gameNamePreview = gameNamePreview;
        }

        public override void _Ready()
        {
            Validator = new GameNameValidator(_gameNamePreview, LineEdit);
            LineEdit = new LineEdit
            {
                SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
                CustomMinimumSize = new Vector2(200, 0)
            };

            LineEdit.TextChanged += OnGameNameChanged;

            AddChild(new Label { Text = "Project Name:" });
            AddChild(LineEdit);
            AddChild(Validator.FeedbackResetTimer);
        }

        public override void _ExitTree()
        {
            LineEdit.TextChanged -= OnGameNameChanged;
        }

        private void OnGameNameChanged(string newText)
        {
            Validator.Validate(LineEdit.Text);
        }
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
            SetupUtils.SetMainScene(_projectRoot, MainSceneName);
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

    private class GameNameValidator
    {
        public FeedbackResetTimer FeedbackResetTimer { get; private init; }

        private readonly GameNamePreview _gameNamePreview;
        private readonly LineEdit _gameNameLineEdit;
        private string _prevGameName = "";

        public GameNameValidator(GameNamePreview gameNamePreview, LineEdit gameNameLineEdit)
        {
            _gameNamePreview = gameNamePreview;
            _gameNameLineEdit = gameNameLineEdit;
            FeedbackResetTimer = new FeedbackResetTimer(gameNamePreview, () => _prevGameName);
        }

        public void Validate(string gameName)
        {
            FeedbackResetTimer.Stop();

            if (string.IsNullOrWhiteSpace(gameName))
            {
                _gameNamePreview.Text = "";
                _prevGameName = "";
                return;
            }

            if (char.IsNumber(gameName.Trim()[0]))
            {
                _gameNamePreview.Text = "The first character cannot be a number";
                FeedbackResetTimer.Start(FeedbackResetTime);
                ResetNameEdit();
                return;
            }

            if (!SetupUtils.IsAlphaNumericAndAllowSpaces(gameName))
            {
                _gameNamePreview.Text = "Special characters are not allowed";
                FeedbackResetTimer.Start(FeedbackResetTime);
                ResetNameEdit();
                return;
            }

            _gameNamePreview.Text = SetupUtils.FormatGameName(gameName);
            _prevGameName = gameName;
            return;

            void ResetNameEdit()
            {
                _gameNameLineEdit.Text = _prevGameName;
                _gameNameLineEdit.CaretColumn = _prevGameName.Length;
            }
        }
    }
}
#endif
