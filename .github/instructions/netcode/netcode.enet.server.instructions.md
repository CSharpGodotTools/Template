---
applyTo: "**/Netcode/ENet/Server/**/*.cs"
---

# ENet Server Stack

```
ENetLow
  └── ENetServer        (peer tracking, queues, dispatch)
        └── GodotServer (game API; abstract – subclass per game)
              └── GameServer (concrete, registers packets)
```

### ENetServer responsibilities
- `_peers Dictionary<uint, Peer>` — worker-thread-only peer lookup.
- `_incoming ConcurrentQueue<IncomingPacket>` — receives from `OnReceiveLow`.
- `_outgoing ConcurrentQueue<OutgoingMessage>` — drained each tick to send/broadcast.
- `_clientPacketHandlers Dictionary<Type, Action<PacketFromPeer<ClientPacket>>>` — registered via `OnPacket<T>`.
  - Handlers receive a `PacketFromPeer<T>` wrapper containing both the deserialized packet and the sender's peer ID.
  - Handlers **run on the ENet worker thread**.
- `_reassemblyBuffers Dictionary<uint, Dictionary<ushort, FragmentBuffer>>` — per-peer map used by the fragment reassembly logic.
- `ConcurrentQueues()` order each tick: ENet commands → incoming packets → outgoing packets → log aggregator flush.
- `OutgoingMessage` factories: `Unicast(data, peerId)`, `Broadcast(data)`, `BroadcastExcept(data, excludePeerId)`.

### GodotServer API (all thread-safe)
| Method | Description |
|--------|-------------|
| `Start(port, maxClients, options, ignoredPackets)` | Launches worker thread |
| `Stop()` | Enqueues stop command |
| `Send(ServerPacket, uint peerId)` | Serializes + enqueues unicast |
| `Send(ServerPacket, IEnumerable<uint> peerIds)` | Serializes once and enqueues a unicast per peer; use when you already track a set of IDs |
| `Broadcast(ServerPacket)` | Serializes + enqueues broadcast to all |
| `Broadcast(ServerPacket, uint excludeId)` | Serializes + enqueues exclusive broadcast |
| `Kick(uint id, DisconnectOpcode)` | Enqueues kick command |
| `Ban(uint id)` | Kick with `Banned` opcode |
| `KickAll / BanAll` | Batch variants |
| `IsRunning` | Interlocked flag |
| `ConnectedPeerCount` | Thread-safe connected peer count (interlocked) |

### Registering server-side packet handlers
```csharp
// In GameServer.RegisterPackets(); handlers run on ENet thread.
// The callback receives a PacketFromPeer<T> struct.
OnPacket<CPacketPlayerPosition>(peerPacket =>
{
    uint id = peerPacket.PeerId;
    CPacketPlayerPosition packet = peerPacket.Packet;
    // ...
});
```
