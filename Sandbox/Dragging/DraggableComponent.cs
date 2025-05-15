using Godot;

namespace Template;

[GlobalClass]
public partial class DraggableComponent : DragDropBase
{
    [Export(PropertyHint.Range, "0, 1, 0.01")]
    public float LerpFactor { get; set; } = 0.1f;
    
    [Export]
    public bool KeepOffset { get; set; }
}
