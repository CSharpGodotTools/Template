using Godot;
using System;

namespace __TEMPLATE__;

public interface ISceneService
{
    event Action? PreSceneChanged;
    event Action? PostSceneChanged;

    Node CurrentScene { get; }

    void SwitchToOptions(SceneManager.TransType transType = SceneManager.TransType.None);
    void SwitchToMainMenu(SceneManager.TransType transType = SceneManager.TransType.None);
    void SwitchToModLoader(SceneManager.TransType transType = SceneManager.TransType.None);
    void SwitchToCredits(SceneManager.TransType transType = SceneManager.TransType.None);
    void SwitchTo(PackedScene scene, SceneManager.TransType transType = SceneManager.TransType.None);
    void ResetCurrentScene();
}
