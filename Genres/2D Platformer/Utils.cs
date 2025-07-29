using Godot;

namespace __TEMPLATE__.Platformer2D.Retro;

public static class Utils
{
    public static float ClampAndDampen(float horzVelocity, float dampening, float maxSpeedGround)
    {
        return Mathf.Abs(horzVelocity) <= dampening ? 
            0 : horzVelocity > 0 ?
                Mathf.Min(horzVelocity - dampening, maxSpeedGround) :
                Mathf.Max(horzVelocity + dampening, -maxSpeedGround);
    }
}
