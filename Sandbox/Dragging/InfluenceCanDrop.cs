using Godot;

namespace __TEMPLATE__;

public partial class InfluenceCanDrop : Node
{
    public override void _Ready()
    {
        DragManager dragManager = GetParent().GetNode<DragManager>("DragManager");

        dragManager.CanDropInContainer = node =>
        {
            if (node.Name == "Container2")
            {
                GD.Print("[InfluenceCanDrop.cs] (TEST) Prevent dropping any items in Container2");
                return false;
            }
            
            return true;
        };
    }
}
