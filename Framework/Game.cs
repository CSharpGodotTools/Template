using Godot;
using System;
using static __TEMPLATE__.SceneManager;

namespace __TEMPLATE__;

public static class Game
{
    public static void SwitchScene(Node node, Scene scene, TransType transType = TransType.None)
    {
        node.GetNode<SceneManager>(Autoloads.SceneManager).SwitchScene(scene, transType);
    }

    public static void Log(object message, BBColor color = BBColor.Gray)
    {
        Global.Instance.Logger.Log(message, color);
    }

    public static void Log(params object[] objects)
    {
        Global.Instance.Logger.Log(objects);
    }

    public static void LogWarning(object message, BBColor color = BBColor.Orange)
    {
        Global.Instance.Logger.LogWarning(message, color);
    }

    public static void LogErr(Exception e, string hint = null)
    {
        Global.Instance.Logger.LogErr(e, hint);
    }
}
