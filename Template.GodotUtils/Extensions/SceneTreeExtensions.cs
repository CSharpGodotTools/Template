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
    /// <typeparam name="T">Expected scene type.</typeparam>
    /// <param name="tree">Scene tree that owns the current scene.</param>
    /// <returns>Current scene cast to <typeparamref name="T"/> when compatible.</returns>
    public static T? GetCurrentScene<T>(this SceneTree tree) where T : Node
    {
        return tree.CurrentScene as T;
    }

    /// <summary>
    /// Retrieves an autoload from the scene tree using the given name.
    /// </summary>
    /// <typeparam name="T">Expected autoload node type.</typeparam>
    /// <param name="tree">Scene tree whose root contains autoload nodes.</param>
    /// <param name="autoload">Autoload node name registered in project settings.</param>
    /// <returns>Resolved autoload node cast to <typeparamref name="T"/>.</returns>
    public static T GetAutoload<T>(this SceneTree tree, string autoload) where T : Node
    {
        return tree.Root.GetNode<T>($"/root/{autoload}");
    }

    /// <summary>
    /// Removes focus from the currently focused UI control, if any.
    /// </summary>
    /// <param name="tree">Scene tree whose focused control should be cleared.</param>
    public static void UnfocusCurrentControl(this SceneTree tree)
    {
        Control focusedControl = tree.Root.GuiGetFocusOwner();

        // Nothing to unfocus when no control currently owns focus.
        if (focusedControl == null)
            return;

        focusedControl.FocusMode = Control.FocusModeEnum.None;
    }
}
