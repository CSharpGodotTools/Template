#if DEBUG
using Godot;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal partial class VisualizeExampleSprite : Sprite2D
{
    [Visualize] private bool _isEnabled = true;
    [Visualize] private float _rotation;
    [Visualize] private double _zoomFactor = 1.2;
    [Visualize] private string _displayName = "Example Sprite";
    [Visualize] private Color _color = Colors.White;
    [Visualize] private float _skew;
    [Visualize] private Vector2 _offset;

    [Visualize] private Godot.Collections.Array _godotArray = null!;
    [Visualize] private Godot.Collections.Dictionary _godotDict = null!;

    [Visualize]
    private ExampleStruct _structValue = new()
    {
        HitCount = 3
    };

    [Visualize]
    private readonly ExampleClass _classValue = new()
    {
        Name = "Inner Class",
        Speed = 4.5f,
        Values = [10, 20, 30]
    };

    [Visualize]
    private ExampleResource _resourceValue = new()
    {
        Tint = Colors.Coral,
        Curve = new Vector2(2.5f, 1.25f)
    };

    [Visualize] public static int StaticCounter { get; private set; } = 7;

    public override void _EnterTree()
    {
        _godotArray = [1, 2, 3];
        _godotDict = new Godot.Collections.Dictionary
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
    public void PrintPrimitives(bool enabled, int health, float rotation, double zoom, string displayName)
    {
        Visualize.Log($"enabled={enabled}, health={health}, rotation={rotation}, zoom={zoom}, name={displayName}", this);
    }

    [Visualize]
    public void PrintVectors(Vector2 vector2, Vector2I vector2I, Vector3 vector3, Vector3I vector3I, Vector4 vector4, Vector4I vector4I, Quaternion quaternion)
    {
        Visualize.Log($"v2={vector2}, v2i={vector2I}, v3={vector3}, v3i={vector3I}, v4={vector4}, v4i={vector4I}, q={quaternion}", this);
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

    [Visualize]
    public void PrintCollections(int[] array, List<string> list, Dictionary<int, Vector4> dictionary, Godot.Collections.Array<int> godotArray, Godot.Collections.Dictionary<int, bool> godotDictionary)
    {
        int arrayCount = array?.Length ?? 0;
        int listCount = list?.Count ?? 0;
        int dictionaryCount = dictionary?.Count ?? 0;
        int godotArrayCount = godotArray?.Count ?? 0;
        int godotDictionaryCount = godotDictionary?.Count ?? 0;

        Visualize.Log($"array={arrayCount}, list={listCount}, dict={dictionaryCount}, gArray={godotArrayCount}, gDict={godotDictionaryCount}", this);
    }

    [Visualize]
    public void PrintObjectGraph(ExampleStruct structValue, ExampleClass classValue, ExampleResource resourceValue)
    {
        Visualize.Log($"struct={structValue}, class={classValue.Name}:{classValue.Speed}, resourceTint={resourceValue.Tint}", this);
    }

    [Visualize]
    public static void BumpStaticCounter(int value)
    {
        StaticCounter += value;
    }

    public enum SomeEnum
    {
        One,
        Two,
        Three
    }

    public struct ExampleStruct
    {
        [Visualize] public int HitCount;
    }

    public sealed class ExampleClass
    {
        [Visualize] public string Name { get; set; } = "ClassValue";
        [Visualize] public float Speed { get; set; } = 1.0f;
        [Visualize] public List<int> Values { get; set; } = [1, 2, 3];
    }

    public sealed partial class ExampleResource : Resource
    {
        [Visualize] public Color Tint { get; set; } = Colors.White;
        [Visualize] public Vector2 Curve { get; set; } = new(1, 1);
    }
}
#endif
