using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__;

public static class Scene 
{
    public const string MainMenu  = "res://Scenes/MenuUI/MainMenu.tscn";
    public const string ModLoader = "res://Scenes/MenuUI/ModLoader.tscn";
    public const string Options   = "res://Scenes/MenuUI/Options.tscn";
    public const string Credits   = "res://Scenes/MenuUI/Credits.tscn";
    public const string Game      = "res://Genres/2D Top Down/Level.tscn";
}

public static class Prefab 
{
    
}

// About Scene Switching: https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html
public partial class SceneManager : Node
{
    /// <summary>
    /// The event is invoked right before the scene is changed
    /// </summary>
    public static event Action<string> PreSceneChanged;

    public static Node CurrentScene { get; private set; }

    private static SceneTree _tree;
    private static SceneManager _instance;

    public override void _Ready()
    {
        _instance = this;
        _tree = GetTree();
        Window root = _tree.Root;
        CurrentScene = root.GetChild(root.GetChildCount() - 1);

        // Gradually fade out all SFX whenever the scene is changed
        PreSceneChanged += _ => AudioManager.FadeOutSFX();
    }

    public static void SwitchScene(string scenePath, TransType transType = TransType.None)
    {
        PreSceneChanged?.Invoke(scenePath);

        switch (transType)
        {
            case TransType.None:
                ChangeScene(scenePath, transType);
                break;
            case TransType.Fade:
                FadeTo(TransColor.Black, 2, () => ChangeScene(scenePath, transType));
                break;
        }
    }

    /// <summary>
    /// Resets the currently active scene.
    /// </summary>
    public static void ResetCurrentScene()
    {
        string sceneFilePath = _tree.CurrentScene.SceneFilePath;

        string[] words = sceneFilePath.Split("/");
        string sceneName = words[words.Length - 1].Replace(".tscn", "");

        PreSceneChanged?.Invoke(sceneName);

        // Wait for engine to be ready before switching scenes
        _instance.CallDeferred(nameof(DeferredSwitchScene), sceneFilePath, Variant.From(TransType.None));
    }

    private static void ChangeScene(string scenePath, TransType transType)
    {
        // Wait for engine to be ready before switching scenes
        _instance.CallDeferred(nameof(DeferredSwitchScene), scenePath,
            Variant.From(transType));
    }

    private static void DeferredSwitchScene(string rawName, Variant transTypeVariant)
    {
        // Safe to remove scene now
        CurrentScene.Free();

        // Load a new scene.
        PackedScene nextScene = (PackedScene)GD.Load(rawName);

        // Internal the new scene.
        CurrentScene = nextScene.Instantiate();

        // Add it to the active scene, as child of root.
        _tree.Root.AddChild(CurrentScene);

        // Optionally, to make it compatible with the SceneTree.change_scene_to_file() API.
        _tree.CurrentScene = CurrentScene;

        TransType transType = transTypeVariant.As<TransType>();

        switch (transType)
        {
            case TransType.None:
                break;
            case TransType.Fade:
                FadeTo(TransColor.Transparent, 1);
                break;
        }
    }

    private static void FadeTo(TransColor transColor, double duration, Action finished = null)
    {
        // Add canvas layer to scene
        CanvasLayer canvasLayer = new()
        {
            Layer = 10 // render on top of everything else
        };

        CurrentScene.AddChild(canvasLayer);

        // Setup color rect
        ColorRect colorRect = new()
        {
            Color = new Color(0, 0, 0, transColor == TransColor.Black ? 0 : 1),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        // Make the color rect cover the entire screen
        colorRect.SetLayout(Control.LayoutPreset.FullRect);
        canvasLayer.AddChild(colorRect);

        // Animate color rect
        new GTween(colorRect)
            .Animate(ColorRect.PropertyName.Color, new Color(0, 0, 0, transColor == TransColor.Black ? 1 : 0), duration)
            .Callback(() =>
            {
                canvasLayer.QueueFree();
                finished?.Invoke();
            });
    }

    public enum TransType
    {
        None,
        Fade
    }

    private enum TransColor
    {
        Black,
        Transparent
    }
}

