using Godot;
using ImGuiGodot.Internal;

namespace __TEMPLATE__.UI;

public partial class FocusOutlineManager(Node owner) : Component(owner)
{
    [Export] public float FlashSpeed = 4f;
    [Export] public float MinAlpha = 0.35f;
    [Export] public float MaxAlpha = 1.0f;

    private Control _currentFocus;
    private StyleBoxTexture _currentStyle;
    private float _time;
    private Node _owner = owner;
    private Viewport _viewport;

    public override void Ready()
    {
        _viewport = _owner.GetViewport();
        _viewport.GuiFocusChanged += OnGuiFocusChanged;

        SetProcess(false); // only process when we have a focused button
    }

    public override void Process(double delta)
    {
        if (_currentStyle == null)
            return;

        _time += (float)delta;

        // Semi-transparent sine pulse between MinAlpha and MaxAlpha
        float t = Mathf.Sin(_time * FlashSpeed) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(MinAlpha, MaxAlpha, t);

        Color c = _currentStyle.ModulateColor;
        c.A = alpha;
        _currentStyle.ModulateColor = c;
    }

    public override void Dispose()
    {
        _viewport.GuiFocusChanged -= OnGuiFocusChanged;
    }

    private void OnGuiFocusChanged(Control newFocus)
    {
        // Reset previous stylebox alpha to fully opaque
        if (_currentStyle != null)
        {
            Color c = _currentStyle.ModulateColor;
            c.A = 1.0f;
            _currentStyle.ModulateColor = c;
        }

        _currentFocus = null;
        _currentStyle = null;

        // Animate only if the new focused control is a Button
        if (newFocus is Button btn)
        {
            _currentFocus = btn;
            _currentStyle = btn.GetThemeStylebox("focus") as StyleBoxTexture;
            SetProcess(_currentStyle != null);
        }
        else
        {
            SetProcess(false);
        }
    }
}
