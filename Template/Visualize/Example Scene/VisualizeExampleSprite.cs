using Godot;
using System.Collections.Generic;

namespace Template.Example;

[Visualize(nameof(Position), nameof(Offset), nameof(Rotation))]
public partial class VisualizeExampleSprite : Sprite2D
{
	[Visualize] private Vector2I _position;
    [Visualize] private float _rotation;
    [Visualize] private Color _color = Colors.White;
    [Visualize] private float _skew;
    [Visualize] private Vector2 _offset;

    [OnInstantiate]
    private void Init()
    {

    }

    public override void _PhysicsProcess(double delta)
    {
        Position = _position;
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
