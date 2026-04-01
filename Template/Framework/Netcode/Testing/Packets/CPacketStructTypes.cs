using __TEMPLATE__.Netcode;
using Godot;

namespace Template.Setup.Testing;

/// <summary>
/// Represents a single movement state sample.
/// </summary>
public struct MovementSnapshot
{
    /// <summary>
    /// Gets or sets simulation tick for this snapshot.
    /// </summary>
    public int Tick { get; set; }

    /// <summary>
    /// Gets or sets world position at this tick.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Gets or sets world velocity at this tick.
    /// </summary>
    public Vector2 Velocity { get; set; }
}

/// <summary>
/// Packet carrying movement snapshots for struct serialization tests.
/// </summary>
public partial class CPacketStructTypes : ClientPacket
{
    /// <summary>
    /// Gets or sets spawn-time movement snapshot.
    /// </summary>
    public MovementSnapshot Spawn { get; set; }

    /// <summary>
    /// Gets or sets current movement snapshot.
    /// </summary>
    public MovementSnapshot Current { get; set; }

    /// <summary>
    /// Gets or sets target movement snapshot.
    /// </summary>
    public MovementSnapshot Target { get; set; }
}
