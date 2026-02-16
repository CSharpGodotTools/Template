<div align="center">
    <h1>PacketGen</h1>
</div>

PacketGen generates packet boilerplate at compile time so packet types in [Template](https://github.com/CSharpGodotTools/Template) stay short and explicit.

## What it generates
- `Write(PacketWriter writer)`
- `Read(PacketReader reader)`
- `Equals(object obj)`
- `GetHashCode()`
- Optional `PacketRegistry` mapping when `[PacketRegistry]` is present

## Supported packet properties
- Primitives and common netcode types (`bool`, `int`, `char`, `string`, `byte[]`, `Godot.Vector2`, `Godot.Vector3` and so on)
- Recursive `List<T>`, `Dictionary<TKey, TValue>` and array types
- Structs and classes (including nested class/struct members)

## Packet requirements
- Packet type must be `partial class` and inherit from `ClientPacket` or `ServerPacket`
- Generation is skipped when packet already implements `Write`/`Read`
- `[NetExclude]` skips individual properties

## Example: packet input
```cs
public struct PlayerStats
{
    public int Level { get; set; }
    public Vector2 Spawn { get; set; }
}

public class PlayerProfile
{
    public string Name { get; set; }
    public PlayerStats Stats { get; set; }
}

public partial class CPacketPlayerInfo : ClientPacket
{
    public uint Id { get; set; }
    public PlayerProfile Profile { get; set; }
}
```

## Example: generated members (shape)
```cs
public partial class CPacketPlayerInfo
{
    public override void Write(PacketWriter writer) { /* generated */ }
    public override void Read(PacketReader reader) { /* generated */ }
    public override bool Equals(object obj) { /* generated */ }
    public override int GetHashCode() { /* generated */ }
}
```

## Packet registry generation
```cs
[PacketRegistry(typeof(ushort))]
public partial class PacketRegistry
{
}
```

Generates deterministic opcode maps for client and server packets:
- `Dictionary<Type, PacketInfo<ClientPacket>> ClientPacketInfo`
- `Dictionary<ushort, Type> ClientPacketTypes`
- `Dictionary<Type, PacketInfo<ServerPacket>> ServerPacketInfo`
- `Dictionary<ushort, Type> ServerPacketTypes`

If no opcode type is provided, `byte` is used.

## Installing as Local NuGet Package
Copy the `.nupkg` from `bin\Debug` to your main project.

Add a file named `NuGet.config` to the main project's root folder with the following contents:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="LocalNugets" value="Framework/Libraries" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

The name for key `LocalNugets` can be changed to anything you like. Update `Framework/Libraries` to wherever your `.nupkg` is stored.

Add this to your main project `.csproj`:

```xml
<PackageReference Include="PacketGen" Version="*" />
```

Replace `*` with an explicit version if preferred.

## Contributing
Generated test outputs: `PacketGen.Tests\bin\Debug\net10.0\_Generated`

Tests: `PacketGen.Tests\Tests`

Generators: `PacketGen\Generators`
