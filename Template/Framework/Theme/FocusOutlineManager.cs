using Godot;
using GodotUtils;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Tracks keyboard/gamepad focus and renders a pulsing outline around the focused control.
/// </summary>
/// <param name="owner">Owning node used to resolve viewport and outline controls.</param>
public partial class FocusOutlineManager(Node owner) : Component(owner)
{
    // Config
    private readonly float _flashSpeed = 4f;
    private readonly float _minAlpha = 0.35f;
    private readonly float _maxAlpha = 0.7f;

    // Variables
    private NavigationMethod _lastNavigation = NavigationMethod.Mouse;
    private Viewport _viewport = null!;
    private Control? _currentFocus;
    private Control _outline = null!;
    private readonly Node _owner = owner;
    private float _time;

    // Godot Overrides
    protected override void Ready()
    {
        SetPausable(false);

        _outline = _owner.GetNode<Control>("%CornerDashOutline");
        _outline.Hide();
        _viewport = _owner.GetViewport();
        _viewport.GuiFocusChanged += OnGuiFocusChanged;

        SetProcess(false);
        SetInput(true);
    }

    protected override void ProcessInput(InputEvent @event)
    {
        // Mouse movement switches focus visuals to mouse-navigation behavior.
        if (@event is InputEventMouse)
        {
            _lastNavigation = NavigationMethod.Mouse;
        }
        // Keyboard or gamepad input enables keyboard-focus outline behavior.
        else if (@event is InputEventKey || @event is InputEventJoypadButton)
        {
            _lastNavigation = NavigationMethod.KeyboardOrGamepad;
        }
    }

    protected override void Process(double delta)
    {
        // Stop processing when focused control is missing or already freed.
        if (_currentFocus == null || !GodotObject.IsInstanceValid(_currentFocus))
        {
            SetProcess(false);
            return;
        }

        _time += (float)delta;

        // Alpha pulse
        float t = Mathf.Sin(_time * _flashSpeed) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, t);

        // Modulate
        Color c = _outline.Modulate;
        c.A = alpha;
        _outline.Modulate = c;

        // Position and size match the focused control, with padding
        Vector2 padding = new(1, 1);
        _outline.GlobalPosition = _currentFocus.GlobalPosition - padding;
        _outline.Size = _currentFocus.Size + padding * 2;
    }

    protected override void ExitTree()
    {
        _viewport.GuiFocusChanged -= OnGuiFocusChanged;
    }

    // API
    /// <summary>
    /// Moves focus to a control and forces the outline to become visible.
    /// </summary>
    /// <param name="focus">Control that should receive focus.</param>
    public void Focus(Control focus)
    {
        _currentFocus = focus;
        _currentFocus.GrabFocus();
        _outline.Show();
        SetProcess(true);
    }

    /// <summary>
    /// Hides the outline and clears tracked focus state.
    /// </summary>
    public void ClearFocus()
    {
        _outline.Hide();
        _currentFocus = null;
        SetProcess(false);
    }

    // Subscribers
    /// <summary>
    /// Reacts to viewport focus changes and toggles the outline based on navigation method.
    /// </summary>
    /// <param name="newFocus">Control that became focused, if any.</param>
    private void OnGuiFocusChanged(Control newFocus)
    {
        _currentFocus = newFocus;

        // Show outline only for non-mouse navigation with a valid focused control.
        if (_currentFocus != null && _lastNavigation == NavigationMethod.KeyboardOrGamepad)
        {
            _outline.Show();
            SetProcess(true);
        }
        else
        {
            _outline.Hide();
            SetProcess(false);
        }
    }

    // Enums
    /// <summary>
    /// Last input source used to navigate focus.
    /// </summary>
    private enum NavigationMethod
    {
        KeyboardOrGamepad,
        Mouse
    }
}
