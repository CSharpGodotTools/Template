using Godot;

namespace Template.Setup.Testing;

public static class PacketStructTypesFactory
{
    public static CPacketStructTypes CreateSample()
    {
        MovementSnapshot spawn = new()
        {
            Tick = 40,
            Position = new Vector2(12f, -3.5f),
            Velocity = new Vector2(0.9f, 0.35f)
        };

        MovementSnapshot current = new()
        {
            Tick = 41,
            Position = new Vector2(13.5f, -3.4f),
            Velocity = new Vector2(1.2f, 0.3f)
        };

        MovementSnapshot target = new()
        {
            Tick = 42,
            Position = new Vector2(15.5f, -3.25f),
            Velocity = new Vector2(1.5f, 0.25f)
        };

        return new CPacketStructTypes
        {
            Spawn = spawn,
            Current = current,
            Target = target
        };
    }
}
