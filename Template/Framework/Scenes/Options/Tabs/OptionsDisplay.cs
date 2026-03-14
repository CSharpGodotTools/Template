using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Linq;

using static Godot.DisplayServer;
using WindowMode = GodotUtils.WindowMode;

namespace __TEMPLATE__.Ui;

public class OptionsDisplay : IDisposable
{
    // Events
    public event Action<int> OnResolutionChanged = null!;

    // Fields
    private readonly ResourceOptions _resourceOptions;
    private Action<WindowMode> _selectWindowModeAction = null!;

    // Window Size
    private OptionButton _windowSizeDropdown = null!;
    private Button _windowSizeCustomBtn = null!;
    private PopupPanel _customSizePopup = null!;
    private LineEdit _popupResX = null!, _popupResY = null!;
    private int _prevNumX, _prevNumY;
    private List<Vector2I> _availableWindowSizes = [];
    private readonly Options _options;

    // Maximum slider steps for the render-resolution slider; mirrors the max_value set in the scene.
    private const int ResolutionSteps = 36;

    // Common display sizes, sorted by area; filtered at runtime against screen resolution.
    private static readonly Vector2I[] BaseWindowSizes =
    [
        new(800, 600),    // 4:3
        new(1024, 768),   // 4:3
        new(1280, 720),   // 16:9
        new(1280, 960),   // 4:3
        new(1366, 768),   // 16:9
        new(1600, 900),   // 16:9
        new(1600, 1200),  // 4:3
        new(1920, 1080),  // 16:9
        new(2560, 1080),  // 21:9
        new(2560, 1440),  // 16:9
        new(3440, 1440),  // 21:9
        new(3840, 2160),  // 4K 16:9
    ];

    // Nodes
    private readonly HSlider _sliderMaxFps;
    private readonly Label _labelMaxFpsFeedback;
    private readonly HSlider _resolutionSlider;
    private readonly OptionButton _vsyncMode;
    private readonly OptionButton _windowMode;

    public OptionsDisplay(Options options, Button displayBtn)
    {
        _options = options;
        _sliderMaxFps = options.GetNode<HSlider>("%MaxFPS");
        _labelMaxFpsFeedback = options.GetNode<Label>("%MaxFPSFeedback");
        _resolutionSlider = options.GetNode<HSlider>("%Resolution");
        _vsyncMode = options.GetNode<OptionButton>("%VSyncMode");
        _windowMode = options.GetNode<OptionButton>("%WindowMode");
        _resourceOptions = Game.Settings;

        SetupMaxFps(displayBtn);
        SetupWindowSize(displayBtn);
        SetupWindowMode(displayBtn);
        SetupResolution(displayBtn);
        SetupVSyncMode(displayBtn);
    }

    public void Dispose()
    {
        _sliderMaxFps.ValueChanged -= OnMaxFpsValueChanged;
        _sliderMaxFps.DragEnded -= OnMaxFpsDragEnded;

        _windowSizeDropdown.ItemSelected -= OnWindowSizeDropdownItemSelected;
        _windowSizeCustomBtn.Pressed -= OnWindowSizeCustomPressed;

        _popupResX.TextChanged -= OnWindowWidthTextChanged;
        _popupResX.TextSubmitted -= OnWindowWidthTextSubmitted;

        _popupResY.TextChanged -= OnWindowHeightTextChanged;
        _popupResY.TextSubmitted -= OnWindowHeightTextSubmitted;

        _windowMode.ItemSelected -= OnWindowModeItemSelected;

        Game.Options.WindowModeChanged -= _selectWindowModeAction;

        _resolutionSlider.ValueChanged -= OnResolutionValueChanged;

        _vsyncMode.ItemSelected -= OnVSyncModeItemSelected;
    }

    private void SetupMaxFps(Button displayBtn)
    {
        _sliderMaxFps.ValueChanged += OnMaxFpsValueChanged;
        _sliderMaxFps.DragEnded += OnMaxFpsDragEnded;
        _sliderMaxFps.FocusNeighborLeft = displayBtn.GetPath();

        _labelMaxFpsFeedback.Text = _resourceOptions.MaxFPS == 0 ? "UNLIMITED" : _resourceOptions.MaxFPS + "";

        _sliderMaxFps.Value = _resourceOptions.MaxFPS;
        _sliderMaxFps.Editable = _resourceOptions.VSyncMode == VSyncMode.Disabled;
    }

    private void SetupWindowSize(Button displayBtn)
    {
        _windowSizeDropdown = _options.GetNode<OptionButton>("%WindowSizeDropdown");
        _windowSizeCustomBtn = _options.GetNode<Button>("%WindowSizeCustom");

        _windowSizeDropdown.FocusNeighborLeft = displayBtn.GetPath();

        PopulateWindowSizeDropdown();
        CreateCustomSizePopup();

        bool isWindowed = _resourceOptions.WindowMode == WindowMode.Windowed;
        _windowSizeDropdown.Disabled = !isWindowed;
        _windowSizeCustomBtn.Disabled = !isWindowed;

        _windowSizeDropdown.ItemSelected += OnWindowSizeDropdownItemSelected;
        _windowSizeCustomBtn.Pressed += OnWindowSizeCustomPressed;
    }

