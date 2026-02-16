using Framework.UI.Console;
using Godot;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Framework;

/*
 * This is meant to replace all GD.Print(...) with Logger.Log(...) to make logging multi-thread friendly. 
 * Remember to put Logger.Update() in _PhysicsProcess(double delta) otherwise you will be wondering why 
 * Logger.Log(...) is printing nothing to the console.
 */
public class Logger : IDisposable
{
    public event Action<string> MessageLogged;

    private readonly ConcurrentQueue<LogInfo> _messages = [];
    private readonly GameConsole _console;
    private int _queuedCount;
    private int _droppedCount;
    private int _dropSummaryPending;

    public int MaxLogsPerFrame { get; set; } = 50;
    public int MaxQueueDepth { get; set; } = 2000;

    public Logger() // No game console dependence
    {
    }

    public Logger(GameConsole console)
    {
        _console = console;
        MessageLogged += _console.AddMessage;
    }

    public void Update()
    {
        DequeueMessages();
    }

    // API
    /// <summary>
    /// Log a message
    /// </summary>
    public void Log(object message, BBColor color = BBColor.Gray)
    {
        EnqueueMessage(new LogInfo(LoggerOpcode.Message, new LogMessage($"{message}"), color));
    }

    /// <summary>
    /// Logs multiple objects by concatenating them into a single message.
    /// </summary>
    public void Log(params object[] objects)
    {
        if (objects.Length == 0)
            return;

        StringBuilder messageBuilder = new();
        for (int index = 0; index < objects.Length; index++)
        {
            if (index > 0)
                messageBuilder.Append(' ');

            messageBuilder.Append(objects[index]);
        }

        EnqueueMessage(new LogInfo(LoggerOpcode.Message, new LogMessage(messageBuilder.ToString())));
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
    public void LogErr(
        Exception e,
        string hint = default,
        BBColor color = BBColor.Red,
        [CallerFilePath] string filePath = default,
        [CallerLineNumber] int lineNumber = 0)
    {
        LogDetailed(LoggerOpcode.Exception, $"[Error] {(string.IsNullOrWhiteSpace(hint) ? "" : $"'{hint}' ")}{e.Message}{e.StackTrace}", color, true, filePath, lineNumber);
    }

    /// <summary>
    /// Logs a debug message that optionally contains trace information
    /// </summary>
    public void LogDebug(
        object message,
        BBColor color = BBColor.Magenta,
        bool trace = true,
        [CallerFilePath] string filePath = default,
        [CallerLineNumber] int lineNumber = 0)
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
        return Volatile.Read(ref _queuedCount) > 0;
    }

    // Private Methods
    /// <summary>
    /// Dequeues all requested messages and logs them
    /// </summary>
    private void DequeueMessages()
    {
        int processed = 0;

        while (processed < MaxLogsPerFrame && _messages.TryDequeue(out LogInfo result))
        {
            Interlocked.Decrement(ref _queuedCount);
            processed++;
            DequeueMessage(result);
        }

        MaybeLogDroppedSummary();
    }

    /// <summary>
    /// Dequeues a message and logs it.
    /// </summary>
    /// <param name="result">The information from the message to log</param>
    private void DequeueMessage(LogInfo result)
    {
        switch (result.Opcode)
        {
            case LoggerOpcode.Message:
                Print(result.Data.Message, result.Color);
                break;

            case LoggerOpcode.Exception:
                PrintErr(result.Data.Message);

                if (result.Data is LogMessageTrace exceptionData && exceptionData.ShowTrace)
                    PrintErr(exceptionData.TracePath);

                break;

            case LoggerOpcode.Debug:
                Print(result.Data.Message, result.Color);

                if (result.Data is LogMessageTrace debugData && debugData.ShowTrace)
                    Print(debugData.TracePath, BBColor.DarkGray);

                break;
        }

        Console.ResetColor();
        MessageLogged?.Invoke(result.Data.Message);
    }

    /// <summary>
    /// Logs a message that may contain trace information
    /// </summary>
    private void LogDetailed(LoggerOpcode opcode, string message, BBColor color, bool trace, string filePath, int lineNumber)
    {
        string sourceFile = Path.GetFileName(filePath)!;
        string tracePath = $"  at {sourceFile}:{lineNumber}";

        EnqueueMessage(new LogInfo(opcode, new LogMessageTrace(message, trace, tracePath), color));
    }

    private static void Print(object v, BBColor color)
    {
        //Console.ForegroundColor = color;

        if (EditorUtils.IsExportedRelease())
        {
            GD.Print(v);
        }
        else
        {
            // Full list of BBCode color tags: https://absitomen.com/index.php?topic=331.0
            GD.PrintRich($"[color={color}]{v}");
        }
    }

    private static void PrintErr(object v)
    {
        //Console.ForegroundColor = color;
        GD.PrintErr(v);
        GD.PushError(v);
    }

    private void EnqueueMessage(LogInfo logInfo)
    {
        int queued = Interlocked.Increment(ref _queuedCount);
        if (MaxQueueDepth > 0 && queued > MaxQueueDepth)
        {
            Interlocked.Decrement(ref _queuedCount);
            Interlocked.Increment(ref _droppedCount);
            Interlocked.Exchange(ref _dropSummaryPending, 1);
            return;
        }

        _messages.Enqueue(logInfo);
    }

    private void MaybeLogDroppedSummary()
    {
        if (MaxQueueDepth <= 0)
            return;

        if (Volatile.Read(ref _queuedCount) >= MaxQueueDepth)
            return;

        if (Interlocked.CompareExchange(ref _dropSummaryPending, 0, 1) != 1)
            return;

        int dropped = Interlocked.Exchange(ref _droppedCount, 0);
        if (dropped <= 0)
            return;

        _messages.Enqueue(new LogInfo(LoggerOpcode.Message, new LogMessage($"Logger dropped {dropped} messages due to backlog")));
        Interlocked.Increment(ref _queuedCount);
    }

    // Private Types
    private class LogInfo(LoggerOpcode opcode, LogMessage data, BBColor color = BBColor.Gray)
    {
        public LoggerOpcode Opcode { get; set; } = opcode;
        public LogMessage Data { get; set; } = data;
        public BBColor Color { get; set; } = color;
    }

    private class LogMessage(string message)
    {
        public string Message { get; set; } = message;
    }

    private class LogMessageTrace(string message, bool trace = true, string tracePath = default) : LogMessage(message)
    {
        // Show the Trace Information for the Message
        public bool ShowTrace { get; set; } = trace;
        public string TracePath { get; set; } = tracePath;
    }

    private enum LoggerOpcode
    {
        Message,
        Exception,
        Debug
    }

    // Dispose
    public void Dispose()
    {
        if (_console != null)
            MessageLogged -= _console.AddMessage;

        GC.SuppressFinalize(this);
    }
}
