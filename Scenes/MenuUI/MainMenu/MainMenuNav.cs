using Godot;

namespace Template.UI;

public partial class MainMenuNav : Node
{
    public override void _Ready()
    {
        GetNode<Button>("Play").GrabFocus();
    }

    private static void _on_play_pressed()
    {
        //AudioManager.PlayMusic(Music.Level1, false);
        Game.SwitchScene(Scene.Game);
    }

    private static void _on_mods_pressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        Game.SwitchScene(Scene.ModLoader);
    }

    private static void _on_options_pressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        Game.SwitchScene(Scene.Options);
    }

    private static void _on_credits_pressed()
    {
        //AudioManager.PlayMusic(Music.Level4);
        Game.SwitchScene(Scene.Credits);
    }

    private async void _on_quit_pressed()
    {
        await Global.QuitAndCleanup();
    }

    private static void _on_discord_pressed()
    {
        OS.ShellOpen("https://discord.gg/j8HQZZ76r8");
    }

    private static void _on_github_pressed()
    {
        OS.ShellOpen("https://github.com/ValksGodotTools/Template");
    }
}