    /// <summary>
    /// Re-syncs the dropdown to reflect the current window size. Call when the Display tab becomes visible.
    /// </summary>
    public void RefreshWindowSizeDisplay()
    {
        PopulateWindowSizeDropdown();

        Vector2I winSize = DisplayServer.WindowGetSize();
        SyncPopupLineEdits(winSize);
    }

    /// <summary>
    /// Updates the custom-size popup line edits to match the supplied window size.
    /// Call every frame while the popup is open so dragging the window is reflected live.
    /// </summary>
    public void UpdatePopupIfOpen()
    {
        if (_customSizePopup == null || !_customSizePopup.Visible)
            return;

        Vector2I winSize = DisplayServer.WindowGetSize();
        SyncPopupLineEdits(winSize);
    }

    private void SyncPopupLineEdits(Vector2I size)
    {
        if (!GodotObject.IsInstanceValid(_popupResX) || !GodotObject.IsInstanceValid(_popupResY))
            return;

        _prevNumX = size.X;
        _prevNumY = size.Y;
        _popupResX.Text = size.X.ToString();
        _popupResY.Text = size.Y.ToString();
    }

    private void PopulateWindowSizeDropdown()
    {
        Vector2I currentSize = DisplayServer.WindowGetSize();
        Vector2I screenSize = DisplayServer.ScreenGetSize();

        _windowSizeDropdown.Clear();
        _availableWindowSizes = [.. BaseWindowSizes
            .Where(s => s.X <= screenSize.X && s.Y <= screenSize.Y)
            .OrderBy(s => s.X * s.Y)];

        bool foundCurrent = false;

        for (int i = 0; i < _availableWindowSizes.Count; i++)
        {
            Vector2I size = _availableWindowSizes[i];
            _windowSizeDropdown.AddItem($"{size.X} x {size.Y}");

            if (size == currentSize)
            {
                _windowSizeDropdown.Select(i);
                foundCurrent = true;
            }
        }

        if (!foundCurrent)
        {
            _windowSizeDropdown.AddItem($"{currentSize.X} x {currentSize.Y}");
            _availableWindowSizes.Add(currentSize);
            _windowSizeDropdown.Select(_availableWindowSizes.Count - 1);
        }
    }

    private void CreateCustomSizePopup()
    {
        _customSizePopup = new PopupPanel();

        MarginContainer margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 14);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_right", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);

