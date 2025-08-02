using Godot;
using GodotUtils;
using System;
using static GodotUtils.SceneManager;

namespace __TEMPLATE__;

public static class Game
{
    public static void SwitchScene(Node node, Scene scene, TransType transType = TransType.None)
    {
        node.GetNode<SceneManager>(AutoloadPaths.SceneManager).SwitchScene(scene, transType);
    }

    public static void Log(object message, BBColor color = BBColor.Gray)
    {
        Logger.Instance.Log(message, color);
    }

    public static void Log(params object[] objects)
    {
        Logger.Instance.Log(objects);
    }

    public static void LogWarning(object message, BBColor color = BBColor.Orange)
    {
        Logger.Instance.LogWarning(message, color);
    }

    public static void LogErr(Exception e, string hint = null)
    {
        Logger.Instance.LogErr(e, hint);
    }
}
