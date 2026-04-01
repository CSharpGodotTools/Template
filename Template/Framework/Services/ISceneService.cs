using Godot;
using System;

namespace __TEMPLATE__;

/// <summary>
/// Defines scene switching and scene lifecycle notifications.
/// </summary>
public interface ISceneService
{
    /// <summary>
    /// Raised before scene transition starts.
    /// </summary>
    event Action? PreSceneChanged;

    /// <summary>
    /// Raised after scene transition completes.
    /// </summary>
    event Action? PostSceneChanged;

    /// <summary>
    /// Current active scene node.
    /// </summary>
    Node CurrentScene { get; }

    /// <summary>
    /// Switches to options scene.
    /// </summary>
    /// <param name="transType">Transition type.</param>
    void SwitchToOptions(SceneManager.TransType transType = SceneManager.TransType.None);

    /// <summary>
    /// Switches to main menu scene.
    /// </summary>
    /// <param name="transType">Transition type.</param>
    void SwitchToMainMenu(SceneManager.TransType transType = SceneManager.TransType.None);

    /// <summary>
    /// Switches to mod-loader scene.
    /// </summary>
    /// <param name="transType">Transition type.</param>
    void SwitchToModLoader(SceneManager.TransType transType = SceneManager.TransType.None);

    /// <summary>
    /// Switches to credits scene.
    /// </summary>
    /// <param name="transType">Transition type.</param>
    void SwitchToCredits(SceneManager.TransType transType = SceneManager.TransType.None);

    /// <summary>
    /// Switches to an arbitrary packed scene.
    /// </summary>
    /// <param name="scene">Packed scene to instantiate.</param>
    /// <param name="transType">Transition type.</param>
    void SwitchTo(PackedScene scene, SceneManager.TransType transType = SceneManager.TransType.None);

    /// <summary>
    /// Reloads the current scene.
    /// </summary>
    void ResetCurrentScene();
}
