using Godot;
using System.Collections.Generic;

namespace Framework.Netcode.Examples.Topdown;

public partial class SPacketPlayerPositions : ServerPacket
{
    public Dictionary<uint, Vector2> Positions { get; set; }

    public override void Write(PacketWriter writer)
    {
        writer.Write(Positions.Count);

        foreach (KeyValuePair<uint, Vector2> kv in Positions)
        {
            writer.Write(kv.Key);
            writer.Write(kv.Value);
        }
    }

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
