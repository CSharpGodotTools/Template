using Godot;
using GodotUtils;

namespace __TEMPLATE__.UI;

public partial class MainMenuNav : Node
{
    #region Exports
    [Export] private PackedScene _gameScene;
    #endregion

    #region Fields
    private SceneManager _scene;
    private Viewport _viewport;
    private Button _playBtn;
    private bool _focusWasNeverChanged = true;
    #endregion

    #region Godot Overrides
    public override void _Ready()
    {
        _scene = Game.Scene;
        _viewport = GetViewport();
        _playBtn = GetNode<Button>("Play");

        FocusOnPlayBtn();

        Game.Scene.PostSceneChanged += OnPostSceneChanged;

        _viewport.GuiFocusChanged += OnGuiFocusChanged;
    }

    private void OnGuiFocusChanged(Control node)
    {
        _focusWasNeverChanged = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            // Solve the issue of pressing up key not focusing on play button if focus was never changed before
            if (keyEvent.IsJustPressed(Key.Up) && _focusWasNeverChanged)
            {
                FocusOnPlayBtn();
            }
        }
    }

    public override void _ExitTree()
    {
        _viewport.GuiFocusChanged -= OnGuiFocusChanged;
        Game.Scene.PostSceneChanged -= OnPostSceneChanged;
    }
    #endregion

    private void FocusOnPlayBtn()
    {
        Game.FocusOutline.Focus(_playBtn);
    }

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
    #endregion
}
