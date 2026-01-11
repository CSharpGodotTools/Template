using Godot;

namespace __TEMPLATE__.UI;

public partial class MainMenuNav : Node
{
    #region Exports
    [Export] private PackedScene _gameScene;
    #endregion

    #region Fields
    private SceneManager _scene;
    #endregion

    #region Godot Overrides
    public override void _Ready()
    {
        _scene = Game.Scene;
        FocusOnPlayBtn();
        Game.Scene.PostSceneChanged += OnPostSceneChanged;
    }

    public override void _ExitTree()
    {
        Game.Scene.PostSceneChanged -= OnPostSceneChanged;
    }
    #endregion

    #region Subscribers
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

    private void OnPostSceneChanged()
    {
        FocusOnPlayBtn();
    }

    private void FocusOnPlayBtn()
    {
        Game.FocusOutline.Focus(GetNode<Button>("Play"));
    }
    #endregion
}
