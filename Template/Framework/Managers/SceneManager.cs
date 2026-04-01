using __TEMPLATE__.Ui;
using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__;

// About Scene Switching: https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html
/// <summary>
/// Coordinates scene transitions, transition effects, and scene-change lifecycle hooks.
/// </summary>
public class SceneManager : ISceneService
{
    // Events
    private const int SceneTransitionLayer = 10;

    /// <summary>
    /// Raised immediately before scene transition work begins.
    /// </summary>
    public event Action? PreSceneChanged;

    /// <summary>
    /// Raised after the new scene has been instantiated and activated.
    /// </summary>
    public event Action? PostSceneChanged;

    // Config
    public const int DefaultSceneFadeDuration = 2;

    // Variables
    private MenuScenes _menuScenes = null!;
    private SceneTree _tree = null!;
    private AutoloadsFramework _autoloads = null!;
    private Node _currentScene = null!;
    private IAudioService? _audioService;
    private FocusOutlineManager? _focusOutline;

    /// <summary>
    /// Creates a scene manager bound to autoload and menu scene references.
    /// </summary>
    /// <param name="autoloads">Framework autoload root.</param>
    /// <param name="scenes">Configured menu scene references.</param>
    public SceneManager(AutoloadsFramework autoloads, MenuScenes scenes)
    {
        SetupFields(autoloads, scenes);

        // Gradually fade out all SFX whenever the scene is changed
        PreSceneChanged += OnPreSceneChanged;
    }

    // API
    /// <summary>
    /// Gets the currently active scene root node.
    /// </summary>
    public Node CurrentScene => _currentScene;

    /// <summary>
    /// Switches to the options scene.
    /// </summary>
    /// <param name="transType">Transition style to apply.</param>
    public void SwitchToOptions(TransType transType = TransType.None) => SwitchTo(_menuScenes.Options, transType);

    /// <summary>
    /// Switches to the main menu scene.
    /// </summary>
    /// <param name="transType">Transition style to apply.</param>
    public void SwitchToMainMenu(TransType transType = TransType.None) => SwitchTo(_menuScenes.MainMenu, transType);

    /// <summary>
    /// Switches to the mod loader scene.
    /// </summary>
    /// <param name="transType">Transition style to apply.</param>
    public void SwitchToModLoader(TransType transType = TransType.None) => SwitchTo(_menuScenes.ModLoader, transType);

    /// <summary>
    /// Switches to the credits scene.
    /// </summary>
    /// <param name="transType">Transition style to apply.</param>
    public void SwitchToCredits(TransType transType = TransType.None) => SwitchTo(_menuScenes.Credits, transType);

    /// <summary>
    /// Switches to a target packed scene with an optional transition effect.
    /// </summary>
    /// <param name="scene">Target scene resource.</param>
    /// <param name="transType">Transition style to apply.</param>
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
        PreSceneChanged?.Invoke();

        // Wait for engine to be ready before switching scenes
        _autoloads.CallDeferred(nameof(AutoloadsFramework.DeferredSwitchSceneProxy), sceneFilePath, Variant.From(TransType.None));
    }

    /// <summary>
    /// Executes the deferred scene switch by replacing the active scene node.
    /// </summary>
    /// <param name="rawName">Path of the scene to load.</param>
    /// <param name="transTypeVariant">Transition payload passed from deferred call site.</param>
    public void DeferredSwitchScene(string rawName, Variant transTypeVariant)
    {
        // Safe to remove scene now
        _currentScene.Free();

        // Load a new scene.
        PackedScene nextScene = GD.Load<PackedScene>(rawName)!;

        // Internal the new scene.
        _currentScene = SceneComposition.InstantiateAndConfigure(nextScene, _autoloads.RuntimeServices);

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
        _focusOutline?.ClearFocus();
    }

    /// <summary>
    /// Binds runtime services used by transition side effects.
    /// </summary>
    /// <param name="audioService">Audio service used for pre-transition fade out.</param>
    /// <param name="focusOutline">Focus manager used to clear stale UI focus after transitions.</param>
    public void BindRuntimeServices(IAudioService audioService, FocusOutlineManager focusOutline)
    {
        _audioService = audioService;
        _focusOutline = focusOutline;
    }

    // Private Methods
    /// <summary>
    /// Initializes core references and captures the current scene root.
    /// </summary>
    /// <param name="autoloads">Framework autoload root.</param>
    /// <param name="scenes">Configured scene references.</param>
    private void SetupFields(AutoloadsFramework autoloads, MenuScenes scenes)
    {
        _autoloads = autoloads;
        _menuScenes = scenes;
        _tree = autoloads.GetTree();

        Window root = _tree.Root;

        // Godot keeps the active gameplay/menu scene as the last child under root.
        _currentScene = root.GetChild(root.GetChildCount() - 1);
    }

    /// <summary>
    /// Handles pre-scene-change audio fade behavior.
    /// </summary>
    private void OnPreSceneChanged() => _audioService?.FadeOutSFX();

    /// <summary>
    /// Requests scene switch through a deferred call to avoid scene-tree mutation timing issues.
    /// </summary>
    /// <param name="scenePath">Scene resource path to load.</param>
    /// <param name="transType">Transition style payload.</param>
    private void ChangeScene(string scenePath, TransType transType)
    {
        // Wait for engine to be ready before switching scenes
        _autoloads.CallDeferred(nameof(AutoloadsFramework.DeferredSwitchSceneProxy), scenePath, Variant.From(transType));
    }

    /// <summary>
    /// Applies a full-screen color fade transition and invokes a continuation callback.
    /// </summary>
    /// <param name="transColor">Fade direction target color state.</param>
    /// <param name="duration">Fade duration in seconds.</param>
    /// <param name="finished">Optional callback invoked when fade finishes.</param>
    private void FadeTo(TransColor transColor, double duration, Action? finished = null)
    {
        // Add canvas layer to scene
        CanvasLayer canvasLayer = new()
        {
            Layer = SceneTransitionLayer
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

    /// <summary>
    /// Unsubscribes transition handlers.
    /// </summary>
    public void Dispose()
    {
        PreSceneChanged -= OnPreSceneChanged;
    }

    // Enums
    /// <summary>
    /// Supported scene transition styles.
    /// </summary>
    public enum TransType
    {
        None,
        Fade
    }

    /// <summary>
    /// Fade target state used by transition rendering.
    /// </summary>
    public enum TransColor
    {
        Black,
        Transparent
    }
}
