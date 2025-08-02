using Godot;
using GodotUtils;

namespace __TEMPLATE__.UI;

public partial class MainMenuNav : Node
{
    public override void _Ready()
    {
        GetNode<Button>("Play").GrabFocus();
    }

    private void _OnPlayPressed()
    {
        //AudioManager.PlayMusic(Music.Level1, false);
        Game.SwitchScene(this, Scene.Game);
    }

    private void _OnModsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        Game.SwitchScene(this, Scene.ModLoader);
    }

    private void _OnOptionsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        Game.SwitchScene(this, Scene.Options);
    }

    private void _OnCreditsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        Game.SwitchScene(this, Scene.Credits);
    }

    private async void _OnQuitPressed()
    {
        await GetNode<Global>(AutoloadPaths.Global).QuitAndCleanup();
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
