using Godot;
using System;
using static __TEMPLATE__.SceneManager;

namespace __TEMPLATE__;

public static class Game
{
    public static void SwitchScene(Node node, Scene scene, TransType transType = TransType.None)
    {
        node.GetNode<SceneManager>(AutoloadPaths.SceneManager).SwitchScene(scene, transType);
    }

    public static void Log(object message, BBColor color = BBColor.Gray)
    {
        LoggerManager.Instance.Logger.Log(message, color);
    }

    public static void Log(params object[] objects)
    {
        LoggerManager.Instance.Logger.Log(objects);
    }

    public static void LogWarning(object message, BBColor color = BBColor.Orange)
    {
        LoggerManager.Instance.Logger.LogWarning(message, color);
    }

    public static void LogErr(Exception e, string hint = null)
    {
        LoggerManager.Instance.Logger.LogErr(e, hint);
    }
}
