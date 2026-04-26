using __TEMPLATE__.Ui.Console;
using Godot;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace __TEMPLATE__;

/// <summary>
/// Thread-friendly logger that queues messages and flushes them to Godot output on the main thread.
/// </summary>
public class Logger : IDisposable, ILoggerService
{
    /// <summary>
    /// Raised after a log message is emitted.
    /// </summary>
    public event Action<string>? MessageLogged;

    private readonly ConcurrentQueue<LogInfo> _messages = [];
    private readonly GameConsole _console = null!;
    private int _queuedCount;
    private int _droppedCount;
    private int _dropSummaryPending;

    /// <summary>
    /// Gets or sets how many queued log messages are flushed per update call.
    /// </summary>
    public int MaxLogsPerFrame { get; set; } = 50;

    /// <summary>
    /// Gets or sets the maximum queued message depth before new messages are dropped.
    /// Set to 0 or less to disable depth limiting.
    /// </summary>
    public int MaxQueueDepth { get; set; } = 2000;

    /// <summary>
    /// Creates a logger with no game-console dependency.
    /// </summary>
    public Logger()
    {
    }

    /// <summary>
    /// Creates a logger that forwards emitted messages to the in-game console.
    /// </summary>
    /// <param name="console">Console sink that receives emitted messages.</param>
    public Logger(GameConsole console)
    {
        _console = console;
        MessageLogged += _console.AddMessage;
    }

    /// <summary>
    /// Flushes queued log messages according to current per-frame limits.
    /// </summary>
    public void Update()
    {
        DequeueMessages();
    }

    // API
    /// <summary>
    /// Log a message
    /// </summary>
    /// <param name="message">Message payload to emit.</param>
    /// <param name="color">Color token used when rendering the message.</param>
    public void Log(object message, BBColor color = BBColor.Gray)
    {
        EnqueueMessage(new LogInfo(LoggerOpcode.Message, new LogMessage($"{message}"), color));
    }

    /// <summary>
    /// Logs multiple objects by concatenating them into a single message.
    /// </summary>
    /// <param name="objects">Objects to join into a single log line.</param>
    public void Log(params object[] objects)
    {
        // Ignore empty variadic log requests.
        if (objects.Length == 0)
            return;

        StringBuilder messageBuilder = new();
        for (int index = 0; index < objects.Length; index++)
        {
            // Insert spaces between concatenated object segments.
            if (index > 0)
                messageBuilder.Append(' ');

            messageBuilder.Append(objects[index]);
        }

        EnqueueMessage(new LogInfo(LoggerOpcode.Message, new LogMessage(messageBuilder.ToString())));
    }

    /// <summary>
    /// Log a warning
    /// </summary>
    /// <param name="message">Warning payload to emit.</param>
    /// <param name="color">Color token used for warning output.</param>
    public void LogWarning(object message, BBColor color = BBColor.Orange)
    {
        Log($"[Warning] {message}", color);
    }

    /// <summary>
    /// Log a todo
    /// </summary>
    /// <param name="message">Todo payload to emit.</param>
    /// <param name="color">Color token used for todo output.</param>
    public void LogTodo(object message, BBColor color = BBColor.White)
    {
        Log($"[Todo] {message}", color);
    }

    /// <summary>
    /// Logs an exception with trace information. Optionally allows logging a human readable hint
    /// </summary>
    /// <param name="e">Exception to log.</param>
    /// <param name="hint">Optional human-readable context added before exception text.</param>
    /// <param name="color">Color token used for exception output.</param>
    /// <param name="filePath">Caller file path captured by compiler services.</param>
    /// <param name="lineNumber">Caller line number captured by compiler services.</param>
    public void LogErr(
        Exception e,
        string? hint = null,
        BBColor color = BBColor.Red,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        LogDetailed(LoggerOpcode.Exception, $"[Error] {(string.IsNullOrWhiteSpace(hint) ? "" : $"'{hint}' ")}{e.Message}{e.StackTrace}", color, true, filePath, lineNumber);
    }

    /// <summary>
    /// Logs a debug message that optionally contains trace information
    /// </summary>
    /// <param name="message">Debug payload to emit.</param>
    /// <param name="color">Color token used for debug output.</param>
    /// <param name="trace">Whether to include caller trace information.</param>
    /// <param name="filePath">Caller file path captured by compiler services.</param>
    /// <param name="lineNumber">Caller line number captured by compiler services.</param>
    public void LogDebug(
        object message,
        BBColor color = BBColor.Magenta,
        bool trace = true,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        LogDetailed(LoggerOpcode.Debug, $"[Debug] {message}", color, trace, filePath, lineNumber);
    }

    /// <summary>
    /// Log the time it takes to do a section of code
    /// </summary>
    /// <param name="code">Synchronous action to execute and time.</param>
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
    /// <returns><see langword="true"/> when queued messages remain to be processed.</returns>
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

        while (processed < MaxLogsPerFrame && _messages.TryDequeue(out LogInfo? result))
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

                // Print trace details when exception payload requests tracing.
                if (result.Data is LogMessageTrace exceptionData && exceptionData.ShowTrace)
                    PrintErr(exceptionData.TracePath!);

