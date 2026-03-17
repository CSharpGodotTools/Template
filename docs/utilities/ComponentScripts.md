There is a very noticeable gap in performance when we have 10,000 nodes all with their own `_Process` functions. If we use a centralized component manager that handles all processes we notice a [3.5x increase in performance](https://www.reddit.com/r/godot/comments/1mdrjce/you_can_save_a_lot_of_fps_by_centralizing_your/) (This only applies to C# users)

<details>
<summary>Component design typically looks like this.</summary>

```cs
public partial class Player : Node
{
    public EntityMovementComponent MovementComponent { get; private set; }

    public override void _Ready()
    {
        MovementComponent = new EntityMovementComponent(this);
    }

    public override void _Process(double delta)
    {
        MovementComponent.Update(delta);
    }

    public override void _ExitTree()
    {
        MovementComponent.Dispose();
    }
}
```

</details>

But we can do better! Lets extend from `Component`.

```cs
public class EntityMovementComponent(Player player) : Component(player)
{
    // Notice these methods do not start with an underscore
    protected override void Ready()
    {
        // Process is disabled by default and we must enable it ourselves
        SetProcess(true);
    }

    protected override void Process(double delta)
    {
        // Handle process...
    }

    protected override void ExitTree()
    {
        // Handle exit tree...
    }
}
```

And this is what the `Player` script would look like.

```cs
public partial class Player : Node
{
    // ComponentList is completely optional but it improves readability imo
    private ComponentList _components = new();

    public override void _Ready()
    {
        // Add your components
        _components.Add(new EntityMovementComponent(this));

        // Disable all your components if you want
        _components.SetActive(false);
    }

    public override void _ExitTree()
    {
        // Get a component like this
        _components.Get<EntityMovementComponent>().(...)
    }
}
```

The `ComponentManager` is centralizing all the `Component`'s each containing `_Process`, `_PhysicsProcess`, `_UnhandledInput`, `_Input` methods which drastically increases performance.
