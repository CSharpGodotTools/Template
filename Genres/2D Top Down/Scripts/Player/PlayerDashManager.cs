using Godot;
using GodotUtils;

namespace __TEMPLATE__.TopDown2D;

public class PlayerDashManager(PlayerConfig config, AnimatedSprite2D dashSprite)
{
    private bool _canDash;

    public void HandleDash(Node2D node, Vector2 moveDirection)
    {
        bool dashJustPressed = Input.IsActionJustPressed(InputActions.Dash) || Input.IsJoyButtonPressed(0, JoyButton.LeftShoulder);

        if (dashJustPressed && _canDash && moveDirection != Vector2.Zero)
        {
            PerformDash(node, moveDirection);
            ResetDashStateAfterDelay(node);
        }
    }

    public void ResetDashState()
    {
        _canDash = true;
    }

    private void PerformDash(Node2D node, Vector2 moveDirection)
    {
        Dash(node, moveDirection);
        _canDash = false;
    }

    private void ResetDashStateAfterDelay(Node node)
    {
        GTween.Delay(node, 0.2, () => _canDash = true);
    }

    private void Dash(Node2D node, Vector2 moveDirection)
    {
        GTween ghosts = new GTween(node)
            .Delay(0.0565)
            .Callback(() => AddDashGhost(node))
            .Loop();

        new GTween(node)
            .Animate(CharacterBody2D.PropertyName.Velocity, moveDirection * config.DashStrength, 0.1)
            .Delay(0.1)
            .Callback(() => ghosts.Stop());
    }

    private void AddDashGhost(Node2D node)
    {
        PlayerDashGhost ghost = PlayerDashGhost.Instantiate(node.Position, dashSprite);

        node.AddChildToCurrentScene(ghost);
    }
}
