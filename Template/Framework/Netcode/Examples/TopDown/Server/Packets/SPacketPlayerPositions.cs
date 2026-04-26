using Godot;
using System.Collections.Generic;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// Server packet carrying a snapshot of player positions keyed by player id.
/// </summary>
public class SPacketPlayerPositions : ServerPacket
{
    /// <summary>
    /// Gets or sets the latest position map keyed by player id.
    /// </summary>
    public Dictionary<uint, Vector2> Positions { get; set; } = null!;

    /// <summary>
    /// Writes the position snapshot to packet stream.
    /// </summary>
    /// <param name="writer">Packet writer.</param>
    public override void Write(PacketWriter writer)
    {
        writer.Write(Positions.Count);

        foreach (KeyValuePair<uint, Vector2> kv in Positions)
        {
            writer.Write(kv.Key);
            writer.Write(kv.Value);
        }
    }

    /// <summary>
    /// Reads position snapshot data from packet stream.
    /// </summary>
    /// <param name="reader">Packet reader.</param>
    public override void Read(PacketReader reader)
    {
        int count = reader.ReadInt();

        // Reuse the dictionary across reads on the shared singleton instance to
        // avoid allocating a new collection every broadcast tick.
        if (Positions == null)
            Positions = new Dictionary<uint, Vector2>(count);
        else
            Positions.Clear();

        for (int i = 0; i < count; i++)
            Positions[reader.ReadUInt()] = reader.ReadVector2();
    }
}