        VBoxContainer vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);

        HBoxContainer inputRow = new HBoxContainer();
        inputRow.AddThemeConstantOverride("separation", 8);

        Vector2I currentSize = DisplayServer.WindowGetSize();
        _prevNumX = currentSize.X;
        _prevNumY = currentSize.Y;

        _popupResX = new LineEdit();
        _popupResX.CustomMinimumSize = new Vector2(80, 0);
        _popupResX.Alignment = HorizontalAlignment.Center;
        _popupResX.Text = _prevNumX.ToString();

        Label xLabel = new Label();
        xLabel.Text = "x";

        _popupResY = new LineEdit();
        _popupResY.CustomMinimumSize = new Vector2(80, 0);
        _popupResY.Alignment = HorizontalAlignment.Center;
        _popupResY.Text = _prevNumY.ToString();

        Button applyBtn = new Button();
        applyBtn.Text = "APPLY";
        applyBtn.Pressed += OnCustomSizeApplyPressed;

        inputRow.AddChild(_popupResX);
        inputRow.AddChild(xLabel);
        inputRow.AddChild(_popupResY);

        vbox.AddChild(inputRow);
        vbox.AddChild(applyBtn);

        margin.AddChild(vbox);
        _customSizePopup.AddChild(margin);
        _options.AddChild(_customSizePopup);

        _popupResX.TextChanged += OnWindowWidthTextChanged;
        _popupResX.TextSubmitted += OnWindowWidthTextSubmitted;
        _popupResY.TextChanged += OnWindowHeightTextChanged;
        _popupResY.TextSubmitted += OnWindowHeightTextSubmitted;
    }

    private void SetupWindowMode(Button displayBtn)
    {
        _windowMode.ItemSelected += OnWindowModeItemSelected;
        _windowMode.Select((int)_resourceOptions.WindowMode);
        _windowMode.FocusNeighborLeft = displayBtn.GetPath();

        _selectWindowModeAction = SelectWindowMode;

        Game.Options.WindowModeChanged += _selectWindowModeAction;

        void SelectWindowMode(WindowMode windowMode)
        {
            if (!GodotObject.IsInstanceValid(_windowMode))
                return;

            // Window mode select button could be null. If there was no null check
            // here then we would be assuming that the user can only change fullscreen
            // when in the options screen but this is not the case.
            _windowMode.Select((int)windowMode);
        }
    }

    private void SetupResolution(Button displayBtn)
    {
        _resolutionSlider.FocusNeighborLeft = displayBtn.GetPath();
        _resolutionSlider.Value = 1 + ResolutionSteps - _resourceOptions.Resolution;
        _resolutionSlider.ValueChanged += OnResolutionValueChanged;
    }

    private void SetupVSyncMode(Button displayBtn)
    {
        _vsyncMode.FocusNeighborLeft = displayBtn.GetPath();
        _vsyncMode.Select((int)_resourceOptions.VSyncMode);
        _vsyncMode.ItemSelected += OnVSyncModeItemSelected;
    }

    private void ApplyWindowSize()
    {
        if (Engine.IsEmbeddedInEditor())
            return;

        // Set Root.Size instead of DisplayServer.WindowSetSize so the RenderingServer
        // viewport is updated synchronously in this frame, not deferred via the OS
        // window-resize event (which can be one or more frames late on Wayland/X11).
        _options.GetTree().Root.Size = new Vector2I(_prevNumX, _prevNumY);

        // Center window on the current screen
        Vector2I winSize = _options.GetTree().Root.Size;
        DisplayServer.WindowSetPosition(DisplayServer.ScreenGetSize() / 2 - winSize / 2);

        _resourceOptions.WindowWidth = winSize.X;
        _resourceOptions.WindowHeight = winSize.Y;

        PopulateWindowSizeDropdown();
    }

    private void OnWindowModeItemSelected(long index)
    {
        switch ((WindowMode)index)
        {
            case WindowMode.Windowed:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                _resourceOptions.WindowMode = WindowMode.Windowed;
                break;
            case WindowMode.Borderless:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                _resourceOptions.WindowMode = WindowMode.Borderless;
                break;
            case WindowMode.Fullscreen:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                _resourceOptions.WindowMode = WindowMode.Fullscreen;
                break;
        }

        bool isWindowed = (WindowMode)index == WindowMode.Windowed;
        _windowSizeDropdown.Disabled = !isWindowed;
        _windowSizeCustomBtn.Disabled = !isWindowed;

        Vector2I winSize = DisplayServer.WindowGetSize();
        _prevNumX = winSize.X;
        _prevNumY = winSize.Y;

        _resourceOptions.WindowWidth = winSize.X;
        _resourceOptions.WindowHeight = winSize.Y;

        PopulateWindowSizeDropdown();
    }

    private void OnWindowSizeDropdownItemSelected(long index)
    {
        Vector2I selected = _availableWindowSizes[(int)index];
        _prevNumX = selected.X;
        _prevNumY = selected.Y;
        ApplyWindowSize();
    }

    private void OnWindowSizeCustomPressed()
    {
        Vector2I winSize = DisplayServer.WindowGetSize();
        SyncPopupLineEdits(winSize);

        // Position popup below the Custom button
        Vector2I btnGlobal = (Vector2I)_windowSizeCustomBtn.GlobalPosition;
        _customSizePopup.Popup(new Rect2I(btnGlobal.X, btnGlobal.Y + (int)_windowSizeCustomBtn.Size.Y, 0, 0));
    }

    private void OnCustomSizeApplyPressed()
    {
        _customSizePopup.Hide();
        ApplyWindowSize();
    }

    private void OnWindowWidthTextChanged(string text)
    {
        text.ValidateNumber(_popupResX, 0, ScreenGetSize().X, ref _prevNumX);
    }

    private void OnWindowHeightTextChanged(string text)
    {
        text.ValidateNumber(_popupResY, 0, ScreenGetSize().Y, ref _prevNumY);
    }

    private void OnWindowWidthTextSubmitted(string text) => OnCustomSizeApplyPressed();
    private void OnWindowHeightTextSubmitted(string text) => OnCustomSizeApplyPressed();

    private void OnResolutionValueChanged(double value)
    {
        _resourceOptions.Resolution = ResolutionSteps - (int)value + 1;
        OnResolutionChanged?.Invoke(_resourceOptions.Resolution);
    }

    private void OnVSyncModeItemSelected(long index)
    {
        VSyncMode vsyncMode = (VSyncMode)index;
        WindowSetVsyncMode(vsyncMode);
        _resourceOptions.VSyncMode = vsyncMode;
        _sliderMaxFps.Editable = _resourceOptions.VSyncMode == VSyncMode.Disabled;
    }

    private void OnMaxFpsValueChanged(double value)
    {
        _labelMaxFpsFeedback.Text = value == 0 ? "UNLIMITED" : value + "";
        _resourceOptions.MaxFPS = (int)value;
    }

    private void OnMaxFpsDragEnded(bool valueChanged)
    {
        if (!valueChanged)
            return;

        Engine.MaxFps = _resourceOptions.MaxFPS;
    }
}
