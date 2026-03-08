---
applyTo: "**/Netcode/Packet/**/*.cs"
---

# Packet System & PacketGen

### Packet class hierarchy
```
GamePacket  (abstract)
  ├── ClientPacket  (abstract) — travels client → server
  └── ServerPacket  (abstract) — travels server → client
```

`PacketFromPeer<T>` is a lightweight struct used by servers to carry a
`ClientPacket` together with the sender's peer ID; it's the type delivered to
`ENetServer.OnPacket<T>` handlers.

### Wire format
```
[opcode: 2 bytes (ushort LE)][payload: variable]
```
Max packet size: **8192 bytes**. Packets exceeding this are automatically fragmented by
`PacketFragmenter` (see ENet common instructions) and reassembled at the receiver before dispatch.

> **Opcode size**: The wire opcode is always `ushort` (2 bytes) regardless of the registry
> backing type. The backing type only controls which values are assigned to user packets:
> `byte` → opcodes 0–254 (255 reserved); `ushort` → opcodes 0–65534 (65535 reserved).
> The last value of each type is `PacketRegistry.FragmentOpcode` and is never assigned
> to a user-defined packet.

### Serialization
- `GamePacket.Write()` — calls `Write(PacketWriter)` (generated) and caches `byte[]`.
- `GamePacket.Read(PacketReader)` — calls `Read(PacketReader)` (generated).
- `Write(PacketWriter)` and `Read(PacketReader)` are generated as partial methods by **PacketGen**.
- Manual override is possible for packets with complex layouts (see `SPacketPlayerPositions`).
- `PacketWriter` wraps `BinaryWriter` over a `MemoryStream`.
- `PacketReader` copies ENet packet bytes into a managed buffer then wraps them in `BinaryReader`.

### PacketGen (Template.PacketGen — Roslyn IIncrementalGenerator)
- Triggered by `[PacketRegistry]` on the `PacketRegistry` partial class.
- Discovers all `ClientPacket` and `ServerPacket` subclasses in the compilation.
- Assigns opcodes **deterministically** (sorted by fully-qualified type name, ascending).
- Generates `PacketRegistry.g.cs` with `ClientPacketInfo` and `ServerPacketInfo` dictionaries.
- Generates `Write(PacketWriter)` / `Read(PacketReader)` partial methods for each packet type.
- Opcode backing type defaults to `byte`; customize with `[PacketRegistry(typeof(ushort))]`.
- `[NetExclude]` on a property/field skips it in generated serialization.
- The generated `PacketRegistry` class also exposes:
  - `ClientPacketTypesWire` / `ServerPacketTypesWire` — `Dictionary<ushort, Type>` used by
    the ENet receive path for wire-format opcode lookups (always `ushort` regardless of backing type).
  - `FragmentOpcode` — `const ushort` equal to the max value of the configured backing type;
    reserved and never assigned to any user packet.

**Opcode stability**: Adding, removing, or renaming packet types changes all opcode assignments.
Client and server must always be built from the same source.

### PacketRegistry usage
```csharp
// Opcode lookup (used internally by GamePacket.GetOpcode()):
PacketRegistry.ClientPacketInfo[typeof(CPacketPlayerPosition)].Opcode
PacketRegistry.ServerPacketInfo[typeof(SPacketPlayerPositions)].Opcode
```
