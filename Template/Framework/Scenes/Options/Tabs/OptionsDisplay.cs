using Godot;
using GodotUtils;
using System;

using static Godot.DisplayServer;
using WindowMode = GodotUtils.WindowMode;

namespace Framework.UI;

public class OptionsDisplay : IDisposable
{
    // Events
    public event Action<int> OnResolutionChanged;

    // Fields
    private readonly ResourceOptions _resourceOptions;
    private Action<WindowMode> _selectWindowModeAction;

    // Window Size
    private LineEdit _resX, _resY;
    private int _prevNumX, _prevNumY;
    private readonly int _minResolution = 36;
    private readonly Options _options;

    // Nodes
    private readonly HSlider _sliderMaxFps;
    private readonly Label _labelMaxFpsFeedback;
    private readonly HSlider _resolutionSlider;
    private readonly OptionButton _vsyncMode;
    private readonly OptionButton _windowMode;
    private readonly Button _windowSizeApply;

    public OptionsDisplay(Options options, Button displayBtn)
    {
        _options = options;
        _sliderMaxFps = options.GetNode<HSlider>("%MaxFPS");
        _labelMaxFpsFeedback = options.GetNode<Label>("%MaxFPSFeedback");
        _resolutionSlider = options.GetNode<HSlider>("%Resolution");
        _vsyncMode = options.GetNode<OptionButton>("%VSyncMode");
        _windowMode = options.GetNode<OptionButton>("%WindowMode");
        _windowSizeApply = options.GetNode<Button>("%WindowSizeApply");
        _resourceOptions = GameFramework.Settings;

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

        _resX.TextChanged -= OnWindowWidthTextChanged;
        _resX.TextSubmitted -= OnWindowWidthTextSubmitted;

        _resY.TextChanged -= OnWindowHeightTextChanged;
        _resY.TextSubmitted -= OnWindowHeightTextSubmitted;

        _windowSizeApply.Pressed -= OnWindowSizeApplyPressed;
        _windowMode.ItemSelected -= OnWindowModeItemSelected;

        GameFramework.Options.WindowModeChanged -= _selectWindowModeAction;

        _resolutionSlider.ValueChanged -= OnResolutionValueChanged;

        _vsyncMode.ItemSelected -= OnVSyncModeItemSelected;
        GC.SuppressFinalize(this);
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
        _resX = _options.GetNode<LineEdit>("%WindowWidth");
        _resY = _options.GetNode<LineEdit>("%WindowHeight");

        _resX.FocusNeighborLeft = displayBtn.GetPath();

        _resX.TextChanged += OnWindowWidthTextChanged;
        _resX.TextSubmitted += OnWindowWidthTextSubmitted;

        _resY.TextChanged += OnWindowHeightTextChanged;
        _resY.TextSubmitted += OnWindowHeightTextSubmitted;

        Vector2I winSize = DisplayServer.WindowGetSize();

        _prevNumX = winSize.X;
        _prevNumY = winSize.Y;

        _resX.Text = winSize.X + "";
        _resY.Text = winSize.Y + "";

        _windowSizeApply.Pressed += OnWindowSizeApplyPressed;
    }

    private void SetupWindowMode(Button displayBtn)
    {
        _windowMode.ItemSelected += OnWindowModeItemSelected;
        _windowMode.Select((int)_resourceOptions.WindowMode);
        _windowMode.FocusNeighborLeft = displayBtn.GetPath();

        _selectWindowModeAction = SelectWindowMode;

        GameFramework.Options.WindowModeChanged += _selectWindowModeAction;

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
        _resolutionSlider.Value = 1 + _minResolution - _resourceOptions.Resolution;
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
        DisplayServer.WindowSetSize(new Vector2I(_prevNumX, _prevNumY));

        // Center window
        Vector2I winSize = DisplayServer.WindowGetSize();
        DisplayServer.WindowSetPosition(DisplayServer.ScreenGetSize() / 2 - winSize / 2);

        _resourceOptions.WindowWidth = winSize.X;
        _resourceOptions.WindowHeight = winSize.Y;
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

        // Update UIWindowSize element on window mode change
        Vector2I winSize = DisplayServer.WindowGetSize();

        _resX.Text = winSize.X + "";
        _resY.Text = winSize.Y + "";
        _prevNumX = winSize.X;
        _prevNumY = winSize.Y;

        _resourceOptions.WindowWidth = winSize.X;
        _resourceOptions.WindowHeight = winSize.Y;
    }

    private void OnWindowWidthTextChanged(string text)
    {
        text.ValidateNumber(_resX, 0, ScreenGetSize().X, ref _prevNumX);
    }

    private void OnWindowHeightTextChanged(string text)
    {
        text.ValidateNumber(_resY, 0, ScreenGetSize().Y, ref _prevNumY);
    }

    private void OnWindowWidthTextSubmitted(string text) => ApplyWindowSize();
    private void OnWindowHeightTextSubmitted(string text) => ApplyWindowSize();
    private void OnWindowSizeApplyPressed() => ApplyWindowSize();

    private void OnResolutionValueChanged(double value)
    {
        _resourceOptions.Resolution = _minResolution - (int)value + 1;
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
