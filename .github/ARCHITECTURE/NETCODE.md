# Netcode Architecture

Agent reference document. Describes every layer of the netcode system bottom-up.
All paths are relative to the repo root unless noted.

---

## Directory Map

```
Template/Framework/Netcode/
  ENet/
    ENetLow.cs                 -- shared worker-thread event loop (base class)
    ENetServer.cs              -- server: peer tracking, queue processing
    ENetClient.cs              -- client: connection lifecycle, queue processing
    GodotServer.cs             -- game-level server facade (Start/Stop/Send/Broadcast/Kick)
    GodotClient.cs             -- game-level client facade (Connect/Stop/Send/HandlePackets)
    ENetOptions.cs             -- logging flags struct
    ServerLogAggr.cs           -- coalesces rapid connect/disconnect/timeout into burst logs
    ClientLogAggr.cs           -- same for client side
    EventLogAggregator.cs      -- shared base for aggregators
  Packet/
    GamePacket.cs              -- base packet (opcode + Write/Read, MaxSize=8192)
    ClientPacket.cs            -- marker: packet travels client→server
    ServerPacket.cs            -- marker: packet travels server→client
    PacketRegistry.cs          -- partial class; PacketGen fills it with opcode maps
    PacketRegistryAttribute.cs -- [PacketRegistry] triggers PacketGen
    PacketWriter.cs            -- BinaryWriter over MemoryStream
    PacketReader.cs            -- BinaryReader over copied ENet packet bytes
    PacketData.cs              -- data-transfer object for cross-thread packet delivery
  ENet/
    PacketFragmenter.cs        -- splits oversized packets into fragments; reassembles on receive
  Opcodes/
    DisconnectOpcode.cs        -- Disconnected/Maintenance/Restarting/Stopping/Timeout/Kicked/Banned
    ENetClientOpcode.cs        -- internal queue commands for client worker thread
    ENetServerOpcode.cs        -- internal queue commands for server worker thread
    GodotOpcode.cs             -- commands pushed from ENet thread to Godot thread
  Net.cs                       -- top-level coordinator: owns one server + one client pair
  NetControlPanelLow.cs        -- Godot Control base; wires UI buttons to Net<>
  Utils/Cmd.cs                 -- generic command envelope used by internal queues
  Attributes/NetExcludeAttribute.cs -- [NetExclude] skips a member in PacketGen serialization
  Examples/
    TopDown/                   -- client-authoritative example (test scene is here)
    TopDown2/                  -- minimal scaffold / blank starting point
Template.PacketGen/            -- Roslyn IIncrementalGenerator project
```

---

## Layer 1 — Transport: ENetLow

`ENetLow` is the abstract base for both client and server.

- Spins a **worker loop** on a `LongRunning` TPL task.
- Each iteration: `ConcurrentQueues()` → `PumpNetworkEvents()`.
- `PumpNetworkEvents` calls `Host.CheckEvents` then `Host.Service(15ms timeout)`.
- Dispatches `Connect / Disconnect / Timeout / Receive` to abstract hooks.
- `CancellationTokenSource` (CTS) controls loop termination; cancelling it causes graceful exit.

**Thread rule**: Everything inside `WorkerLoop` runs on the **ENet worker thread**.

---

## Layer 2 — Server Stack

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
- `_clientPacketHandlers Dictionary<Type, Action<ClientPacket, uint>>` — registered via `OnPacket<T>`.
  - Handlers **run on the ENet worker thread**.
- `ConcurrentQueues()` order each tick: ENet commands → incoming packets → outgoing packets → log aggregator flush.
- `OutgoingMessage` factories: `Unicast(data, peerId)`, `Broadcast(data)`, `BroadcastExcept(data, excludePeerId)`.

### GodotServer API (all thread-safe)
| Method | Description |
|--------|-------------|
| `Start(port, maxClients, options, ignoredPackets)` | Launches worker thread |
| `Stop()` | Enqueues stop command |
| `Send(ServerPacket, uint peerId)` | Serializes + enqueues unicast |
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
OnPacket<CPacketPlayerPosition>((packet, peerId) => { ... });
```

---

## Layer 3 — Client Stack

```
ENetLow
  └── ENetClient        (connection lifecycle, queues, dispatch)
        └── GodotClient (game API; abstract – subclass per game)
              └── GameClient (concrete, registers packets)
