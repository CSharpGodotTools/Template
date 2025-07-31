using __TEMPLATE__.UI;
using GodotUtils;

namespace __TEMPLATE__;

public partial class LoggerManager : Component
{
    public static LoggerManager Instance { get; private set; }

    public Logger Logger { get; } = new();

    public override void Ready()
    {
        ComponentManager.RegisterPhysicsProcess(this);
        Instance = this;
        Logger.MessageLogged += GetNode<UIConsole>(AutoloadPaths.Console).AddMessage;
        SetPhysicsProcess(false);
    }

    public override void PhysicsProcess(double delta)
    {
        Logger.Update();
    }
}
