using Godot;
using GodotUtils;
using GodotUtils.UI;
using System;

using static Godot.DisplayServer;
using WindowMode = GodotUtils.WindowMode;

namespace __TEMPLATE__.UI;

public partial class OptionsDisplay : Control
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

    public override void _Ready()
    {
        _options = OptionsManager.GetOptions();

        SetupMaxFps();
        SetupWindowSize();
        SetupWindowMode();
        SetupResolution();
        SetupVSyncMode();
    }

    private void _OnWindowModeItemSelected(int index)
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

    private void _OnWindowWidthTextChanged(string text)
    {
        text.ValidateNumber(_resX, 0, ScreenGetSize().X, ref _prevNumX);
    }

    private void _OnWindowHeightTextChanged(string text)
    {
        text.ValidateNumber(_resY, 0, ScreenGetSize().Y, ref _prevNumY);
    }

    private void _OnWindowWidthTextSubmitted(string text) => ApplyWindowSize();
    private void _OnWindowHeightTextSubmitted(string text) => ApplyWindowSize();
    private void _OnWindowSizeApplyPressed() => ApplyWindowSize();

    private void _OnResolutionValueChanged(float value)
    {
        _options.Resolution = _minResolution - (int)value + 1;
        OnResolutionChanged?.Invoke(_options.Resolution);
    }

    private void _OnVSyncModeItemSelected(int index)
    {
        VSyncMode vsyncMode = (VSyncMode)index;
        WindowSetVsyncMode(vsyncMode);
        _options.VSyncMode = vsyncMode;
        _sliderMaxFps.Editable = _options.VSyncMode == VSyncMode.Disabled;
    }

    private void _OnMaxFpsValueChanged(float value)
    {
        _labelMaxFpsFeedback.Text = value == 0 ? "UNLIMITED" : value + "";
        _options.MaxFPS = (int)value;
    }

    private void _OnMaxFpsDragEnded(bool valueChanged)
    {
        if (!valueChanged)
            return;

        Engine.MaxFps = _options.MaxFPS;
    }

    private void SetupMaxFps()
    {
        _labelMaxFpsFeedback = GetNode<Label>("%MaxFPSFeedback");
        _labelMaxFpsFeedback.Text = _options.MaxFPS == 0 ? "UNLIMITED" : _options.MaxFPS + "";

        _sliderMaxFps = GetNode<HSlider>("%MaxFPS");
        _sliderMaxFps.Value = _options.MaxFPS;
        _sliderMaxFps.Editable = _options.VSyncMode == VSyncMode.Disabled;
    }

    private void SetupWindowSize()
    {
        _resX = GetNode<LineEdit>("%WindowWidth");
        _resY = GetNode<LineEdit>("%WindowHeight");

        Vector2I winSize = DisplayServer.WindowGetSize();

        _prevNumX = winSize.X;
        _prevNumY = winSize.Y;

        _resX.Text = winSize.X + "";
        _resY.Text = winSize.Y + "";
    }

    private void SetupWindowMode()
    {
        OptionButton optionBtnWindowMode = GetNode<OptionButton>("%WindowMode");
        optionBtnWindowMode.Select((int)_options.WindowMode);

        OptionsManager.Instance.WindowModeChanged += windowMode =>
        {
            if (!IsInstanceValid(optionBtnWindowMode))
                return;

            // Window mode select button could be null. If there was no null check
            // here then we would be assuming that the user can only change fullscreen
            // when in the options screen but this is not the case.
            optionBtnWindowMode.Select((int)windowMode);
        };
    }

    private void SetupResolution()
    {
        GetNode<HSlider>("%Resolution").Value = 1 + _minResolution - _options.Resolution;
    }

    private void SetupVSyncMode()
    {
        GetNode<OptionButton>("%VSyncMode").Select((int)_options.VSyncMode);
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
}
