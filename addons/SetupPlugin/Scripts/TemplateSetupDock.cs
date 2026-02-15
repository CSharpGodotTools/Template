#if TOOLS
using Godot;
using System;
using System.Collections.Generic;

namespace Framework.Setup;

[Tool]
public partial class TemplateSetupDock : VBoxContainer
{
    private const string DefaultClearColorPath = "rendering/environment/defaults/default_clear_color";
    private const string MainScenesPath = "res://addons/SetupPlugin/MainScenes";
    private const string ProjectRootPath = "res://";
    private const string RebuildInstruction = "Rebuild the project, then disable and re-enable the Setup Plugin.";
    private const int LabelPadding = 120;
    private const int MarginPadding = 30;

    private ConfirmationDialog _confirmRestartDialog;
    private Timer _feedbackResetTimer;
    private GameNameValidator _gameNameValidator;
    private ProjectSetupRunner _projectSetupRunner;
    private SetupRuntimeStateValidator _runtimeStateValidator;
    private SetupTemplateCatalog _templateCatalog;

    private OptionButton _templateType;
    private OptionButton _projectType;
    private LineEdit _gameNameLineEdit;
    private ColorPickerButton _defaultClearColorPicker;
    private Button _applyButton;
    private VBoxContainer _contentContainer;
    private Label _gameNamePreview;
    private Label _statusLabel;

    private string _selectedProjectType = string.Empty;
    private string _selectedTemplateType = string.Empty;
    private string _runtimeStateError = string.Empty;
    private bool _isRuntimeStateValid;
    private bool _eventsRegistered;
    private bool _isDisposed;

    public override void _Ready()
    {
        InitializeServices();
        CreateControls();
        BuildLayout();
        ValidateAndInitializeState();
        RegisterEvents();
    }

    public override void _ExitTree()
    {
        PrepareForPluginDisable();
    }

