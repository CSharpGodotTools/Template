using Godot;

namespace GodotUtils;

/// <summary>
/// Base wrapper for area nodes with shapes.
/// </summary>
public abstract class BaseShapeArea<TArea> where TArea : Node
{
    /// <summary>
    /// Gets the underlying area node.
    /// </summary>
    protected TArea Area => _area;

    private readonly TArea _area;

    /// <summary>
    /// Creates a wrapper around the provided area node.
    /// </summary>
    protected BaseShapeArea(TArea area)
    {
        _area = area;
    }

    /// <summary>
    /// Sets the debug color for the area.
    /// </summary>
    public abstract void SetColor(Color color, bool transparent = false);

    /// <summary>
    /// Gets the debug color for the area.
    /// </summary>
    public abstract Color GetColor();

    /// <summary>
    /// Returns the underlying area node.
    /// </summary>
    public static implicit operator TArea(BaseShapeArea<TArea> area) => area._area;
}
