using Godot;
using GodotUtils;

namespace __TEMPLATE__.UI;

public partial class MainMenuNav : Node
{
    [Export] private PackedScene _gameScene;

    private SceneManager _sceneManager = Game.Scene;

    public override void _Ready()
    {
        GetNode<Button>("Play").GrabFocus();
    }

    private void _OnPlayPressed()
    {
        //AudioManager.PlayMusic(Music.Level1, false);
        _sceneManager.SwitchScene(_gameScene);
    }

    private void _OnModsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        _sceneManager.SwitchScene(_sceneManager.MenuScenes.ModLoader);
    }

    private void _OnOptionsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        _sceneManager.SwitchScene(_sceneManager.MenuScenes.Options);
    }

    private void _OnCreditsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        _sceneManager.SwitchScene(_sceneManager.MenuScenes.Credits);
    }

    private async void _OnQuitPressed()
    {
        await Autoloads.Instance.ExitGame();
    }

    private void _OnDiscordPressed()
    {
        OS.ShellOpen("https://discord.gg/j8HQZZ76r8");
    }

    private void _OnGitHubPressed()
    {
        OS.ShellOpen("https://github.com/ValksGodotTools/Template");
    }
}
