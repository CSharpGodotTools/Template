using Godot;

namespace Template.Setup.Testing;

/// <summary>
/// Builds struct-based packet fixtures used by ENet packet tests.
/// </summary>
public static class PacketStructTypesFactory
{
    /// <summary>
    /// Creates a packet with sequential movement snapshots.
    /// </summary>
    /// <returns>Packet populated with spawn, current, and target snapshots.</returns>
    public static CPacketStructTypes CreateSample()
    {
        // Keep ticks ordered to mirror a realistic movement timeline.
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
