using Godot;
using System;
using System.Linq;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Options scene root that wires tab navigation, input, and custom option UI.
/// </summary>
public partial class Options : PanelContainer, ISceneDependencyReceiver
{
    // Constants
    private const int PopupStartPosition = -12;
    private const double PopupAnimationDuration = 0.17;

    // Fields
    private OptionsNav _optionsNav = null!;
    private OptionsInput _optionsInput = null!;
    private OptionsCustom _optionsCustom = null!;
    private bool _isConfigured;
    private OptionsManager _optionsManager = null!;
    private SceneManager _sceneManager = null!;
    private FocusOutlineManager _focusOutlineManager = null!;

    /// <summary>
    /// Injects services required by the options scene.
    /// </summary>
    /// <param name="services">Resolved game service bundle.</param>
    public void Configure(GameServices services)
    {
        _optionsManager = services.OptionsManager;
        _sceneManager = services.SceneManager;
        _focusOutlineManager = services.FocusOutline;
        _isConfigured = true;
    }

    public override void _EnterTree()
    {
        SceneComposition.ConfigureNodeFromGame(this);
    }

    // Godot Overrides
    public override void _Ready()
    {
        // Fail fast when dependencies were not configured before initialization.
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(Options)} was not configured before _Ready.");

        _optionsNav = new OptionsNav(this, GetNode<Label>("%Title"), _optionsManager);

        ClearLegacyTabRows();

        // Input tab button must exist for input-controller initialization.
        if (!_optionsNav.TryGetTabButton(FrameworkOptionsTabs.Input, out Button inputNavButton))
            throw new InvalidOperationException($"Input tab button '{FrameworkOptionsTabs.Input}' was not found.");

        _optionsInput = new OptionsInput(this, inputNavButton, _optionsManager, _sceneManager, _focusOutlineManager);
        _optionsCustom = new OptionsCustom(_optionsNav, _optionsManager);

        // Register after custom bindings are subscribed so rows are built from events reliably.
        RegisterTabOptions();

        _optionsNav.RefreshOptionalTabs(FrameworkOptionsTabs.Input);

        VisibilityChanged += OnVisibilityChanged;

        _sceneManager.PostSceneChanged += OnPostSceneChanged;

        SetupPopupAnimations();
    }

    public override void _Input(InputEvent @event)
    {
        _optionsInput.HandleInput(@event);
    }

    public override void _ExitTree()
    {
        _optionsNav.Dispose();
        _optionsInput.Dispose();
        _optionsCustom.Dispose();

        _sceneManager.PostSceneChanged -= OnPostSceneChanged;
        VisibilityChanged -= OnVisibilityChanged;
    }

    // Subscribers
    /// <summary>
    /// Removes pre-authored non-input rows so dynamic registration can rebuild them.
    /// </summary>
    private void ClearLegacyTabRows()
    {
        foreach (string tabName in _optionsNav.GetTabNames())
        {
            // Keep input tab rows authored separately by input-specific UI.
            if (string.Equals(tabName, FrameworkOptionsTabs.Input, StringComparison.OrdinalIgnoreCase))
                continue;

            // Skip tabs that no longer resolve to a container.
            if (!_optionsNav.TryGetTabContainer(tabName, out VBoxContainer tabContainer))
                continue;

            for (int childIndex = tabContainer.GetChildCount() - 1; childIndex >= 0; childIndex--)
            {
                Node child = tabContainer.GetChild(childIndex);
                tabContainer.RemoveChild(child);
                child.QueueFree();
            }
        }
    }

    /// <summary>
    /// Registers all default tab option definitions.
    /// </summary>
    private void RegisterTabOptions()
    {
        foreach (IOptionsTabRegistrar registrar in DefaultOptionsTabRegistrars.Create())
            registrar.Register(_optionsManager);
    }

    /// <summary>
    /// Hooks animated popup behavior for option dropdown menus.
    /// </summary>
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

                // Skip animation when popup was closed or freed before the next frame.
                if (!IsInstanceValid(popup) || !popup.Visible)
                {
                    return;
                }

                AnimatePopupIn(popup);
            };
        }
    }

    /// <summary>
    /// Animates popup menu entrance from an offset Y position.
    /// </summary>
    /// <param name="popup">Popup menu to animate.</param>
    private static void AnimatePopupIn(Godot.PopupMenu popup)
    {
        Vector2I targetPosition = popup.Position;
        popup.Position = new Vector2I(targetPosition.X, targetPosition.Y + PopupStartPosition);

        Tween tween = popup.CreateTween();
        tween.TweenProperty(popup, "position:y", (double)targetPosition.Y, PopupAnimationDuration)
             .SetEase(Tween.EaseType.Out)
             .SetTrans(Tween.TransitionType.Quad);
    }

    /// <summary>
    /// Refocuses current tab button after scene changes while visible.
    /// </summary>
    private void OnPostSceneChanged()
    {
        // Skip focus updates while options panel is hidden.
        if (!Visible)
            return;

        // Restore focus to the currently selected tab button when available.
        if (_optionsNav.TryGetTabButton(_optionsManager.GetCurrentTab(), out Button tabButton))
            _focusOutlineManager.Focus(tabButton);
    }

    /// <summary>
    /// Restores keyboard focus to current tab button when panel becomes visible.
    /// </summary>
    private void OnVisibilityChanged()
    {
        // Skip focus updates while options panel is hidden.
        if (!Visible)
            return;

        // Focus currently selected tab button when available.
        if (_optionsNav.TryGetTabButton(_optionsManager.GetCurrentTab(), out Button tabButton))
            tabButton.GrabFocus();
    }
}