    public void PrepareForPluginDisable()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        UnregisterEvents();
        ReleaseRestartDialog();
    }

    private void InitializeServices()
    {
        string projectRoot = ProjectSettings.GlobalizePath(ProjectRootPath);
        string mainScenesRoot = ProjectSettings.GlobalizePath(MainScenesPath);

        _runtimeStateValidator = new SetupRuntimeStateValidator(projectRoot, mainScenesRoot);
        _projectSetupRunner = new ProjectSetupRunner(projectRoot, mainScenesRoot);
    }

    private void CreateControls()
    {
        _confirmRestartDialog = new ConfirmationDialog
        {
            Title = "Setup Confirmation",
            DialogText = "Godot will restart with your changes. This cannot be undone",
            OkButtonText = "Yes",
            CancelButtonText = "No"
        };

        _feedbackResetTimer = new Timer();

        _statusLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Visible = false,
            Modulate = new Color(1.0f, 0.4f, 0.4f)
        };

        _gameNamePreview = new Label
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(200, 0)
        };

        _gameNameLineEdit = new LineEdit
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            CustomMinimumSize = new Vector2(200, 0)
        };

        _projectType = new OptionButton();
        _templateType = new OptionButton();

        _defaultClearColorPicker = new ColorPickerButton
        {
            CustomMinimumSize = new Vector2(75, 35),
            Color = ProjectSettings.GetSetting(DefaultClearColorPath).AsColor()
        };

        _applyButton = new Button
        {
            Text = "Run Setup",
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(200, 0)
        };

        _gameNameValidator = new GameNameValidator(_gameNamePreview, _feedbackResetTimer, _gameNameLineEdit);

        Node editorMainScreen = EditorInterface.Singleton.GetEditorMainScreen();
        editorMainScreen.AddChild(_confirmRestartDialog);

        SetControlsEnabled(enabled: false);
    }

    private void BuildLayout()
    {
        _contentContainer = new VBoxContainer();
        _contentContainer.AddChild(_statusLabel);
        _contentContainer.AddChild(_gameNamePreview);

        AddLabeledControl("Project Name", _gameNameLineEdit);
        AddLabeledControl("Project", _projectType);
        AddLabeledControl("Template", _templateType);
        AddLabeledControl("Clear Color", _defaultClearColorPicker);

        MarginContainer marginContainer = new MarginContainer();
        marginContainer.AddThemeConstantOverride("margin_left", MarginPadding);
        marginContainer.AddThemeConstantOverride("margin_top", MarginPadding);
        marginContainer.AddThemeConstantOverride("margin_right", MarginPadding);
        marginContainer.AddThemeConstantOverride("margin_bottom", MarginPadding);

        marginContainer.AddChild(_contentContainer);

        AddChild(_feedbackResetTimer);
        AddChild(marginContainer);
        AddChild(_applyButton);
    }

    private void RegisterEvents()
    {
        if (_eventsRegistered)
        {
            return;
        }

        _confirmRestartDialog.Confirmed += OnConfirmed;
        _feedbackResetTimer.Timeout += OnFeedbackResetTimerTimeout;
        _gameNameLineEdit.TextChanged += OnProjectNameChanged;
        _projectType.ItemSelected += OnProjectTypeSelected;
        _templateType.ItemSelected += OnTemplateTypeSelected;
        _defaultClearColorPicker.ColorChanged += OnDefaultClearColorChanged;
        _applyButton.Pressed += OnApplyPressed;
        _eventsRegistered = true;
    }

    private void UnregisterEvents()
    {
        if (!_eventsRegistered)
        {
            return;
        }

        _eventsRegistered = false;

        if (HasSignalConnection(_confirmRestartDialog, ConfirmationDialog.SignalName.Confirmed, Callable.From(OnConfirmed)))
        {
            _confirmRestartDialog.Confirmed -= OnConfirmed;
        }

        if (HasSignalConnection(_feedbackResetTimer, Timer.SignalName.Timeout, Callable.From(OnFeedbackResetTimerTimeout)))
        {
            _feedbackResetTimer.Timeout -= OnFeedbackResetTimerTimeout;
        }

        if (HasSignalConnection(_gameNameLineEdit, LineEdit.SignalName.TextChanged, Callable.From<string>(OnProjectNameChanged)))
        {
            _gameNameLineEdit.TextChanged -= OnProjectNameChanged;
        }

        if (HasSignalConnection(_projectType, OptionButton.SignalName.ItemSelected, Callable.From<long>(OnProjectTypeSelected)))
        {
            _projectType.ItemSelected -= OnProjectTypeSelected;
        }

        if (HasSignalConnection(_templateType, OptionButton.SignalName.ItemSelected, Callable.From<long>(OnTemplateTypeSelected)))
        {
            _templateType.ItemSelected -= OnTemplateTypeSelected;
        }

        if (HasSignalConnection(_defaultClearColorPicker, ColorPickerButton.SignalName.ColorChanged, Callable.From<Color>(OnDefaultClearColorChanged)))
        {
            _defaultClearColorPicker.ColorChanged -= OnDefaultClearColorChanged;
        }

        if (HasSignalConnection(_applyButton, Button.SignalName.Pressed, Callable.From(OnApplyPressed)))
        {
            _applyButton.Pressed -= OnApplyPressed;
        }
    }

    private static bool HasSignalConnection(GodotObject source, StringName signalName, Callable callable)
    {
        // Godot can auto-disconnect managed delegates during plugin unload/reload
        // before _ExitTree executes, so we guard each -= to avoid disconnect errors.
        if (source == null)
        {
            return false;
        }

        if (!GodotObject.IsInstanceValid(source))
        {
            return false;
        }

        return source.IsConnected(signalName, callable);
    }

    private void ReleaseRestartDialog()
    {
        if (_confirmRestartDialog == null)
        {
            return;
        }

        if (!GodotObject.IsInstanceValid(_confirmRestartDialog))
        {
            _confirmRestartDialog = null;
            return;
        }

        Node parent = _confirmRestartDialog.GetParent();
        if (parent != null)
        {
            parent.RemoveChild(_confirmRestartDialog);
        }

        _confirmRestartDialog.QueueFree();
        _confirmRestartDialog = null;
    }

    private void ValidateAndInitializeState()
    {
        if (!_runtimeStateValidator.TryValidate(out string runtimeFailure))
        {
            MarkRuntimeStateInvalid(runtimeFailure, null);
            return;
        }

        string mainScenesRoot = ProjectSettings.GlobalizePath(MainScenesPath);
        if (!SetupTemplateCatalog.TryLoad(mainScenesRoot, out SetupTemplateCatalog templateCatalog, out string catalogFailure))
        {
            MarkRuntimeStateInvalid(catalogFailure, null);
            return;
        }

        _templateCatalog = templateCatalog;

        if (!_templateCatalog.TryGetFirstSelection(out string projectType, out string templateType))
        {
            MarkRuntimeStateInvalid("No setup template options are available.", null);
            return;
        }

        PopulateProjectTypeOptions();
        _selectedProjectType = projectType;
        PopulateTemplateTypeOptions(_selectedProjectType);
        _selectedTemplateType = templateType;

        _projectType.Select(0);
        _templateType.Select(0);

        _isRuntimeStateValid = true;
        _runtimeStateError = string.Empty;
        SetControlsEnabled(enabled: true);
        SetStatus("Setup plugin is ready.", isError: false);
    }

    private void PopulateProjectTypeOptions()
    {
        _projectType.Clear();

        IEnumerable<string> projectTypes = _templateCatalog.ProjectTypes;
        foreach (string projectType in projectTypes)
        {
            _projectType.AddItem(projectType);
        }
    }

    private void PopulateTemplateTypeOptions(string projectType)
    {
        _templateType.Clear();

        if (!_templateCatalog.TryGetTemplates(projectType, out IReadOnlyList<string> templateTypes))
        {
            _selectedTemplateType = string.Empty;
            return;
        }

        foreach (string templateType in templateTypes)
        {
            _templateType.AddItem(templateType);
        }

        if (_templateType.ItemCount == 0)
        {
            _selectedTemplateType = string.Empty;
            return;
        }

        _templateType.Select(0);
        _selectedTemplateType = _templateType.GetItemText(0);
    }

    private void AddLabeledControl(string labelText, Control control)
    {
        HBoxContainer row = new HBoxContainer();
        row.AddChild(new Label
        {
            Text = $"{labelText}:",
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(LabelPadding, 0)
        });

        row.AddChild(control);
        _contentContainer.AddChild(row);
    }

    private void SetControlsEnabled(bool enabled)
    {
        _gameNameLineEdit.Editable = enabled;
        _projectType.Disabled = !enabled;
        _templateType.Disabled = !enabled;
        _defaultClearColorPicker.Disabled = !enabled;
        _applyButton.Disabled = !enabled;
    }

    private void SetStatus(string text, bool isError)
    {
        _statusLabel.Text = text;
        _statusLabel.Visible = !string.IsNullOrWhiteSpace(text);
        _statusLabel.Modulate = isError
            ? new Color(1.0f, 0.4f, 0.4f)
            : new Color(0.6f, 0.95f, 0.6f);
    }

    private bool EnsureRuntimeStateValid(string operationName)
    {
        if (_isRuntimeStateValid)
        {
            return true;
        }

        string message = _runtimeStateError;
        if (string.IsNullOrWhiteSpace(message))
        {
            message = RebuildInstruction;
        }

        SetStatus(message, isError: true);
        GD.PrintErr($"Setup operation blocked ({operationName}). {message}");
        return false;
    }

    private void MarkRuntimeStateInvalid(string reason, Exception exception)
    {
        _isRuntimeStateValid = false;
        _runtimeStateError = $"{reason} {RebuildInstruction}";
        SetControlsEnabled(enabled: false);
        SetStatus(_runtimeStateError, isError: true);

        GD.PrintErr(_runtimeStateError);

        if (exception == null)
        {
            return;
        }

        GD.PrintErr(exception.ToString());
    }

    private void ReportUserError(string message)
    {
        _gameNamePreview.Text = message;
        SetStatus(message, isError: true);
        GD.PrintErr(message);
    }

    private void OnProjectTypeSelected(long index)
    {
        if (!EnsureRuntimeStateValid("ProjectTypeSelected"))
        {
            return;
        }

        int selectedIndex = (int)index;
        if (selectedIndex < 0 || selectedIndex >= _projectType.ItemCount)
        {
            ReportUserError("Selected project type is out of range.");
            return;
        }

        _selectedProjectType = _projectType.GetItemText(selectedIndex);
        PopulateTemplateTypeOptions(_selectedProjectType);
    }

    private void OnTemplateTypeSelected(long index)
    {
        if (!EnsureRuntimeStateValid("TemplateTypeSelected"))
        {
            return;
        }

        int selectedIndex = (int)index;
        if (selectedIndex < 0 || selectedIndex >= _templateType.ItemCount)
        {
            ReportUserError("Selected template type is out of range.");
            return;
        }

        _selectedTemplateType = _templateType.GetItemText(selectedIndex);
    }

    private void OnDefaultClearColorChanged(Color color)
    {
        if (!EnsureRuntimeStateValid("DefaultClearColorChanged"))
        {
            return;
        }

        try
        {
            ProjectSettings.SetSetting(DefaultClearColorPath, color);
            ProjectSettings.Save();
        }
        catch (Exception exception)
        {
            MarkRuntimeStateInvalid("Failed to save clear color setting.", exception);
        }
    }

    private void OnConfirmed()
    {
        if (!EnsureRuntimeStateValid("Confirmed"))
        {
            return;
        }

        try
        {
            string formattedGameName = GameNameRules.FormatGameName(_gameNameLineEdit.Text);
            _projectSetupRunner.Run(formattedGameName, _selectedProjectType, _selectedTemplateType);
        }
        catch (Exception exception)
        {
            MarkRuntimeStateInvalid("Setup execution failed because required artifacts are missing or stale.", exception);
        }
    }

    private void OnFeedbackResetTimerTimeout()
    {
        if (_gameNameValidator == null)
        {
            return;
        }

        _gameNameValidator.RestorePreviousGameNamePreview();
    }

    private void OnProjectNameChanged(string gameName)
    {
        if (!EnsureRuntimeStateValid("ProjectNameChanged"))
        {
            return;
        }

        try
        {
            _gameNameValidator.Validate(gameName);
        }
        catch (Exception exception)
        {
            MarkRuntimeStateInvalid("Game name validation failed because setup state is not ready.", exception);
        }
    }

    private void OnApplyPressed()
    {
        if (!EnsureRuntimeStateValid("ApplyPressed"))
        {
            return;
        }

        if (!GameNameRules.TryValidateForSetup(_gameNameLineEdit.Text, out string validationError))
        {
            ReportUserError(validationError);
            return;
        }

        _confirmRestartDialog.PopupCentered();
    }
}
#endif
