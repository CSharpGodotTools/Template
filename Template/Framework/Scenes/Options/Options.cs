using Godot;
using System;
using System.Linq;

namespace __TEMPLATE__.Ui;

public partial class Options : PanelContainer, ISceneDependencyReceiver
{
    // Constants
    private const int PopupStartPosition = -12;
    private const double PopupAnimationDuration = 0.17;

    // Fields
    private OptionsNav _optionsNav = null!;
    private OptionsGeneral _optionsGeneral = null!;
    private OptionsDisplay _optionsDisplay = null!;
    private OptionsGraphics _optionsGraphics = null!;
    private OptionsAudio _optionsAudio = null!;
    private OptionsInput _optionsInput = null!;
    private OptionsCustom _optionsCustom = null!;
    private Node _navNode = null!;
    private bool _isConfigured;
    private OptionsManager _optionsManager = null!;
    private SceneManager _sceneManager = null!;
    private FocusOutlineManager _focusOutlineManager = null!;
    private AudioManager _audioManager = null!;

    public void Configure(GameServices services)
    {
        Configure(services.OptionsManager, services.SceneManager, services.FocusOutline, services.AudioManager);
    }

    public void Configure(OptionsManager optionsManager, SceneManager sceneManager, FocusOutlineManager focusOutlineManager, AudioManager audioManager)
    {
        _optionsManager = optionsManager;
        _sceneManager = sceneManager;
        _focusOutlineManager = focusOutlineManager;
        _audioManager = audioManager;
        _isConfigured = true;
    }

    // Godot Overrides
    public override void _Ready()
    {
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(Options)} was not configured before _Ready.");

        _navNode = GetNode("%Nav");
        _optionsNav = new OptionsNav(this, GetNode<Label>("%Title"), _optionsManager);
        _optionsGeneral = new OptionsGeneral(this, _optionsNav.GeneralButton, _optionsManager);
        _optionsDisplay = new OptionsDisplay(this, _optionsNav.DisplayButton, _optionsManager);
        _optionsGraphics = new OptionsGraphics(this, _optionsNav.GraphicsButton, _optionsManager);
        _optionsAudio = new OptionsAudio(this, _optionsManager, _audioManager);
        _optionsInput = new OptionsInput(this, _optionsNav.InputButton, _optionsManager, _sceneManager, _focusOutlineManager);
        _optionsCustom = new OptionsCustom(_optionsNav, _optionsManager);

        VisibilityChanged += OnVisibilityChanged;

        _sceneManager.PostSceneChanged += OnPostSceneChanged;

        SetupPopupAnimations();
    }

    public override void _Input(InputEvent @event)
    {
        _optionsInput.HandleInput(@event);
    }

    public override void _Process(double delta)
    {
        _optionsDisplay.UpdatePopupIfOpen();
    }

    public override void _ExitTree()
    {
        _optionsNav.Dispose();
        _optionsGeneral.Dispose();
        _optionsDisplay.Dispose();
        _optionsGraphics.Dispose();
        _optionsAudio.Dispose();
        _optionsInput.Dispose();
        _optionsCustom.Dispose();

        _sceneManager.PostSceneChanged -= OnPostSceneChanged;
        VisibilityChanged -= OnVisibilityChanged;
    }

    // Subscribers
    private void SetupPopupAnimations()
    {
        SceneTree tree = GetTree();

        foreach (OptionButton button in FindChildren("*", "OptionButton", true, false)
                                        .Cast<OptionButton>())
        {
            Godot.PopupMenu popup = button.GetPopup();
            popup.AboutToPopup += async () =>
            {
                await popup.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

                if (!IsInstanceValid(popup) || !popup.Visible)
                {
                    return;
                }

                AnimatePopupIn(popup);
            };
        }
    }

    private static void AnimatePopupIn(Godot.PopupMenu popup)
    {
        Vector2I targetPosition = popup.Position;
        popup.Position = new Vector2I(targetPosition.X, targetPosition.Y + PopupStartPosition);

        Tween tween = popup.CreateTween();
        tween.TweenProperty(popup, "position:y", (double)targetPosition.Y, PopupAnimationDuration)
             .SetEase(Tween.EaseType.Out)
             .SetTrans(Tween.TransitionType.Quad);
    }

    private void OnPostSceneChanged()
    {
        if (Visible)
        {
            _focusOutlineManager.Focus(_navNode.GetNode<Button>(_optionsManager.GetCurrentTab()));
        }
    }

    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            _navNode.GetNode<Button>(_optionsManager.GetCurrentTab()).GrabFocus();
            _optionsDisplay.RefreshWindowSizeDisplay();
        }
    }
}
