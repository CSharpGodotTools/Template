using __TEMPLATE__.UI;
using GodotUtils;

namespace __TEMPLATE__;

public partial class LoggerManager : Component
{
    public static LoggerManager Instance { get; private set; }

    public Logger Logger { get; } = new();

    public override void Ready()
    {
        Instance = this;
        Logger.MessageLogged += GetNode<UIConsole>(AutoloadPaths.Console).AddMessage;
    }

    public override void PhysicsProcess(double delta)
    {
        Logger.Update();
    }
}
