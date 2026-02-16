using Godot;
using GodotUtils;

namespace Framework.UI;

public partial class MainMenuNav : Node
{
    // Exports
    [Export] private PackedScene _gameScene;

    // Fields
    private SceneManager _scene;
    private Viewport _viewport;
    private Button _playBtn;
    private bool _focusWasNeverChanged = true;

    // Godot Overrides
    public override void _Ready()
    {
        _scene = GameFramework.Scene;
        _viewport = GetViewport();
        _playBtn = GetNode<Button>("Play");

        FocusOnPlayBtn();

        GameFramework.Scene.PostSceneChanged += OnPostSceneChanged;

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
        GameFramework.Scene.PostSceneChanged -= OnPostSceneChanged;
    }

    // FocusOnPlayBtn
    private void FocusOnPlayBtn()
    {
        GameFramework.FocusOutline.Focus(_playBtn);
    }

    // Subscribers
    private void OnPlayPressed()
    {
        GD.Print($"Remove this message and set the game scene (it is currently {(_gameScene == null ? "not set" : "set")})");
        //_scene.SwitchTo(_gameScene);
    }

    private void OnModsPressed()
    {
        _scene.SwitchToModLoader();
    }

    private void OnOptionsPressed()
    {
        _scene.SwitchToOptions();
    }

    private void OnCreditsPressed()
    {
        _scene.SwitchToCredits();
    }

    private async static void OnQuitPressed()
    {
        await Autoloads.Instance.ExitGame();
    }

    private static void OnDiscordPressed()
    {
        OS.ShellOpen("https://discord.gg/j8HQZZ76r8");
    }

    private static void OnGitHubPressed()
    {
        OS.ShellOpen("https://github.com/ValksGodotTools/Template");
    }

    private void OnPostSceneChanged()
    {
        FocusOnPlayBtn();
    }
}
