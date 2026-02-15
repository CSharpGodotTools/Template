#if TOOLS
using Godot;

namespace Framework.Setup;

[Tool]
public partial class TemplateSetupPlugin : EditorPlugin
{
    private EditorDock _dock;
    private TemplateSetupDock _content;

    public override void _EnterTree()
    {
        _dock = new EditorDock
        {
            Title = "Setup",
            DefaultSlot = EditorDock.DockSlot.RightBl
        };

        _content = new TemplateSetupDock();
        _dock.AddChild(_content);

        AddDock(_dock);
    }

    public override void _ExitTree()
    {
        if (_dock == null)
        {
            return;
        }

        if (!GodotObject.IsInstanceValid(_dock))
        {
            _content = null;
            _dock = null;
            return;
        }

        if (_content != null && GodotObject.IsInstanceValid(_content))
        {
            _content.PrepareForPluginDisable();
        }

        RemoveDock(_dock);
        _dock.QueueFree();
        _content = null;
        _dock = null;
    }
}
#endif
