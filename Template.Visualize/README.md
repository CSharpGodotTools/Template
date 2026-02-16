Easily debug in-game by adding the `[Visualize]` attribute to any of the supported members. This feature allows you to visualize and interact with various types of data directly within the game environment.

https://github.com/user-attachments/assets/1fe282b9-f044-42cd-b9be-0e26f20b1aa6

https://github.com/user-attachments/assets/2f44ae8e-0c99-4bd2-b15f-a72a70ffaa74

#### Example Usage
```cs
public partial class Player : CharacterBody2D
{
    // You will be able to edit this in-game
    [Visualize] private static int _totalPlayers;

    private State _currentState;

    public override void _Ready()
    {
        // Visualize.Register(this) is required for the tool to even see this script, every other parameter is optional
        // currentState and Rotation will be observed as readonly members.
        Visualize.Register(this, nameof(_currentState), nameof(Rotation));
        _currentState = new State("Idle");
    }

    // You will be able to execute this method in-game
    [Visualize]
    public void Attack(int damage)
    {
        Visualize.Log(damage); // Floating text will appear near node then disappear
    }
}
```

#### Supported Members

| Member Type       | Supported  | Example Types                                 | Additional Notes                                                      |
|-------------------|------------|-----------------------------------------------|-----------------------------------------------------------------------|
| **Numericals**    | ✅         | `int`, `float`, `double`                      | All numerical types are supported                                     |
| **Enums**         | ✅         | `Direction`, `Colors`                         | All enum types are supported                                          |
| **Booleans**      | ✅         | `bool`                                        |                                                                       |
| **Strings**       | ✅         | `string`                                      |                                                                       |
| **Color**         | ✅         | `Color`                                       |                                                                       |
| **Vectors**       | ✅         | `Vector2`, `Vector2I`, `Vector3`, `Vector3I`, `Vector4`, `Vector4I` |                                                 |
| **Quaternion**    | ✅         | `Quaternion`                                  |                                                                       |
| **NodePath**      | ✅         | `NodePath`                                    |                                                                       |
| **StringName**    | ✅         | `StringName`                                  |                                                                       |
| **Methods**       | ✅         |                                               | Method parameters support all listed types here                       |
| **Static Members**| ✅         |                                               | This includes static methods, fields, and properties                  |
| **Arrays**        | ✅         | `int[]`, `string[]`, `Vector2[]`              | Arrays support all listed types here                                  |
| **Lists**         | ✅         | `List<string[]>`, `List<Vector2>`             | Lists support all listed types here                                   |
| **Dictionaries**  | ✅         | `Dictionary<List<Color[]>, Vector2>`          | Dictionaries support all listed types here                            |
| **Structs**       | ✅         | `struct`                                      |                                                                       |
| **Classes**       | ✅         | `class`                                       |                                                                       |
| **Resources**     | ✅         | `Resource`                                    |                                                                       |
| **Godot Classes** | ❌         | `PointLight2D`, `CharacterBody3D`             |                                                                       |
| **Godot Array**   | ❌         | `Godot.Collections.Array`                     |                                                                       |
| **Godot Dictionary** | ❌      | `Godot.Collections.Dictionary`                |                                                                       |

> [!IMPORTANT]
> There are some annoyances when trying to visualize members from **inherited classes**. I will try to solve this later.

#### ⚠️ Common Mistakes ⚠️
If the script is placed on a `Node2D` or `Control` but the script itself does not extend from `Node2D` or `Control` the visual panel will be placed at `(0, 0)`. There is no way for me to check for this, you will just have to remember to make the script extend from the appropriate type.

If `Visualize.Register(this)` is called in `_EnterTree`, you will get a null reference exception. This is because Visualize did not have time to initialize and you must instead call it in `_Ready`.
