Visual in-game debugging!

https://github.com/user-attachments/assets/1fe282b9-f044-42cd-b9be-0e26f20b1aa6

https://github.com/user-attachments/assets/2f44ae8e-0c99-4bd2-b15f-a72a70ffaa74

#### Example Usage 1
```cs
// Works in both 2D and 3D scenes
public partial class Player : CharacterBody3D
{
    [Visualize]
    private int _x;

    public override void _Ready()
    {
        Visualize.Register(this);
    }

    [Visualize]
    public void IncrementX(int amount)
    {
        _x += amount;
        Visualize.Log(amount);
    }
}
```

#### Example Usage 2
```cs
// Scripts do not have to extend from Node for Visualize to work
public class PlayerMovementComponent
{
    [Visualize]
    private float _y;

    public PlayerMovementComponent(Player player)
    {
        Visualize.Register(this, player);
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
| **Godot Array**   | ✅         | `Godot.Collections.Array<int>`                | Both generic and non-generic types are supported.                     |
| **Godot Dictionary** | ✅      | `Godot.Collections.Dictionary<int, bool>`     | Both generic and non-generic types are supported.                     |
| **Godot Classes** | ❌         | `PointLight2D`, `CharacterBody3D`             |                                                                       |

> [!NOTE]
> No mouse interactions have been implemented in 3D, so you will only be able to use it for read only.