                break;

            case LoggerOpcode.Debug:
                Print(result.Data.Message, result.Color);

                // Print trace details when debug payload requests tracing.
                if (result.Data is LogMessageTrace debugData && debugData.ShowTrace)
                    Print(debugData.TracePath!, BBColor.DarkGray);

                break;
        }

        Console.ResetColor();
        MessageLogged?.Invoke(result.Data.Message);
    }

    /// <summary>
    /// Logs a message that may contain trace information
    /// </summary>
    /// <param name="opcode">Message opcode describing output behavior.</param>
    /// <param name="message">Formatted message text.</param>
    /// <param name="color">Color token used for rendering.</param>
    /// <param name="trace">Whether to include caller trace information.</param>
    /// <param name="filePath">Caller file path captured by compiler services.</param>
    /// <param name="lineNumber">Caller line number captured by compiler services.</param>
    private void LogDetailed(LoggerOpcode opcode, string message, BBColor color, bool trace, string? filePath, int lineNumber)
    {
        string sourceFile = Path.GetFileName(filePath)!;
        string tracePath = $"  at {sourceFile}:{lineNumber}";

        EnqueueMessage(new LogInfo(opcode, new LogMessageTrace(message, trace, tracePath), color));
    }

    /// <summary>
    /// Emits a standard message using rich text in debug/editor builds.
    /// </summary>
    /// <param name="v">Message payload.</param>
    /// <param name="color">BBCode color token.</param>
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

    /// <summary>
    /// Emits an error message and pushes it into Godot's error channel.
    /// </summary>
    /// <param name="v">Error payload.</param>
    private static void PrintErr(object v)
    {
        //Console.ForegroundColor = color;
        GD.PrintErr(v);
        GD.PushError(v);
    }

    /// <summary>
    /// Enqueues a log message, applying queue-depth backpressure when configured.
    /// </summary>
    /// <param name="logInfo">Log payload to enqueue.</param>
    private void EnqueueMessage(LogInfo logInfo)
    {
        int queued = Interlocked.Increment(ref _queuedCount);

        // Drop message when queue depth exceeds configured limit.
        if (MaxQueueDepth > 0 && queued > MaxQueueDepth)
        {
            // Roll back queue depth and record a dropped-message summary for later emission.
            Interlocked.Decrement(ref _queuedCount);
            Interlocked.Increment(ref _droppedCount);
            Interlocked.Exchange(ref _dropSummaryPending, 1);
            return;
        }

        _messages.Enqueue(logInfo);
    }

    /// <summary>
    /// Emits a single backlog summary once queue pressure subsides.
    /// </summary>
    private void MaybeLogDroppedSummary()
    {
        // Skip drop-summary behavior when queue limiting is disabled.
        if (MaxQueueDepth <= 0)
            return;

        // Wait until the queue drains below threshold before logging the drop summary.
        if (Volatile.Read(ref _queuedCount) >= MaxQueueDepth)
            return;

        // Ensure only one caller emits the drop summary.
        if (Interlocked.CompareExchange(ref _dropSummaryPending, 0, 1) != 1)
            return;

        int dropped = Interlocked.Exchange(ref _droppedCount, 0);

        // Skip summary when no dropped messages were recorded.
        if (dropped <= 0)
            return;

        _messages.Enqueue(new LogInfo(LoggerOpcode.Message, new LogMessage($"Logger dropped {dropped} messages due to backlog")));
        Interlocked.Increment(ref _queuedCount);
    }

    // Private Types
    /// <summary>
    /// Internal queued log record.
    /// </summary>
    /// <param name="opcode">Message opcode describing how to emit this record.</param>
    /// <param name="data">Message payload data.</param>
    /// <param name="color">Color token used when rendering output.</param>
    private class LogInfo(LoggerOpcode opcode, LogMessage data, BBColor color = BBColor.Gray)
    {
        public LoggerOpcode Opcode { get; set; } = opcode;
        public LogMessage Data { get; set; } = data;
        public BBColor Color { get; set; } = color;
    }

    /// <summary>
    /// Base log message payload.
    /// </summary>
    /// <param name="message">Raw message text.</param>
    private class LogMessage(string message)
    {
        public string Message { get; set; } = message;
    }

    /// <summary>
    /// Log payload that optionally carries source trace details.
    /// </summary>
    /// <param name="message">Raw message text.</param>
    /// <param name="trace">Whether trace output should be shown.</param>
    /// <param name="tracePath">Resolved caller source path text.</param>
    private class LogMessageTrace(string message, bool trace = true, string? tracePath = null) : LogMessage(message)
    {
        // Show the Trace Information for the Message
        public bool ShowTrace { get; set; } = trace;
        public string? TracePath { get; set; } = tracePath;
    }

    /// <summary>
    /// Internal opcode describing how a queued message should be emitted.
    /// </summary>
    private enum LoggerOpcode
    {
        Message,
        Exception,
        Debug
    }

    // Dispose
    /// <summary>
    /// Detaches console subscriptions.
    /// </summary>
    public void Dispose()
    {
        // Detach console forwarding when logger was bound to a console sink.
        if (_console != null)
            MessageLogged -= _console.AddMessage;
    }
}