```

### ENetClient responsibilities
- `_peer Peer` — the single ENet peer representing the server connection.
- Configurable timeout/ping (overridable properties, all 5000ms/1000ms defaults).
- On ENet thread: `OnConnected()`, `OnDisconnected()`, `OnTimedOut()` virtual hooks for subclasses.
- `MainThreadCommands ConcurrentQueue<Cmd<GodotOpcode>>` — lifecycle signals pushed to Godot thread.
- `MainThreadPackets ConcurrentQueue<PacketData>` — received packets pushed to Godot thread.
- `_connected` Interlocked long flag — readable from any thread via `IsConnected`.

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

---

## Layer 4 — Net<TClient, TServer> Coordinator

`Net<TGameClient, TGameServer>` is the single entry point for managing a server+client pair.

- Initializes/deinitializes the ENet native library.
- Creates new instances of `TGameClient` and `TGameServer` on each `StartServer`/`StartClient` call.
- Events: `ServerCreated`, `ClientCreated`, `ClientDestroyed`.
- Properties: `Server`, `Client`, `ServerPort`, `ServerMaxClients`.
- `Dispose()` stops both server and client and tears down ENet.
- Registered with `Autoloads.PreQuit` to stop threads on Godot shutdown.

**Important**: `StartServer` and `StartClient` allocate new instances. Do not hold stale references to
old `Server` / `Client` objects after calling these methods.

---

## Layer 5 — Packet System

### Packet class hierarchy
```
GamePacket  (abstract)
  ├── ClientPacket  (abstract) — travels client → server
  └── ServerPacket  (abstract) — travels server → client
```

### Wire format
```
[opcode: 2 bytes (ushort LE)][payload: variable]
```
Max packet size: **8192 bytes**. Packets exceeding this are automatically fragmented by
`PacketFragmenter` and reassembled at the receiver before dispatch (see Packet Fragmentation below).

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

---

## Packet Fragmentation

`PacketFragmenter` (`ENet/PacketFragmenter.cs`) transparently handles packets that would
exceed `GamePacket.MaxSize = 8192` bytes.

### Sender (ENetServer / ENetClient outgoing queue)
When the serialized byte array exceeds `MaxSize`, it is split into fragments:
```
[fragment opcode: 2 bytes]  PacketRegistry.FragmentOpcode
[streamId: 2 bytes]         identifies this logical message (wraps around ushort)
[fragIndex: 2 bytes]        0-based fragment index
[totalFrags: 2 bytes]       total fragment count
[payload: up to 8184 bytes] fragment slice of the original data
```
Header is 8 bytes, so each fragment carries up to **8184 bytes** of payload.
Small packets (< `MaxSize`) are sent as-is with zero overhead.

### Receiver (ENetServer / ENetClient incoming queue)
When a packet arrives, the first 2 bytes are checked against `PacketRegistry.FragmentOpcode`.
- **Fragment**: stored in `_reassemblyBuffers` keyed by `(peerId, streamId)`. Once all fragments
  arrive, the orignal byte sequence is reassembled and dispatched normally.
- **Normal packet**: dispatched immediately.

### Reserved opcode
`PacketRegistry.FragmentOpcode` equals the maximum value of the configured opcode type:
- `[PacketRegistry]` (byte) → `FragmentOpcode = 255`
- `[PacketRegistry(typeof(ushort))]` → `FragmentOpcode = 65535`

PacketGen validates at code-generation time that no user packet is ever assigned this value.

### Packet size example
`SPacketPlayerPositions` with N subscribers:
- Wire size = `2 (opcode) + 4 (count) + N × 12 (id:4 + x:4 + y:4)` bytes
- At N = 680: 8186 bytes — fits in one packet (≤ 8192)
- At N = 681: 8198 bytes — split into 2 fragments automatically

---

## Layer 6 — NetControlPanelLow

`NetControlPanelLow<TClient, TServer> : Control`
- Owns `Net<TClient, TServer>`.
- Wires Start Server / Stop Server / Start Client / Stop Client UI buttons.
- Calls `Net.Client.HandlePackets()` in `_Process`.
- Subclass overrides: `DefaultPort`, `DefaultMaxClients`, `DefaultLocalIp`, `Options`.
- Example: `NetControlPanel` (TopDown) sets `DefaultMaxClients = 500` and disables all packet logging.

---

## TopDown Example — Client-Authoritative Multiplayer

**Test scene**: `res://Framework/Netcode/Examples/TopDown/World.tscn`

