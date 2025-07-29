using Godot;

namespace __TEMPLATE__.TopDown2D;

public class PlayerLookManager
{
    private double _controllerLookInputsActiveBuffer;
    private Vector2 _targetLookDirection;
    private Vector2 _currentLookDirection;

    public void HandleLookDirection(Node2D cursor, Node2D node, PlayerConfig config, double delta)
    {
        _targetLookDirection = GetLookDirection(node);
        _currentLookDirection = _currentLookDirection.Slerp(_targetLookDirection, (float)(config.LookLerpSpeed * delta));
        cursor.LookAt(node.Position + _currentLookDirection.Normalized() * 100);
    }

    public void UpdateControllerLookInputs(double delta)
    {
        float rightX = Input.GetJoyAxis(0, JoyAxis.RightX);
        float rightY = Input.GetJoyAxis(0, JoyAxis.RightY);
        float triggerLeft = Input.GetJoyName(0) == "PS3 Controller" ? -(Input.GetJoyAxis(0, JoyAxis.TriggerLeft) - 0.5f) * 2 : 0;

        float sum = Mathf.Abs(rightX) + Mathf.Abs(rightY) + Mathf.Abs(triggerLeft);

        if (sum > 0.3)
        {
            _controllerLookInputsActiveBuffer = 1;
        }

        _controllerLookInputsActiveBuffer -= delta;
    }

    private Vector2 GetLookDirection(Node2D node)
    {
        if (_controllerLookInputsActiveBuffer > 0)
        {
            return GetControllerLookDirection();
        }
        else
        {
            return GetMouseLookDirection(node);
        }
    }

    private static Vector2 GetControllerLookDirection()
    {
        return Input.GetJoyName(0) switch
        {
            "PS3 Controller" => new Vector2(Input.GetJoyAxis(0, JoyAxis.RightX), -(Input.GetJoyAxis(0, JoyAxis.TriggerLeft) - 0.5f) * 2),
            _ => new Vector2(Input.GetJoyAxis(0, JoyAxis.RightX), Input.GetJoyAxis(0, JoyAxis.RightY))
        };
    }

    private static Vector2 GetMouseLookDirection(Node2D node)
    {
        return (node.GetGlobalMousePosition() - node.Position).Normalized();
    }
}
