using Godot;
using System;

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
    private Node _owner = owner;
    private Viewport _viewport;
    private PopupMenu _popupMenu;
    //private float _lastInputTime;

    public override void Ready()
    {
        _outline = _owner.GetNode<Control>("%CornerDashOutline");
        _outline.Hide();
        _viewport = _owner.GetViewport();
        _viewport.GuiFocusChanged += OnGuiFocusChanged;
        Game.Scene.PostSceneChanged += OnPostSceneChanged;

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

    public override void Dispose()
    {
        _viewport.GuiFocusChanged -= OnGuiFocusChanged;
        Game.Scene.PostSceneChanged -= OnPostSceneChanged;
    }

    public void RegisterPopupMenu(PopupMenu popupMenu)
    {
        if (_popupMenu != null)
            throw new InvalidOperationException("Popup menu was registered already.");

        _popupMenu = popupMenu;
        _popupMenu.Closed += OnPopupMenuClosed;
        _popupMenu.OptionsClosed += OnOptionsClosed;
    }

    public void UnregisterPopupMenu(PopupMenu popupMenu)
    {
        if (_popupMenu == popupMenu)
        {
            _popupMenu.Closed -= OnPopupMenuClosed;
            _popupMenu.OptionsClosed -= OnOptionsClosed;
            _popupMenu = null;
        }
    }

    private void OnPopupMenuClosed()
    {
        Disable();
    }

    private void OnOptionsClosed()
    {
        Disable();
    }

    /// <summary>
    /// Focused on the targeted <paramref name="focus"/> control.
    /// </summary>
    public void Focus(Control focus)
    {
        Enable(focus);
    }

    private void OnPostSceneChanged()
    {
        Disable();
    }

    private void Enable(Control focus)
    {
        _currentFocus = focus;
        _currentFocus.GrabFocus();
        _outline.Show();
        SetProcess(true);
    }

    private void Disable()
    {
        _outline.Hide();
        _currentFocus = null;
        SetProcess(false);
    }

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

    private enum NavigationMethod
    {
        KeyboardOrGamepad,
        Mouse
    }
}
