using Godot;
using GodotUtils;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System;

namespace Template;

/*
 * This is meant to replace all GD.Print(...) with Logger.Log(...) to make
 * logging multi-thread friendly. Remember to put Logger.Update() in
 * _PhysicsProcess(double delta) otherwise you will be wondering why Logger.Log(...)
 * is printing nothing to the console.
 */
public class Logger
{
    public event Action<string> MessageLogged;

    private readonly ConcurrentQueue<LogInfo> _messages = new();

    /// <summary>
    /// Log a message
    /// </summary>
    public void Log(object message, BBColor color = BBColor.Gray)
    {
        _messages.Enqueue(new LogInfo(LoggerOpcode.Message, new LogMessage($"{message}"), color));
    }

    /// <summary>
    /// Logs multiple objects by concatenating them into a single message.
    /// </summary>
    public void Log(params object[] objects)
    {
        if (objects == null || objects.Length == 0)
        {
            return; // or handle the case where no objects are provided
        }

        StringBuilder messageBuilder = new();

        foreach (object obj in objects)
        {
            messageBuilder.Append(obj);
            messageBuilder.Append(' '); // Add a space between objects for readability
        }

        string message = messageBuilder.ToString().Trim(); // Remove the trailing space

        LogInfo logInfo = new(LoggerOpcode.Message, new LogMessage(message), BBColor.Gray);

        _messages.Enqueue(logInfo);
    }

    /// <summary>
    /// Log a warning
    /// </summary>
    public void LogWarning(object message, BBColor color = BBColor.Orange)
    {
        Log($"[Warning] {message}", color);
    }

    /// <summary>
    /// Log a todo
    /// </summary>
    public void LogTodo(object message, BBColor color = BBColor.White)
    {
        Log($"[Todo] {message}", color);
    }

    /// <summary>
    /// Logs an exception with trace information. Optionally allows logging a human readable hint
    /// </summary>
    public void LogErr
    (
        Exception e,
        string hint = default,
        BBColor color = BBColor.Red,
        [CallerFilePath] string filePath = default,
        [CallerLineNumber] int lineNumber = 0
    )
    {
        LogDetailed(LoggerOpcode.Exception, $"[Error] {(string.IsNullOrWhiteSpace(hint) ? "" : $"'{hint}' ")}{e.Message}{e.StackTrace}", color, true, filePath, lineNumber);
    }

    /// <summary>
    /// Logs a debug message that optionally contains trace information
    /// </summary>
    public void LogDebug
    (
        object message,
        BBColor color = BBColor.Magenta,
        bool trace = true,
        [CallerFilePath] string filePath = default,
        [CallerLineNumber] int lineNumber = 0
    )
    {
        LogDetailed(LoggerOpcode.Debug, $"[Debug] {message}", color, trace, filePath, lineNumber);
    }

    /// <summary>
    /// Log the time it takes to do a section of code
    /// </summary>
    public void LogMs(Action code)
    {
        Stopwatch watch = new();
        watch.Start();
        code();
        watch.Stop();
        Log($"Took {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Checks to see if there are any messages left in the queue
    /// </summary>
    public bool StillWorking()
    {
        return !_messages.IsEmpty;
    }

    /// <summary>
    /// Dequeues a Requested Message and Logs it
    /// </summary>
    public void Update()
    {
        if (!_messages.TryDequeue(out LogInfo result))
        {
            return;
        }

        switch (result.Opcode)
        {
            case LoggerOpcode.Message:
                Print(result.Data.Message, result.Color);
                Console.ResetColor();
                break;

            case LoggerOpcode.Exception:
                PrintErr(result.Data.Message, result.Color);

                if (result.Data is LogMessageTrace exceptionData && exceptionData.ShowTrace)
                {
                    PrintErr(exceptionData.TracePath, BBColor.DarkGray);
                }

                Console.ResetColor();
                break;

            case LoggerOpcode.Debug:
                Print(result.Data.Message, result.Color);

                if (result.Data is LogMessageTrace debugData && debugData.ShowTrace)
                {
                    Print(debugData.TracePath, BBColor.DarkGray);
                }

                Console.ResetColor();
                break;
        }

        MessageLogged?.Invoke(result.Data.Message);
    }

    /// <summary>
    /// Logs a message that may contain trace information
    /// </summary>
    private void LogDetailed(LoggerOpcode opcode, string message, BBColor color, bool trace, string filePath, int lineNumber)
    {
        string tracePath;

        if (filePath.Contains("Scripts"))
        {
            // Ex: Scripts/Main.cs:23
            tracePath = $"  at {filePath.Substring(filePath.IndexOf("Scripts"))}:{lineNumber}";
            tracePath = tracePath.Replace(Path.DirectorySeparatorChar, '/');
        }
        else
        {
            // Main.cs:23
            string[] elements = filePath.Split(Path.DirectorySeparatorChar);
            tracePath = $"  at {elements[elements.Length - 1]}:{lineNumber}";
        }

        _messages.Enqueue(
            new LogInfo(opcode,
                new LogMessageTrace(
                    message,
                    trace,
                    tracePath
                ),
            color
        ));
    }

    private void Print(object v, BBColor color)
    {
        //Console.ForegroundColor = color;

        if (GOS.IsExportedRelease())
        {
            GD.Print(v);
        }
        else
        {
            // Full list of BBCode color tags: https://absitomen.com/index.php?topic=331.0
            GD.PrintRich($"[color={color}]{v}");
        }
    }

    private void PrintErr(object v, BBColor color)
    {
        //Console.ForegroundColor = color;
        GD.PrintErr(v);
        GD.PushError(v);
    }
}

public class LogInfo(LoggerOpcode opcode, LogMessage data, BBColor color = BBColor.Gray)
{
    public LoggerOpcode Opcode { get; set; } = opcode;
    public LogMessage Data { get; set; } = data;
    public BBColor Color { get; set; } = color;
}

public class LogMessage(string message)
{
    public string Message { get; set; } = message;
}

public class LogMessageTrace(string message, bool trace = true, string tracePath = default) : LogMessage(message)
{
    // Show the Trace Information for the Message
    public bool ShowTrace { get; set; } = trace;
    public string TracePath { get; set; } = tracePath;
}

public enum LoggerOpcode
{
    Message,
    Exception,
    Debug
}

// Full list of BBCode color tags: https://absitomen.com/index.php?topic=331.0
public enum BBColor
{
    Gray,
    DarkGray,
    Green,
    DarkGreen,
    LightGreen,
    Aqua,
    DarkAqua,
    Deepskyblue,
    Magenta,
    Red,
    White,
    Yellow,
    Orange
}

