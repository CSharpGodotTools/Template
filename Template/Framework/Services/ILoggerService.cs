using GodotUtils;
using System;

namespace __TEMPLATE__;

/// <summary>
/// Defines logging operations for runtime, warning, error, and debug output.
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// Raised when a plain message is logged.
    /// </summary>
    event Action<string>? MessageLogged;

    /// <summary>
    /// Logs a single message with optional color.
    /// </summary>
    /// <param name="message">Message payload.</param>
    /// <param name="color">Message color.</param>
    void Log(object message, BBColor color = BBColor.Gray);

    /// <summary>
    /// Logs multiple objects as a combined message.
    /// </summary>
    /// <param name="objects">Objects to log.</param>
    void Log(params object[] objects);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">Warning payload.</param>
    /// <param name="color">Message color.</param>
    void LogWarning(object message, BBColor color = BBColor.Orange);

    /// <summary>
    /// Logs a todo/reminder message.
    /// </summary>
    /// <param name="message">Todo payload.</param>
    /// <param name="color">Message color.</param>
    void LogTodo(object message, BBColor color = BBColor.White);

    /// <summary>
    /// Logs an exception with optional hint and source context.
    /// </summary>
    /// <param name="e">Exception to log.</param>
    /// <param name="hint">Optional hint message.</param>
    /// <param name="color">Message color.</param>
    /// <param name="filePath">Optional source file path.</param>
    /// <param name="lineNumber">Optional source line number.</param>
    void LogErr(Exception e, string? hint = null, BBColor color = BBColor.Red, string? filePath = null, int lineNumber = 0);

    /// <summary>
    /// Logs debug output with optional trace and source context.
    /// </summary>
    /// <param name="message">Debug payload.</param>
    /// <param name="color">Message color.</param>
    /// <param name="trace">Whether to include trace output.</param>
    /// <param name="filePath">Optional source file path.</param>
    /// <param name="lineNumber">Optional source line number.</param>
    void LogDebug(object message, BBColor color = BBColor.Magenta, bool trace = true, string? filePath = null, int lineNumber = 0);

    /// <summary>
    /// Logs elapsed time in milliseconds for the provided action.
    /// </summary>
    /// <param name="code">Action to measure.</param>
    void LogMs(Action code);

    /// <summary>
    /// Returns whether the logger is still in an active working state.
    /// </summary>
    /// <returns>True when logger reports active work.</returns>
    bool StillWorking();
}
