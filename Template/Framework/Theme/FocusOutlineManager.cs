using Godot;
using GodotUtils;

namespace Framework.UI;

public partial class FocusOutlineManager(Node owner) : Component(owner)
{
    // Config
    private readonly float _flashSpeed = 4f;
    private readonly float _minAlpha = 0.35f;
    private readonly float _maxAlpha = 0.7f;

    // Variables
    private NavigationMethod _lastNavigation = NavigationMethod.Mouse;
    private Viewport _viewport;
    private Control _currentFocus;
    private Control _outline;
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
        if (@event is InputEventMouse)
        {
            _lastNavigation = NavigationMethod.Mouse;
        }
        else if (@event is InputEventKey || @event is InputEventJoypadButton)
        {
            _lastNavigation = NavigationMethod.KeyboardOrGamepad;
            //_lastInputTime = (float)(Time.GetTicksUsec() / 1_000_000.0);
        }
    }

    protected override void Process(double delta)
    {
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

    protected override void OnDispose()
    {
        _viewport.GuiFocusChanged -= OnGuiFocusChanged;
    }

    // API
    public void Focus(Control focus)
    {
        _currentFocus = focus;
        _currentFocus.GrabFocus();
        _outline.Show();
        SetProcess(true);
    }

    public void ClearFocus()
    {
        _outline.Hide();
        _currentFocus = null;
        SetProcess(false);
    }

    // Subscribers
    private void OnGuiFocusChanged(Control newFocus)
    {
        _currentFocus = newFocus;

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
    private enum NavigationMethod
    {
        KeyboardOrGamepad,
        Mouse
    }
}
