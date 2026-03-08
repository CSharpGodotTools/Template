---
applyTo: "**/Netcode/Examples/**/*.cs"
---

# Example Projects

Two demonstration scenes live under `Netcode/Examples`.

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

## TopDown2 Example

Minimal scaffold. `GameClient` and `GameServer` have empty `RegisterPackets()`.
`World.cs` creates `Net<>`, calls `StartServer` + `StartClient`. No packets, no game logic.
Use as a copy-paste starting point for a new example.
