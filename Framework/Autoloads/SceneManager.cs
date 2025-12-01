using Godot;
using GodotUtils.UI;
using System;

namespace GodotUtils;

// About Scene Switching: https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html
public class SceneManager : IDisposable
{
    /// <summary>
    /// The event is invoked right before the scene is changed
    /// </summary>
    public event Action<string> PreSceneChanged;

    public static SceneManager Instance { get; private set; }
    public MenuScenes MenuScenes { get; private set; }

    private SceneTree _tree;
    private Autoloads _autoloads;
    private Node _currentScene;

    public SceneManager(Autoloads autoloads, MenuScenes scenes)
    {
        if (Instance != null)
            throw new InvalidOperationException($"{nameof(SceneManager)} was initialized already");

        Instance = this;
        _autoloads = autoloads;
        MenuScenes = scenes;
        _tree = autoloads.GetTree();

        Window root = _tree.Root;

        _currentScene = root.GetChild(root.GetChildCount() - 1);

        // Gradually fade out all SFX whenever the scene is changed
        PreSceneChanged += OnPreSceneChanged;
    }

    public void Dispose()
    {
        PreSceneChanged -= OnPreSceneChanged;
        Instance = null;
    }

    private void OnPreSceneChanged(string scene) => AudioManager.FadeOutSFX();

    public static Node GetCurrentScene()
    {
        return Instance._currentScene;
    }

    public static void SwitchScene(PackedScene scene, TransType transType = TransType.None)
    {
        ArgumentNullException.ThrowIfNull(scene);
        string path = scene.ResourcePath;
        Instance.PreSceneChanged?.Invoke(path);

        switch (transType)
        {
            case TransType.None:
                Instance.ChangeScene(path, transType);
                break;
            case TransType.Fade:
                Instance.FadeTo(TransColor.Black, 2, () => Instance.ChangeScene(path, transType));
                break;
        }
    }


    /// <summary>
    /// Resets the currently active scene.
    /// </summary>
    public void ResetCurrentScene()
    {
        string sceneFilePath = _tree.CurrentScene.SceneFilePath;

        string[] words = sceneFilePath.Split("/");
        string sceneName = words[words.Length - 1].Replace(".tscn", "");

        PreSceneChanged?.Invoke(sceneName);

        // Wait for engine to be ready before switching scenes
        _autoloads.CallDeferred(nameof(Autoloads.DeferredSwitchSceneProxy), sceneFilePath, Variant.From(TransType.None));
    }

    private void ChangeScene(string scenePath, TransType transType)
    {
        // Wait for engine to be ready before switching scenes
        _autoloads.CallDeferred(nameof(Autoloads.DeferredSwitchSceneProxy), scenePath, Variant.From(transType));
    }

    public void DeferredSwitchScene(string rawName, Variant transTypeVariant)
    {
        // Safe to remove scene now
        _currentScene.Free();

        // Load a new scene.
        PackedScene nextScene = (PackedScene)GD.Load(rawName);

        // Internal the new scene.
        _currentScene = nextScene.Instantiate();

        // Add it to the active scene, as child of root.
        _tree.Root.AddChild(_currentScene);

        // Optionally, to make it compatible with the SceneTree.change_scene_to_file() API.
        _tree.CurrentScene = _currentScene;

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

    private void FadeTo(TransColor transColor, double duration, Action finished = null)
    {
        // Add canvas layer to scene
        CanvasLayer canvasLayer = new()
        {
            Layer = 10 // render on top of everything else
        };

        _currentScene.AddChild(canvasLayer);

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
        new GodotTween(colorRect)
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
