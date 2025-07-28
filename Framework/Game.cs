using System;
using static __TEMPLATE__.SceneManager;

namespace __TEMPLATE__;

public static class Game
{
    public static void SwitchScene(Scene scene, TransType transType = TransType.None)
    {
        SceneManager.SwitchScene(scene, transType);
    }

    public static void Log(object message, BBColor color = BBColor.Gray)
    {
        Global.Logger.Log(message, color);
    }

    public static void Log(params object[] objects)
    {
        Global.Logger.Log(objects);
    }

    public static void LogWarning(object message, BBColor color = BBColor.Orange)
    {
        Global.Logger.LogWarning(message, color);
    }

    public static void LogErr(Exception e, string hint = null)
    {
        Global.Logger.LogErr(e, hint);
    }
}
