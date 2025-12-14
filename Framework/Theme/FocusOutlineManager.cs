using Godot;

namespace __TEMPLATE__.UI;

public partial class FocusOutlineManager(Node owner) : Component(owner)
{
    [Export] public float FlashSpeed = 4f;
    [Export] public float MinAlpha = 0.35f;
    [Export] public float MaxAlpha = 0.7f;

    private NavigationMethod _lastNavigation = NavigationMethod.Mouse;
    private Control _currentFocus;
    private Control _outline;
    private float _time;
    private Node _owner = owner;

    public override void Ready()
    {
        _outline = _owner.GetNode<Control>("CornerDashOutline");
        _outline.Visible = false;

        _owner.GetViewport().GuiFocusChanged += OnGuiFocusChanged;
        Game.Scene.PreSceneChanged += OnSceneChanged;

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
        }
    }

    private void OnSceneChanged(string sceneName)
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
        float t = Mathf.Sin(_time * FlashSpeed) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(MinAlpha, MaxAlpha, t);

        Color c = _outline.Modulate;
        c.A = alpha;
        _outline.Modulate = c;

        // Position and size match the focused control, with padding
        Vector2 padding = new(1, 1);
        _outline.GlobalPosition = _currentFocus.GlobalPosition - padding;
        _outline.Size = _currentFocus.Size + padding * 2;
    }

    private void OnGuiFocusChanged(Control newFocus)
    {
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
