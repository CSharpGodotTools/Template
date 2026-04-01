#if DEBUG
using Godot;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Debug sprite containing sample fields and methods used to demonstrate the Visualize runtime.
/// </summary>
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

    /// <summary>
    /// Logs primitive argument values to validate method-parameter visualization and invocation.
    /// </summary>
    /// <param name="enabled">Example boolean flag.</param>
    /// <param name="health">Example integer argument.</param>
    /// <param name="rotation">Example float argument.</param>
    /// <param name="zoom">Example double argument.</param>
    /// <param name="displayName">Example string argument.</param>
    [Visualize]
    public void PrintPrimitives(bool enabled, int health, float rotation, double zoom, string displayName)
    {
        Visualize.Log($"enabled={enabled}, health={health}, rotation={rotation}, zoom={zoom}, name={displayName}", this);
    }

    /// <summary>
    /// Logs representative vector and quaternion arguments for visualization testing.
    /// </summary>
    /// <param name="vector2">Sample <see cref="Vector2"/> value.</param>
    /// <param name="vector2I">Sample <see cref="Vector2I"/> value.</param>
    /// <param name="vector3">Sample <see cref="Vector3"/> value.</param>
    /// <param name="vector3I">Sample <see cref="Vector3I"/> value.</param>
    /// <param name="vector4">Sample <see cref="Vector4"/> value.</param>
    /// <param name="vector4I">Sample <see cref="Vector4I"/> value.</param>
    /// <param name="quaternion">Sample <see cref="Quaternion"/> value.</param>
    [Visualize]
    public void PrintVectors(Vector2 vector2, Vector2I vector2I, Vector3 vector3, Vector3I vector3I, Vector4 vector4, Vector4I vector4I, Quaternion quaternion)
    {
        Visualize.Log($"v2={vector2}, v2i={vector2I}, v3={vector3}, v3i={vector3I}, v4={vector4}, v4i={vector4I}, q={quaternion}", this);
    }

    /// <summary>
    /// Logs dictionary contents in a multi-line format to verify collection visualization output.
    /// </summary>
    /// <param name="dictionary">Dictionary argument to render in the log output.</param>
    [Visualize]
    public void PrintDictionary(Dictionary<int, Vector4> dictionary)
    {
        // Handle null or empty inputs explicitly so log output stays informative.
        if (dictionary == null || dictionary.Count == 0)
        {
            Visualize.Log("Method dictionary param has no elements", this);
        }
        else
        {
            string logMessage = "[\n";

            // Build a readable multi-line payload so each entry is easy to inspect in the debug panel.
            foreach (KeyValuePair<int, Vector4> kvp in dictionary)
            {
                logMessage += $"    {{ {kvp.Key}, {kvp.Value} }},\n";
            }

            logMessage = logMessage.TrimEnd('\n', ',') + "\n]";

            Visualize.Log(logMessage, this);
        }
    }

    /// <summary>
    /// Logs an enum argument to validate enum parameter handling.
    /// </summary>
    /// <param name="someEnum">Enum value supplied from the visualize UI.</param>
    [Visualize]
    public void PrintEnum(SomeEnum someEnum)
    {
        Visualize.Log(someEnum, this);
    }

    /// <summary>
    /// Logs item counts for common managed and Godot collection argument types.
    /// </summary>
    /// <param name="array">Managed array sample.</param>
    /// <param name="list">Managed list sample.</param>
    /// <param name="dictionary">Managed dictionary sample.</param>
    /// <param name="godotArray">Godot typed array sample.</param>
    /// <param name="godotDictionary">Godot typed dictionary sample.</param>
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

    /// <summary>
    /// Logs values from nested object graphs to validate structured argument inspection.
    /// </summary>
    /// <param name="structValue">Sample struct payload.</param>
    /// <param name="classValue">Sample class payload.</param>
    /// <param name="resourceValue">Sample resource payload.</param>
    [Visualize]
    public void PrintObjectGraph(ExampleStruct structValue, ExampleClass classValue, ExampleResource resourceValue)
    {
        Visualize.Log($"struct={structValue}, class={classValue.Name}:{classValue.Speed}, resourceTint={resourceValue.Tint}", this);
    }

    /// <summary>
    /// Increments the static counter to test static member visualization updates.
    /// </summary>
    /// <param name="value">Amount to add to <see cref="StaticCounter"/>.</param>
    [Visualize]
    public static void BumpStaticCounter(int value)
    {
        StaticCounter += value;
    }

    /// <summary>
    /// Example enum used by visualize method parameter tests.
    /// </summary>
    public enum SomeEnum
    {
        One,
        Two,
        Three
    }

    /// <summary>
    /// Example struct used for nested object-graph visualization.
    /// </summary>
    public struct ExampleStruct
    {
        [Visualize] public int HitCount;
    }

    /// <summary>
    /// Example class used for nested object-graph visualization.
    /// </summary>
    public sealed class ExampleClass
    {
        [Visualize] public string Name { get; set; } = "ClassValue";
        [Visualize] public float Speed { get; set; } = 1.0f;
        [Visualize] public List<int> Values { get; set; } = [1, 2, 3];
    }

    /// <summary>
    /// Example resource used for nested object-graph visualization.
    /// </summary>
    public sealed partial class ExampleResource : Resource
    {
        [Visualize] public Color Tint { get; set; } = Colors.White;
        [Visualize] public Vector2 Curve { get; set; } = new(1, 1);
    }
}
#endif
