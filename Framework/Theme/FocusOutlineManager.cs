using Godot;

namespace __TEMPLATE__.UI;

public partial class FocusOutlineManager(Node owner) : Component(owner)
{
    // Config
    private float _flashSpeed = 4f;
    private float _minAlpha = 0.35f;
    private float _maxAlpha = 0.7f;
    //private float _fadeDelay = 2f;
    //private float _fadeSpeed = 2f;

    private NavigationMethod _lastNavigation = NavigationMethod.Mouse;
    private Control _currentFocus;
    private Control _outline;
    private float _time;
    private bool _ignoreNextFocus;
    private Node _owner = owner;
    //private float _lastInputTime;

    public override void Ready()
    {
        _outline = _owner.GetNode<Control>("CornerDashOutline");
        _outline.Visible = false;

        _owner.GetViewport().GuiFocusChanged += OnGuiFocusChanged;
        Game.Scene.PreSceneChanged += OnPreSceneChanged;

        SetProcess(false);
        SetInput(true);
    }

    public override void ProcessInput(InputEvent @event)
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

    private void OnPreSceneChanged()
    {
        _outline.Visible = false;
        _currentFocus = null;
        SetProcess(false);
    }

    public override void Process(double delta)
    {
        if (_currentFocus == null)
            return;

        _time += (float)delta;

        // Alpha pulse
        float t = Mathf.Sin(_time * _flashSpeed) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, t);

        // Fade out if inactive
        /*double currentTime = Time.GetTicksUsec() / 1_000_000.0; // seconds
        float inactiveTime = (float)(currentTime - _lastInputTime);

        if (inactiveTime > _fadeDelay)
            alpha = Mathf.Lerp(alpha, 0f, (inactiveTime - _fadeDelay) * _fadeSpeed);*/

        // Modulate
        Color c = _outline.Modulate;
        c.A = alpha;
        _outline.Modulate = c;

        // Position and size match the focused control, with padding
        Vector2 padding = new(1, 1);
        _outline.GlobalPosition = _currentFocus.GlobalPosition - padding;
        _outline.Size = _currentFocus.Size + padding * 2;
    }

    /// <summary>
    /// Prevents the focus outline from appearing on next received input.
    /// Use this when setting a control as focused with <c>GrabFocus()</c> but don't 
    /// want the focus outline to show.
    /// </summary>
    public void IgnoreNextFocus()
    {
        _ignoreNextFocus = true;
    }

    private void OnGuiFocusChanged(Control newFocus)
    {
        if (_ignoreNextFocus)
        {
            _ignoreNextFocus = false;
            return;
        }

        _currentFocus = newFocus;

        if (_currentFocus != null && _lastNavigation == NavigationMethod.KeyboardOrGamepad)
        {
            _outline.Visible = true;
            SetProcess(true);
        }
        else
        {
            _outline.Visible = false;
            SetProcess(false);
        }
    }

    private enum NavigationMethod
    {
        Mouse,
        KeyboardOrGamepad
    }
}