### Packet inventory
| Packet | Direction | Fields | Purpose |
|--------|-----------|--------|---------|
| `CPacketPlayerJoinLeave` | C→S | `Joined: bool` | Join or leave the session |
| `CPacketPlayerPosition` | C→S | `Position: Vector2` | Client reports own position |
| `CPacketSubscribePositions` | C→S | *(none)* | Opt in to position broadcasts |
| `SPacketPlayerJoinedLeaved` | S→C | `Id: uint, Joined: bool, IsLocal: bool` | Notify of player join/leave |
| `SPacketPlayerPositions` | S→C | `Positions: Dictionary<uint, Vector2>` | Batch position snapshot |

### Connection flow
1. Client connects → ENet fires `OnConnected()` on worker thread.
2. Client sends `CPacketPlayerJoinLeave { Joined = true }`.
3. If `IsPositionSubscriber` is true, client also sends `CPacketSubscribePositions`.
4. Server `AddPlayer`: tells new peer their own ID (`SPacketPlayerJoinedLeaved { IsLocal=true }`), broadcasts join to others, sends existing player list.
5. Server optionally adds peer to `_positionSubscribers` and sends current position snapshot.

### Position update flow
```
LocalPlayer.Tick()
  → throttled at 20 Hz (SendIntervalSeconds=0.05s)
  → only if moved ≥ 0.5px (SendEpsilonSq=0.25)
  → client.SendPosition(pos)
  → CPacketPlayerPosition → server

Server.OnPlayerPosition()
  → stores in _positions[peerId]
  → BroadcastPositions() — rate-limited at 10 Hz (PositionBroadcastIntervalMs=100)
  → serializes SPacketPlayerPositions once
  → unicasts to each peer in _positionSubscribers only
```

### Bot / subscriber optimization
Bots set `IsPositionSubscriber = false` before connecting.
They never send `CPacketSubscribePositions`, so the server never adds them to `_positionSubscribers`.
Result: the server sends zero position packets to bots regardless of how many bots are connected.
This is the **critical optimization** that lets the system scale to hundreds of bots.

### Class responsibilities (TopDown)
| Class | File | Responsibility |
|-------|------|----------------|
| `World` | `World.cs` | Scene root; wires `NetControlPanel`, `LocalPlayer`, `RemotePlayers`, `WorldStressTest` |
| `NetControlPanel` | `UI/NetControlPanel.cs` | Thin `NetControlPanelLow` subclass; sets max clients=500, logging off |
| `GameClient` | `Client/GameClient.cs` | Registers packet handlers; fires typed C# events for World to consume |
| `GameServer` | `Server/GameServer.cs` | Registers packet handlers; manages `_players`, `_positionSubscribers`, `_positions` |
| `LocalPlayer` | `LocalPlayer.cs` | Input → movement → throttled position sends |
| `RemotePlayers` | `RemotePlayers.cs` | Creates/lerps ColorRect nodes for remote peers |
| `WorldStressTest` | `WorldStressTest.cs` | UI-driven bot spawner; spawns `BotClient` instances and ticks them; shows live stats (status, active bots, elapsed time, connected peers, spawn rate) |
| `BotClient` | `WorldStressTest.cs` (nested) | One `GameClient` per bot; orbits in a circle; sends position at configurable interval |

