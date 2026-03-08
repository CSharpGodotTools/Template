---
applyTo: "**/Netcode/ENet/Client/**/*.cs"
---

# ENet Client Stack

```
ENetLow
  └── ENetClient        (connection lifecycle, queues, dispatch)
        └── GodotClient (game API; abstract – subclass per game)
              └── GameClient (concrete, registers packets)
```

### ENetClient responsibilities
- `_peer Peer` — the single ENet peer representing the server connection.
- Configurable timeout/ping (overridable virtual properties: `PingIntervalMs`, `PeerTimeoutMs`, `PeerTimeoutMinimumMs`, `PeerTimeoutMaximumMs`). Defaults are 1000 ms ping, 5000 ms timeout for all three timeout values.
- On ENet thread: `OnConnected()`, `OnDisconnected()`, `OnTimedOut()` virtual hooks for subclasses.
- `MainThreadCommands ConcurrentQueue<Cmd<GodotOpcode>>` — lifecycle signals pushed to Godot thread.
- `MainThreadPackets ConcurrentQueue<PacketData>` — received packets pushed to Godot thread.
- `_connected` Interlocked long flag — readable from any thread via `IsConnected`.
- `_reassemblyBuffers` map of stream‑id → buffer; cleared automatically when the
  client disconnects to avoid stale fragments.

### GodotClient API
| Method | Thread-safe | Description |
|--------|-------------|-------------|
| `Connect(ip, port, options, ignoredPackets)` | Yes | Starts worker thread |
| `Stop()` | Yes | Requests graceful disconnect |
| `Send(ClientPacket)` | Yes | Serializes + enqueues outgoing |
| `HandlePackets()` | **Main thread only** | Pumps `MainThreadPackets` and `MainThreadCommands` |
| `IsConnected` | Yes | Interlocked flag |
| `IsRunning` | Yes | Interlocked flag (inherited) |
| `PeerId` | ENet thread | Peer ID assigned by ENet |

### Registering client-side packet handlers
```csharp
// In GameClient.RegisterPackets(); handlers run on the Godot main thread
// (called from HandlePackets()).
OnPacket<SPacketPlayerPositions>(packet => { ... });
```

### HandlePackets pump
`NetControlPanelLow._Process` calls `Net.Client.HandlePackets()` every frame.
If not using `NetControlPanelLow`, call it manually from `_Process` or `_PhysicsProcess`.
