using Godot;

namespace __TEMPLATE__.TopDown2D;

public class PlayerMoveManager(PlayerConfig config)
{
    public void HandleMovement(Player player, out Vector2 moveDirection)
    {
        moveDirection = GetMoveDirection();

        player.Velocity += moveDirection * config.Speed;
        player.Velocity = player.Velocity.Lerp(Vector2.Zero, config.Friction);
    }

    private static Vector2 GetMoveDirection()
    {
        Vector2 moveDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        moveDirection += new Vector2(Input.GetJoyAxis(0, JoyAxis.LeftX), Input.GetJoyAxis(0, JoyAxis.LeftY));
        return moveDirection.Normalized();
    }
}
