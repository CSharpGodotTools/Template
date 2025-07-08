using Godot;

namespace __TEMPLATE__.UI;

public partial class MainMenuNav : Node
{
    public override void _Ready()
    {
        GetNode<Button>("Play").GrabFocus();
    }

    private static void _OnPlayPressed()
    {
        //AudioManager.PlayMusic(Music.Level1, false);
        Game.SwitchScene(Scene.Game);
    }

    private static void _OnModsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        Game.SwitchScene(Scene.ModLoader);
    }

    private static void _OnOptionsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        Game.SwitchScene(Scene.Options);
    }

    private static void _OnCreditsPressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        Game.SwitchScene(Scene.Credits);
    }

#pragma warning disable CA1822 // Mark members as static
    private async void _OnQuitPressed()
#pragma warning restore CA1822 // Mark members as static
    {
        await Global.QuitAndCleanup();
    }

    private static void _OnDiscordPressed()
    {
        OS.ShellOpen("https://discord.gg/j8HQZZ76r8");
    }

    private static void _OnGitHubPressed()
    {
        OS.ShellOpen("https://github.com/ValksGodotTools/Template");
    }
}

