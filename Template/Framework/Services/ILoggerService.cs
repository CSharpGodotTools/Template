using GodotUtils;
using System;

namespace __TEMPLATE__;

public interface ILoggerService
{
    event Action<string>? MessageLogged;

    void Log(object message, BBColor color = BBColor.Gray);
    void Log(params object[] objects);
    void LogWarning(object message, BBColor color = BBColor.Orange);
    void LogTodo(object message, BBColor color = BBColor.White);
    void LogErr(Exception e, string? hint = null, BBColor color = BBColor.Red, string? filePath = null, int lineNumber = 0);
    void LogDebug(object message, BBColor color = BBColor.Magenta, bool trace = true, string? filePath = null, int lineNumber = 0);
    void LogMs(Action code);
    bool StillWorking();
}
