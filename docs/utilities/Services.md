> [!IMPORTANT]
> The service attribute is only valid on scripts that extend from Node and the node must be in the scene tree.

```cs
// Services assume there will only ever be one instance of this script.
// All services get cleaned up on scene change. 
public partial class Player : Node
{
    // Use _EnterTree() if the service is not being registered soon enough
    public override void _Ready()
    {
        Services.Register(this);
    }
}
```

```cs
// Get the service from anywhere in your code
Player player = Services.Get<Player>();
```