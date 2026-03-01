Packets only contain the data to be sent and need to extend from `ClientPacket` or `ServerPacket`. All packets are prefixed with `C` for client or `S` for server, this is not required.
```cs
public partial class CPacketPlayerPosition : ClientPacket
{
    public uint Id { get; set; }
    public Vector2 Position { get; set; }

    // Optionally exclude properties so they do not get sent
    [NetExclude]
    public Vector2 PrevPosition { get; set; }
}
```

The following is what is outputted by the source gen for you:
```cs
// You do not need to write this code, it is generated for you!
public partial class CPacketPlayerPosition
{
    public override void Write(PacketWriter writer)
    {
        writer.Write(Id);
        writer.Write(Position);
    }

    public override void Read(PacketReader reader)
    {
        Id = reader.ReadUInt();
        Position = reader.ReadVector2();
    }
}
```

If a type is not supported, you will need to manually override `Write` and `Read`. You should also get a warning in the IDE telling you a type is not supported. The warning will go away when you override `Write` or `Read`.

| Type              | Supported | Example Types                       | Additional Notes |
| ----------------- | --------- | ----------------------------------- | -------------------------------------------------------- |
| Primitives        | ✅        | `int`, `bool`, `ulong`              |                                                          |
| Vectors & byte[]  | ✅        | `Vector2`, `Vector3`, `byte[]`      |                                                          |
| Generics          | ✅        | `List<List<int>>`, `Dictionary<string, List<char>>`      |                                     |
| Arrays            | ✅        | `int[]`, `bool[]`                             |                                                |
| Classes & Structs | ✅        | `PlayerData`                        |                                                          |
| Msc               | ❌        | `HashSet`, `PointLight2D`           | These types are too specific and will not be supported.  |

You have full control over the order of which data gets sent when you override `Write` and `Read`.

```cs
// Since we need to use if conditions we actually have to type out the Write and Read functions
public partial class SPacketPlayerJoinLeave : ServerPacket
{
    public uint Id { get; set; }
    public string Username { get; set; }
    public Vector2 Position { get; set; }
    public bool Joined { get; set; }

    public override void Write(PacketWriter writer)
    {
        // Not required to cast explicit types but helps prevent human error
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
}
```

By default you can only have up to `256` different client packet classes and `256` different server packets for a total of `512` different packet classes. 

If you need more space, you have 2 options.

1. (Recommended) Add a `OpcodeEnum Opcode { get; set; }` property to any of your packet classes with a conditional if-else chain kind of like what you see above but instead of a bool it is a enum. Replace `OpcodeEnum` with your own enum. (All enums are converted to bytes so you cannot have more than `256` options in any enum you send over the network! _This may be changed in the future._)  
2. In `PacketRegistry.cs`, change `byte` to `ushort` but note that now all your packets will have a 2 byte overhead instead of just 1 byte. Note that `ushort` has a size of `65,535` compared to `byte` that only has `255`.

```cs
// res://Framework/Netcode/Packet/PacketRegistry.cs
[PacketRegistry(typeof(byte))]
public partial class PacketRegistry
{
}
```  

