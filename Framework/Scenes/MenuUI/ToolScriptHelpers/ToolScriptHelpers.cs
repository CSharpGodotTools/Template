using Godot;
using GodotUtils;

namespace __TEMPLATE__.UI;

[Tool]
public partial class ToolScriptHelpers : Node
{
    [Export] 
    public bool RemoveEmptyFolders
    {
        get => false;
        set => DeleteEmptyFolders();
    }

    private static void DeleteEmptyFolders()
    {
        if (!Engine.IsEditorHint()) // Do not trigger on game build
            return;

        DirectoryUtils.DeleteEmptyDirectories("res://");

        GD.Print("Removed all empty folders from the project. Restart the game engine or wait some time to see the effect.");
    }
}
