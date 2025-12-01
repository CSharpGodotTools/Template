using Godot;
using System;

using static Godot.DisplayServer;

namespace GodotUtils.UI;

public class OptionsDisplay(Options options)
{
    public event Action<int> OnResolutionChanged;

    private ResourceOptions _options;

    // Max FPS
    private HSlider _sliderMaxFps;
    private Label _labelMaxFpsFeedback;

    // Window Size
    private LineEdit _resX, _resY;
    private int _prevNumX, _prevNumY;
    private int _minResolution = 36;

    public void Initialize()
    {
        _options = OptionsManager.GetOptions();

        SetupMaxFps();
        SetupWindowSize();
        SetupWindowMode();
        SetupResolution();
        SetupVSyncMode();
    }

    private void SetupMaxFps()
    {
        HSlider maxFps = options.GetNode<HSlider>("%MaxFPS");
        maxFps.ValueChanged += OnMaxFpsValueChanged;
        maxFps.DragEnded += OnMaxFpsDragEnded;

        _labelMaxFpsFeedback = options.GetNode<Label>("%MaxFPSFeedback");
        _labelMaxFpsFeedback.Text = _options.MaxFPS == 0 ? "UNLIMITED" : _options.MaxFPS + "";

        maxFps.Value = _options.MaxFPS;
        maxFps.Editable = _options.VSyncMode == VSyncMode.Disabled;

        _sliderMaxFps = maxFps;
    }

    private void SetupWindowSize()
    {
        _resX = options.GetNode<LineEdit>("%WindowWidth");
        _resY = options.GetNode<LineEdit>("%WindowHeight");

        _resX.TextChanged += OnWindowWidthTextChanged;
        _resX.TextSubmitted += OnWindowWidthTextSubmitted;

        _resY.TextChanged += OnWindowHeightTextChanged;
        _resY.TextSubmitted += OnWindowHeightTextSubmitted;

        Vector2I winSize = DisplayServer.WindowGetSize();

        _prevNumX = winSize.X;
        _prevNumY = winSize.Y;

        _resX.Text = winSize.X + "";
        _resY.Text = winSize.Y + "";

        options.GetNode<Button>("%WindowSizeApply").Pressed += OnWindowSizeApplyPressed;
    }

    private void SetupWindowMode()
    {
        OptionButton windowModeBtn = options.GetNode<OptionButton>("%WindowMode");
        windowModeBtn.ItemSelected += OnWindowModeItemSelected;
        windowModeBtn.Select((int)_options.WindowMode);

        OptionsManager.Instance.WindowModeChanged += windowMode =>
        {
            if (!GodotObject.IsInstanceValid(windowModeBtn))
                return;

            // Window mode select button could be null. If there was no null check
            // here then we would be assuming that the user can only change fullscreen
            // when in the options screen but this is not the case.
            windowModeBtn.Select((int)windowMode);
        };
    }

    private void SetupResolution()
    {
        HSlider resolutionSlider = options.GetNode<HSlider>("%Resolution");
        resolutionSlider.Value = 1 + _minResolution - _options.Resolution;
        resolutionSlider.ValueChanged += OnResolutionValueChanged;
    }

    private void SetupVSyncMode()
    {
        OptionButton vsyncMode = options.GetNode<OptionButton>("%VSyncMode");
        vsyncMode.Select((int)_options.VSyncMode);
        vsyncMode.ItemSelected += OnVSyncModeItemSelected;
    }

    private void ApplyWindowSize()
    {
        DisplayServer.WindowSetSize(new Vector2I(_prevNumX, _prevNumY));

        // Center window
        Vector2I winSize = DisplayServer.WindowGetSize();
        DisplayServer.WindowSetPosition(DisplayServer.ScreenGetSize() / 2 - winSize / 2);

        _options.WindowWidth = winSize.X;
        _options.WindowHeight = winSize.Y;
    }

    private void OnWindowModeItemSelected(long index)
    {
        switch ((WindowMode)index)
        {
            case WindowMode.Windowed:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                _options.WindowMode = WindowMode.Windowed;
                break;
            case WindowMode.Borderless:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                _options.WindowMode = WindowMode.Borderless;
                break;
            case WindowMode.Fullscreen:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                _options.WindowMode = WindowMode.Fullscreen;
                break;
        }

        // Update UIWindowSize element on window mode change
        Vector2I winSize = DisplayServer.WindowGetSize();

        _resX.Text = winSize.X + "";
        _resY.Text = winSize.Y + "";
        _prevNumX = winSize.X;
        _prevNumY = winSize.Y;

        _options.WindowWidth = winSize.X;
        _options.WindowHeight = winSize.Y;
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
        _options.Resolution = _minResolution - (int)value + 1;
        OnResolutionChanged?.Invoke(_options.Resolution);
    }

    private void OnVSyncModeItemSelected(long index)
    {
        VSyncMode vsyncMode = (VSyncMode)index;
        WindowSetVsyncMode(vsyncMode);
        _options.VSyncMode = vsyncMode;
        _sliderMaxFps.Editable = _options.VSyncMode == VSyncMode.Disabled;
    }

    private void OnMaxFpsValueChanged(double value)
    {
        _labelMaxFpsFeedback.Text = value == 0 ? "UNLIMITED" : value + "";
        _options.MaxFPS = (int)value;
    }

    private void OnMaxFpsDragEnded(bool valueChanged)
    {
        if (!valueChanged)
            return;

        Engine.MaxFps = _options.MaxFPS;
    }
}
