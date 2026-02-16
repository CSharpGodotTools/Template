using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for canvas items.
/// </summary>
public static class CanvasItemExtensions
{
    /// <summary>
    /// Applies an unshaded material to the canvas item.
    /// </summary>
    public static void SetUnshaded(this CanvasItem canvasItem)
    {
        canvasItem.Material = new CanvasItemMaterial
        {
            LightMode = CanvasItemMaterial.LightModeEnum.Unshaded
        };
    }

    /// <summary>
    /// Converts the canvas item position to a screen position.
    /// </summary>
    public static Vector2 GetScreenPosition(this CanvasItem canvasItem)
    {
        // Code retrieved from https://www.reddit.com/r/godot/comments/1aq1f0b/comment/kqa6z0u/
        // Relevant Godot Docs at https://docs.godotengine.org/en/stable/tutorials/2d/2d_transforms.html#transform-functions
        Window root = canvasItem.GetTree().Root;

        return (root.GetFinalTransform() * canvasItem.GetGlobalTransformWithCanvas()).Origin 
            + root.Position;
    }

    /// <summary>
    /// Sets the canvas item fully transparent and returns it.
    /// </summary>
    public static CanvasItem SetTransparent(this CanvasItem canvasItem)
    {
        canvasItem.SelfModulate = new Color(1, 1, 1, 0);
        return canvasItem;
    }
}
