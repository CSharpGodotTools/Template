using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for the scene tree.
/// </summary>
public static class SceneTreeExtensions
{
    /// <summary>
    /// Returns the current scene as the specified type.
    /// </summary>
    public static T GetCurrentScene<T>(this SceneTree tree) where T : Node
    {
        return tree.CurrentScene as T;
    }

    /// <summary>
    /// Retrieves an autoload from the scene tree using the given name.
    /// </summary>
    public static T GetAutoload<T>(this SceneTree tree, string autoload) where T : Node
    {
        return tree.Root.GetNode<T>($"/root/{autoload}");
    }

    /// <summary>
    /// Removes focus from the currently focused UI control, if any.
    /// </summary>
    public static void UnfocusCurrentControl(this SceneTree tree)
    {
        Control focusedControl = tree.Root.GuiGetFocusOwner();

        if (focusedControl == null)
            return;

        focusedControl.FocusMode = Control.FocusModeEnum.None;
    }
}
