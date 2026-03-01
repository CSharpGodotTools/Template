There is a very noticeable gap in performance when we have 10,000 nodes all with their own `_Process` functions. If we use a centralized component manager that handles all processes we notice a [3.5x increase in performance](https://www.reddit.com/r/godot/comments/1mdrjce/you_can_save_a_lot_of_fps_by_centralizing_your/) (This only applies to C# users)

That is why we make use of the component design pattern and do not make our component scripts extend from node. But this can make our root scripts messy. Below is what you would typically do. Imagine we add 10 more component scripts to player, you can see this will get really messy.

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

Instead lets make our component script extend from `Component`.

```cs
public class EntityMovementComponent(Player player) : Component(player)
{
    // Notice these methods do not start with an underscore
    public override void Ready()
    {
        // Process is disabled by default and we must enable it ourselves
        SetProcess(true);
    }

    public override void Process(double delta)
    {
        // Handle process...
    }

    public override void Dispose()
    {
        // Handle dispose...
    }
}
```

Now the player script is super clean!

```cs
public partial class Player : Node
{
    public EntityMovementComponent MovementComponent { get; private set; }

    public override void _Ready()
    {
        MovementComponent = new EntityMovementComponent(this);
    }
}
```

The `ComponentManager` is centralizing all the `Component`'s each containing `_Process`, `_PhysicsProcess`, `_UnhandledInput`, `_Input` methods which drastically increases performance.