#if DEBUG
using Godot;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal partial class VisualizeExampleSprite : Sprite2D
{
    [Visualize] private float _rotation;
    [Visualize] private Color _color = Colors.White;
    [Visualize] private float _skew;
    [Visualize] private Vector2 _offset;
    [Visualize] private Godot.Collections.Array<int> _godotArray = null!;
    [Visualize] private Godot.Collections.Dictionary<int, bool> _godotDict = null!;

    public override void _EnterTree()
    {
        _godotArray = [1, 2, 3];
        _godotDict = new Godot.Collections.Dictionary<int, bool>
        {
            { 1, false },
            { 2, true },
            { 3, false }
        };
        Visualize.Register(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        Rotation = _rotation;
        Modulate = _color;
        Skew = _skew;
        Offset = _offset;
    }

    [Visualize]
    public void PrintDictionary(Dictionary<int, Vector4> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            Visualize.Log("Method dictionary param has no elements", this);
        }
        else
        {
            string logMessage = "[\n";

            foreach (KeyValuePair<int, Vector4> kvp in dictionary)
            {
                logMessage += $"    {{ {kvp.Key}, {kvp.Value} }},\n";
            }

            logMessage = logMessage.TrimEnd('\n', ',') + "\n]";

            Visualize.Log(logMessage, this);
        }
    }

    [Visualize]
    public void PrintEnum(SomeEnum someEnum)
    {
        Visualize.Log(someEnum, this);
    }

    public enum SomeEnum
    {
        One,
        Two,
        Three
    }
}
#endif