### Key constants
| Constant | Value | Location |
|----------|-------|----------|
| `LocalPlayer.SendIntervalSeconds` | 0.05 s (20 Hz) | `LocalPlayer.cs` |
| `LocalPlayer.SendEpsilonSq` | 0.25 (0.5 px) | `LocalPlayer.cs` |
| `LocalPlayer.MoveSpeed` | 200 px/s | `LocalPlayer.cs` |
| `GameServer.PositionBroadcastIntervalMs` | 100 ms (10 Hz) | `GameServer.cs` |
| `RemotePlayers.RemoteLerpSpeed` | 6.0 | `RemotePlayers.cs` |
| `WorldStressTest.DefaultTargetClients` | 250 | `WorldStressTest.cs` |
| `WorldStressTest.DefaultSendIntervalSeconds` | 0.05 s | `WorldStressTest.cs` |
| `WorldStressTest.DefaultMaxClients` | 500 | `WorldStressTest.cs` |
| `WorldStressTest.DefaultPort` | 25565 | `WorldStressTest.cs` |

---

## TopDown2 Example

Minimal scaffold. `GameClient` and `GameServer` have empty `RegisterPackets()`.
`World.cs` creates `Net<>`, calls `StartServer` + `StartClient`. No packets, no game logic.
Use as a copy-paste starting point for a new example.

---

## Threading Summary

| Code location | Thread |
|---------------|--------|
| `ENetServer.OnPacket<T>` handler body | ENet worker thread |
| `GodotClient.OnPacket<T>` handler body | Godot main thread (via `HandlePackets()`) |
| `GodotClient.Connected / Disconnected / TimedOut` events | Godot main thread |
| `GodotServer.Send / Broadcast / Kick / Ban` | Any thread (thread-safe) |
| `GodotClient.Send / Stop` | Any thread (thread-safe) |
| `ENetClient.OnConnected / OnDisconnected / OnTimedOut` virtuals | ENet worker thread |
| `GameServer.OnPeerDisconnected` virtual | ENet worker thread |

---

## Gotchas

- **Opcode determinism**: Opcodes are assigned by sorted fully-qualified type name. Renaming or adding a packet type changes all opcodes — always rebuild both client and server together.
- **HandlePackets() is mandatory**: Call it from `_Process` on the Godot main thread. Packets accumulate silently in the queue if omitted.
- **StartServer / StartClient create new instances**: After calling these, `Net.Server` and `Net.Client` point to new objects. Unsubscribe from old instances first or use `Net.ServerCreated` / `Net.ClientCreated` events.
- **MaxSize 8192**: Packets larger than this are automatically fragmented by `PacketFragmenter`. No user code needed. Small packets are unaffected.
- **Fragment opcode reservation**: `PacketRegistry.FragmentOpcode` is always the max value of the configured opcode type. PacketGen will throw at generation time if any user packet would be assigned this value.
- **Bot subscription**: Bots MUST set `IsPositionSubscriber = false` before connecting. If they subscribe, they receive all position broadcasts and will cause O(N) outgoing traffic per position update.
- **Server packet handlers run on ENet thread**: Do not access Godot scene nodes from a server packet handler. Queue work to the main thread if Godot API access is needed.
- **`BroadcastPositions` serializes once, unicasts N times**: The `SPacketPlayerPositions` byte array is shared across all subscriber unicasts — do not modify `_positions` during the loop.

---

## How to Add a New Example

1. Create `Template/Framework/Netcode/Examples/MyExample/`.
2. Define client packets in `Client/Packets/` extending `ClientPacket`.
3. Define server packets in `Server/Packets/` extending `ServerPacket`.
   - PacketGen auto-generates `Write` / `Read` for all public properties.
   - Add `[NetExclude]` to skip a property.
   - Override `Write(PacketWriter)` / `Read(PacketReader)` manually for complex layouts.
4. Create `GameClient : GodotClient`:
   - `RegisterPackets()` calls `OnPacket<SomeServerPacket>(handler)`.
   - Handlers run on the Godot main thread.
5. Create `GameServer : GodotServer`:
   - `RegisterPackets()` calls `OnPacket<SomeClientPacket>((packet, peerId) => handler)`.
   - Handlers run on the ENet worker thread.
   - Override `OnPeerDisconnected(uint peerId)` for cleanup.
6. Create a `NetControlPanelLow<GameClient, GameServer>` subclass for the UI panel.
7. Wire the scene: root node calls `_netControlPanel.Net.ClientCreated` etc., or use `Net<>` directly.
8. If not using `NetControlPanelLow`, call `Net.Client.HandlePackets()` in `_Process`.
