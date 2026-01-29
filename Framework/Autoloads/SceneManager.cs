using __TEMPLATE__.UI;
using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__;

// About Scene Switching: https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html
public class SceneManager
{
    #region Events
    public event Action PreSceneChanged;
    public event Action PostSceneChanged;
    #endregion

    #region Config
    public const int DefaultSceneFadeDuration = 2;
    #endregion

    #region Variables
    private MenuScenes _menuScenes;
    private SceneTree _tree;
    private Autoloads _autoloads;
    private Node _currentScene;
    #endregion

    public SceneManager(Autoloads autoloads, MenuScenes scenes)
    {
        SetupFields(autoloads, scenes);

        // Gradually fade out all SFX whenever the scene is changed
        PreSceneChanged += OnPreSceneChanged;
    }

    #region API
    public Node CurrentScene => _currentScene;
    public void SwitchToOptions(TransType transType = TransType.None) => SwitchTo(_menuScenes.Options, transType);
    public void SwitchToMainMenu(TransType transType = TransType.None) => SwitchTo(_menuScenes.MainMenu, transType);
    public void SwitchToModLoader(TransType transType = TransType.None) => SwitchTo(_menuScenes.ModLoader, transType);
    public void SwitchToCredits(TransType transType = TransType.None) => SwitchTo(_menuScenes.Credits, transType);

    public void SwitchTo(PackedScene scene, TransType transType = TransType.None)
    {
        ArgumentNullException.ThrowIfNull(scene);
        string path = scene.ResourcePath;
        PreSceneChanged?.Invoke();

        switch (transType)
        {
            case TransType.None:
                ChangeScene(path, transType);
                break;
            case TransType.Fade:
                FadeTo(TransColor.Black, DefaultSceneFadeDuration, () => ChangeScene(path, transType));
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

        PreSceneChanged?.Invoke();

        // Wait for engine to be ready before switching scenes
        _autoloads.CallDeferred(nameof(Autoloads.DeferredSwitchSceneProxy), sceneFilePath, Variant.From(TransType.None));
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

        PostSceneChanged?.Invoke();
        Game.FocusOutline.ClearFocus();
    }
    #endregion

    #region Private Methods
    private void SetupFields(Autoloads autoloads, MenuScenes scenes)
    {
        _autoloads = autoloads;
        _menuScenes = scenes;
        _tree = autoloads.GetTree();

        Window root = _tree.Root;

        _currentScene = root.GetChild(root.GetChildCount() - 1);
    }

    private void OnPreSceneChanged() => Game.Audio.FadeOutSFX();

    private void ChangeScene(string scenePath, TransType transType)
    {
        // Wait for engine to be ready before switching scenes
        _autoloads.CallDeferred(nameof(Autoloads.DeferredSwitchSceneProxy), scenePath, Variant.From(transType));
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
        Tweens.Animate(colorRect, ColorRect.PropertyName.Color)
            .PropertyTo(new Color(0, 0, 0, transColor == TransColor.Black ? 1 : 0), duration)
            .Then(() =>
            {
                canvasLayer.QueueFree();
                finished?.Invoke();
            });
    }
    #endregion

    public void Dispose()
    {
        PreSceneChanged -= OnPreSceneChanged;
    }

    #region Types
    public enum TransType
    {
        None,
        Fade
    }

    public enum TransColor
    {
        Black,
        Transparent
    }
    #endregion
}
