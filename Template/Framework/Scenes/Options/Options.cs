using Godot;
using System.Linq;

namespace Framework.Ui;

public partial class Options : PanelContainer
{
    // Constants
    private const int PopupStartPosition = -12;
    private const double PopupAnimationDuration = 0.17;

    // Fields
    private OptionsNav _optionsNav;
    private OptionsGeneral _optionsGeneral;
    private OptionsDisplay _optionsDisplay;
    private OptionsGraphics _optionsGraphics;
    private OptionsAudio _optionsAudio;
    private OptionsInput _optionsInput;
    private OptionsCustom _optionsCustom;
    private Node _navNode;

    // Godot Overrides
    public override void _Ready()
    {
        _navNode = GetNode("%Nav");
        _optionsNav = new OptionsNav(this, GetNode<Label>("%Title"));
        _optionsGeneral = new OptionsGeneral(this, _optionsNav.GeneralButton);
        _optionsDisplay = new OptionsDisplay(this, _optionsNav.DisplayButton);
        _optionsGraphics = new OptionsGraphics(this, _optionsNav.GraphicsButton);
        _optionsAudio = new OptionsAudio(this);
        _optionsInput = new OptionsInput(this, _optionsNav.InputButton);
        _optionsCustom = new OptionsCustom(_optionsNav);

        VisibilityChanged += OnVisibilityChanged;

        GameFramework.Scene.PostSceneChanged += OnPostSceneChanged;

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

        GameFramework.Scene.PostSceneChanged -= OnPostSceneChanged;
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
            GameFramework.FocusOutline.Focus(_navNode.GetNode<Button>(GameFramework.Options.GetCurrentTab()));
        }
    }

    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            _navNode.GetNode<Button>(GameFramework.Options.GetCurrentTab()).GrabFocus();
            _optionsDisplay.RefreshWindowSizeDisplay();
        }
    }
}
