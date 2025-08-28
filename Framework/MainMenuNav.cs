using Godot;
using GodotUtils;
using GodotUtils.UI;

namespace __TEMPLATE__.UI;

public partial class MainMenuNav : Node
{
    [Export] private MenuScenes _menuScenes;
    [Export] private PackedScene _gameScene;

    public override void _Ready()
    {
        GetNode<Button>("Play").GrabFocus();
    }

    private void _OnPlayPressed()
    {
        //AudioManager.PlayMusic(Music.Level1, false);
        SceneManager.SwitchScene(_gameScene);
    }

    private void _OnModsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        SceneManager.SwitchScene(_menuScenes.ModLoader);
    }

    private void _OnOptionsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        SceneManager.SwitchScene(_menuScenes.Options);
    }

    private void _OnCreditsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        SceneManager.SwitchScene(_menuScenes.Credits);
    }

    private async void _OnQuitPressed()
    {
        await Autoloads.Instance.QuitAndCleanup();
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
