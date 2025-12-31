using Godot;

namespace __TEMPLATE__.UI;

public partial class MainMenuNav : Node
{
    [Export] private PackedScene _gameScene;

    private SceneManager _scene;

    public override void _Ready()
    {
        _scene = Game.Scene;
        Game.FocusOutline.IgnoreNextFocus();
        GetNode<Button>("Play").GrabFocus();
    }

    private void _OnPlayPressed()
    {
        _scene.SwitchTo(_gameScene);
    }

    private void _OnModsPressed()
    {
        _scene.SwitchToModLoader();
    }

    private void _OnOptionsPressed()
    {
        _scene.SwitchToOptions();
    }

    private void _OnCreditsPressed()
    {
        _scene.SwitchToCredits();
    }

    private async void _OnQuitPressed()
    {
        await BaseAutoloads.Instance.ExitGame();
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
