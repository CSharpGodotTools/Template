All your managers in one place. In this example we add a `WorldManager` instance to our game.

<img width="193" height="248" alt="Untitled" src="https://github.com/user-attachments/assets/0618cc49-a9ac-4cd0-b5e7-924e887cd0bb" />

```cs
// Anything added here will need to be added in Autoloads.cs as well
public partial class Game : GameFramework
{
    // For example:
    // public static WorldManager World => Autoloads.Instance.WorldManager;
}
```

```cs
public partial class Autoloads : AutoloadsFramework
{
    public static Autoloads Instance { get; private set; }

    // For example:
    // public WorldManager WorldManager { get; private set; }

    protected override void EnterTree()
    {
        Instance = this;
        // WorldManager = new WorldManager();
    }

    protected override void Ready()
    {
        // WorldManager.Initialize();
    }

    protected override void Process(double delta)
    {
        // WorldManager.Update(delta);
    }

    protected override void PhysicsProcess(double delta)
    {
        // WorldManager.PhysicsUpdate(delta);
    }

    // Uncomment if _Input is needed
    //public override void _Input(InputEvent @event)
    //{
    //    // WorldManager.Input(@event);
    //}

    protected override void Notification(int what)
    {
        // WorldManager.Notification(what);
    }

    protected override void ExitTree()
    {
        // WorldManager.Dispose();
        Instance = null;
    }
}
```

Then you can do for example the following from anywhere in your game.
```cs
Game.World.(...)
```