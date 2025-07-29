using __TEMPLATE__.UI;
using Godot;

namespace __TEMPLATE__;

public partial class LoggerManager : Node
{
    public static LoggerManager Instance { get; private set; }

    public Logger Logger { get; } = new();

    public override void _Ready()
    {
        Instance = this;
        Logger.MessageLogged += GetNode<UIConsole>(Autoloads.Console).AddMessage;
    }

    public override void _PhysicsProcess(double delta)
    {
        Logger.Update();
    }
}
