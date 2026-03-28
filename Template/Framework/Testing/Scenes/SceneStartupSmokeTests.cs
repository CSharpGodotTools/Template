using __TEMPLATE__;
using GdUnit4;
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

[TestSuite]
public class SceneStartupSmokeTests
{
    [TestCase]
    [RequireGodotRuntime]
    public static async Task MainMenu_Starts_With_Composed_Dependencies()
    {
        TestOutput.Header(nameof(MainMenu_Starts_With_Composed_Dependencies));
        await SceneStartupSmokeRunner.BootSceneAsync("res://Scenes/MainMenu/MainMenu.tscn");
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task FpsLevel_Starts_With_Composed_Dependencies()
    {
        TestOutput.Header(nameof(FpsLevel_Starts_With_Composed_Dependencies));
        await SceneStartupSmokeRunner.BootSceneAsync("res://MainScenes/3D/FPS/Level.tscn");
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task TopDown2_Starts_With_Composed_Dependencies()
    {
        TestOutput.Header(nameof(TopDown2_Starts_With_Composed_Dependencies));
        await SceneStartupSmokeRunner.BootSceneAsync("res://Framework/Netcode/Examples/TopDown2/World.tscn");
    }
}

internal static class SceneStartupSmokeRunner
{
    private const int StartupFrames = 2;

    public static async Task BootSceneAsync(string scenePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenePath);

        SceneTree tree = Engine.GetMainLoop() as SceneTree
            ?? throw new InvalidOperationException("SceneTree is not available. Use [RequireGodotRuntime] on smoke tests.");

        AutoloadsFramework autoloads = tree.Root.GetNodeOrNull<AutoloadsFramework>("/root/Autoloads")
            ?? throw new InvalidOperationException("Autoloads runtime was not found at '/root/Autoloads'.");

        PackedScene packedScene = GD.Load<PackedScene>(scenePath)
            ?? throw new InvalidOperationException($"Failed to load scene '{scenePath}'.");

        Node? sceneInstance = null;

        try
        {
            sceneInstance = SceneComposition.InstantiateAndConfigure(packedScene, autoloads.RuntimeServices);
            AssertReceiversConfigured(sceneInstance, scenePath);

            tree.Root.AddChild(sceneInstance);
            await WaitFramesAsync(tree, StartupFrames);
        }
        finally
        {
            if (sceneInstance != null && GodotObject.IsInstanceValid(sceneInstance))
            {
                sceneInstance.QueueFree();
                await WaitFramesAsync(tree, 1);
            }
        }
    }

    private static async Task WaitFramesAsync(SceneTree tree, int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }
    }

    private static void AssertReceiversConfigured(Node root, string scenePath)
    {
        int receiverCount = 0;

        foreach (Node node in Enumerate(root))
        {
            if (node is not ISceneDependencyReceiver)
            {
                continue;
            }

            receiverCount++;

            FieldInfo? configuredField = FindConfiguredField(node.GetType());

            if (configuredField is null || configuredField.FieldType != typeof(bool))
            {
                throw new InvalidOperationException(
                    $"Receiver '{node.GetType().Name}' in '{scenePath}' must expose private bool _isConfigured for smoke validation.");
            }

            bool isConfigured = (bool)(configuredField.GetValue(node) ?? false);

            if (!isConfigured)
            {
                throw new InvalidOperationException(
                    $"Receiver '{node.GetType().Name}' in '{scenePath}' was not composed before startup.");
            }
        }

        if (receiverCount == 0)
        {
            throw new InvalidOperationException($"Scene '{scenePath}' does not contain dependency receiver nodes.");
        }
    }

    private static FieldInfo? FindConfiguredField(Type nodeType)
    {
        Type? current = nodeType;

        while (current != null)
        {
            FieldInfo? configuredField = current.GetField(
                "_isConfigured",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            if (configuredField != null)
            {
                return configuredField;
            }

            current = current.BaseType;
        }

        return null;
    }

    private static IEnumerable<Node> Enumerate(Node root)
    {
        yield return root;

        foreach (Node child in root.GetChildren())
        {
            foreach (Node descendant in Enumerate(child))
            {
                yield return descendant;
            }
        }
    }
}
