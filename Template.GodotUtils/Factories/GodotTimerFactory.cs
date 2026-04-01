using Godot;
using System;

namespace GodotUtils;

/// <summary>
/// Factory helpers for Godot timers.
/// </summary>
public static class GodotTimerFactory
{
    /// <summary>
    /// Creates a one-shot scene tree timer that invokes <paramref name="timeout"/>.
    /// </summary>
    /// <param name="tree">Scene tree used to create the timer.</param>
    /// <param name="seconds">Delay before timeout callback runs.</param>
    /// <param name="timeout">Callback invoked on timeout.</param>
    public static void OneShot(SceneTree tree, double seconds, Action timeout)
    {
        SceneTreeTimer timer = tree.CreateTimer(seconds);
        timer.Timeout += timeout;
    }

    /// <summary>
    /// Creates a one-shot timer as a child of <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Parent node for the timer.</param>
    /// <param name="seconds">Delay before timeout callback runs.</param>
    /// <param name="timeout">Callback invoked on timeout.</param>
    /// <returns>Configured timer instance.</returns>
    public static Timer OneShot(Node node, double seconds, Action timeout)
    {
        return Create(node, seconds, true, timeout);
    }

    /// <summary>
    /// Creates a looping timer as a child of <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Parent node for the timer.</param>
    /// <param name="seconds">Delay between timeout callbacks.</param>
    /// <param name="timeout">Callback invoked on each timeout.</param>
    /// <returns>Configured timer instance.</returns>
    public static Timer Looping(Node node, double seconds, Action timeout)
    {
        return Create(node, seconds, false, timeout);
    }

    /// <summary>
    /// Creates and configures a timer with optional one-shot behavior.
    /// </summary>
    /// <param name="node">Parent node for the timer.</param>
    /// <param name="seconds">Timer wait time in seconds.</param>
    /// <param name="oneShot">Whether timer should auto-free after first timeout.</param>
    /// <param name="timeout">Callback invoked on timeout.</param>
    /// <returns>Configured timer instance.</returns>
    private static Timer Create(Node node, double seconds, bool oneShot, Action timeout)
    {
        Timer timer = new()
        {
            WaitTime = seconds,
            Autostart = true,
            OneShot = oneShot,
        };

        timer.Timeout += OnTimeout;
        timer.TreeExited += OnExitedTree;

        node.AddChild(timer);

        timer.Start();

        return timer;

        void OnTimeout()
        {
            timeout();

            // Dispose one-shot timers after firing once.
            if (oneShot)
                timer.QueueFree();
        }

        void OnExitedTree()
        {
            timer.Timeout -= OnTimeout;
            timer.TreeExited -= OnExitedTree;
        }
    }
}
