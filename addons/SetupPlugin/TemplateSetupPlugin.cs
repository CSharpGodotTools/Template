#if TOOLS
using Godot;
using System;

namespace Framework.Setup;

[Tool]
public partial class TemplateSetupPlugin : EditorPlugin
{
    private EditorDock _dock;

    public override void _EnterTree()
    {
        _dock = new EditorDock
        {
            Title = "Setup",
            DefaultSlot = EditorDock.DockSlot.RightBl
        };

        TemplateSetupDock content = new();
        _dock.AddChild(content);

        AddDock(_dock);
    }

    public override void _ExitTree()
    {
        RemoveDock(_dock);
        _dock.QueueFree();
    }
}
#endif
