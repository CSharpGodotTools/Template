// ReSharper disable RedundantCast (explicitly cast to help prevent human error when writing incorrect type)
using Godot;
using __TEMPLATE__.Netcode;
using __TEMPLATE__.Netcode.Client;

namespace __TEMPLATE__.TopDown2D;

/// <summary>
/// A other player has joined or left the server.
/// </summary>
public class SPacketPlayerJoinLeave : ServerPacket
{
    public uint Id { get; set; }
    public string Username { get; set; }
    public Vector2 Position { get; set; }
    public bool Joined { get; set; }

    public override void Write(PacketWriter writer)
    {
        writer.Write((uint)Id);
        writer.Write((bool)Joined);

        if (Joined)
        {
            writer.Write((string)Username);
            writer.Write((Vector2)Position);
        }
    }

    public override void Read(PacketReader reader)
    {
        Id = reader.ReadUInt();

        Joined = reader.ReadBool();

        if (Joined)
        {
            Username = reader.ReadString();
            Position = reader.ReadVector2();
        }
    }

    public override void Handle(ENetClient client)
    {
        Level level = Services.Get<Level>();

        if (Joined)
        {
            level.AddOtherPlayer(Id, new PlayerData
            {
                Username = Username,
                Position = Position
            });
        }
        else
        {
            level.RemoveOtherPlayer(Id);
        }
    }
}

