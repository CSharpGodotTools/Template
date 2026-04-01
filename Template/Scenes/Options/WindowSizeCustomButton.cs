using Godot;
using System;

namespace __TEMPLATE__.Ui;

internal sealed class WindowSizeCustomButtonController : IDisposable
{
    private readonly Button _button;
    private readonly IOptionsService _optionsService;

    private PopupPanel? _popup;
    private LineEdit? _widthInput;
    private LineEdit? _heightInput;
    private Button? _applyButton;
    private readonly Action _onPressed;
    private readonly LineEdit.TextSubmittedEventHandler _onTextSubmitted;
    private readonly Action _onApplyPressed;

    public WindowSizeCustomButtonController(Button button, IOptionsService optionsService)
    {
        _button = button ?? throw new ArgumentNullException(nameof(button));
        _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));

        _onPressed = OnPressed;
        _onTextSubmitted = OnTextSubmitted;
        _onApplyPressed = OnApplyPressed;

        _button.Pressed += _onPressed;
    }

    public void Dispose()
    {
        // Unsubscribe only while source button instance is still valid.
        if (GodotObject.IsInstanceValid(_button))
            _button.Pressed -= _onPressed;

        _widthInput?.TextSubmitted -= _onTextSubmitted;
        _heightInput?.TextSubmitted -= _onTextSubmitted;
        _applyButton?.Pressed -= _onApplyPressed;

        // Free popup only when it exists and is still valid.
        if (_popup is not null && GodotObject.IsInstanceValid(_popup))
            _popup.QueueFree();
    }

    /// <summary>
    /// Opens the custom-size popup, synchronizing inputs with current window dimensions first.
    /// </summary>
    private void OnPressed()
    {
        EnsurePopup();
        SyncInputsFromWindow();
        ShowPopup();
    }

    /// <summary>
    /// Creates popup UI nodes the first time the custom-size button is used.
    /// </summary>
    private void EnsurePopup()
    {
        // Reuse existing popup instance when it already exists.
        if (_popup is not null && GodotObject.IsInstanceValid(_popup))
            return;

        _popup = new PopupPanel { Name = "WindowSizePopup" };

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 14);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_right", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);

        VBoxContainer vbox = new();
        vbox.AddThemeConstantOverride("separation", 10);

        HBoxContainer inputRow = new();
        inputRow.AddThemeConstantOverride("separation", 8);

        _widthInput = new LineEdit
        {
            Name = "WidthInput",
            CustomMinimumSize = new Vector2(80, 0),
            Alignment = HorizontalAlignment.Center,
            PlaceholderText = "X"
        };

        Label separator = new() { Text = "x" };

        _heightInput = new LineEdit
        {
            Name = "HeightInput",
            CustomMinimumSize = new Vector2(80, 0),
            Alignment = HorizontalAlignment.Center,
            PlaceholderText = "Y"
        };

        _applyButton = new Button
        {
            Name = "ApplyButton",
            Text = "Apply"
        };

        _applyButton.Pressed += _onApplyPressed;
        _widthInput.TextSubmitted += _onTextSubmitted;
        _heightInput.TextSubmitted += _onTextSubmitted;

        inputRow.AddChild(_widthInput);
        inputRow.AddChild(separator);
        inputRow.AddChild(_heightInput);

        vbox.AddChild(inputRow);
        vbox.AddChild(_applyButton);

        margin.AddChild(vbox);
        _popup.AddChild(margin);
        _button.GetTree().Root.AddChild(_popup);
    }

    /// <summary>
    /// Shows the popup anchored under the custom-size button.
    /// </summary>
    private void ShowPopup()
    {
        // Guard against calls before popup initialization.
        if (_popup is null)
            return;

        Vector2I buttonPos = (Vector2I)_button.GlobalPosition;
        int y = buttonPos.Y + (int)_button.Size.Y;
        _popup.Popup(new Rect2I(buttonPos.X, y, 0, 0));
    }

    /// <summary>
    /// Refreshes width and height input fields from the current display window size.
    /// </summary>
    private void SyncInputsFromWindow()
    {
        // Guard against calls before input controls are initialized.
        if (_widthInput is null || _heightInput is null)
            return;

        Vector2I size = DisplayServer.WindowGetSize();
        _widthInput.Text = size.X.ToString();
        _heightInput.Text = size.Y.ToString();
    }

    /// <summary>
    /// Handles Enter submission from either size input by applying entered values.
    /// </summary>
    /// <param name="_">Submitted text payload (unused).</param>
    private void OnTextSubmitted(string _)
    {
        OnApplyPressed();
    }

    /// <summary>
    /// Parses and applies custom window size inputs, then stores values in options settings.
    /// </summary>
    private void OnApplyPressed()
    {
        // Guard against apply before popup and inputs are initialized.
        if (_popup is null || _widthInput is null || _heightInput is null)
            return;

        // Avoid changing editor-hosted window size while running embedded.
        if (Engine.IsEmbeddedInEditor())
        {
            _popup.Hide();
            return;
        }

        Vector2I currentSize = DisplayServer.WindowGetSize();
        int width = ParseDimension(_widthInput.Text, currentSize.X, DisplayServer.ScreenGetSize().X);
        int height = ParseDimension(_heightInput.Text, currentSize.Y, DisplayServer.ScreenGetSize().Y);

        _optionsService.Settings.WindowWidth = width;
        _optionsService.Settings.WindowHeight = height;
        _popup.Hide();
    }

    /// <summary>
    /// Parses one dimension input and clamps it to a valid display size range.
    /// </summary>
    /// <param name="text">Raw input text.</param>
    /// <param name="fallback">Fallback value used when parsing fails.</param>
    /// <param name="max">Maximum allowed size for the dimension.</param>
    /// <returns>Clamped dimension value.</returns>
    private static int ParseDimension(string text, int fallback, int max)
    {
        // Fall back to current value when text is not a valid integer.
        if (!int.TryParse(text, out int parsed))
            return Math.Clamp(fallback, 1, Math.Max(1, max));

        int maxValue = Math.Max(1, max);
        return Math.Clamp(parsed, 1, maxValue);
    }
}
