---
applyTo: "**/Netcode/**/*.cs"
---

# Threading & Gotchas

Several important threading rules and gotchas apply across the netcode stack:

- **Opcode determinism**: Opcodes are assigned by sorted fully-qualified type name. Renaming or adding a packet type changes all opcodes — always rebuild both client and server together.
- **HandlePackets() is mandatory**: Call it from `_Process` on the Godot main thread. Packets accumulate silently in the queue if omitted.
- **StartServer / StartClient create new instances**: After calling these, `Net.Server` and `Net.Client` point to new objects. Unsubscribe from old instances first or use `Net.ServerCreated` / `Net.ClientCreated` events.
- **MaxSize 8192**: Packets larger than this are automatically fragmented by `PacketFragmenter`. No user code needed. Small packets are unaffected.
- **Fragment opcode reservation**: `PacketRegistry.FragmentOpcode` is always the max value of the configured opcode type. PacketGen will throw at generation time if any user packet would be assigned this value.
- **Bot subscription**: Bots MUST set `IsPositionSubscriber = false` before connecting. If they subscribe, they receive all position broadcasts and will cause O(N) outgoing traffic per position update.
- **Server packet handlers run on ENet thread**: Do not access Godot scene nodes from a server packet handler. Queue work to the main thread if Godot API access is needed.
- **PacketFromPeer wrapper**: `ENetServer.OnPacket<T>` callbacks no longer take `(packet, peerId)`; the handler receives a `PacketFromPeer<T>` struct. Adjust existing code accordingly.
- **ENetOptions.ShowLogTimestamps** defaults to `true` and will prefix every log entry with a timestamp; disable if you prefer cleaner output.
- **Net.RequestShutdown** offers a way to tear down network threads without disposing the `Net<>` object; this is handy for tests or when keeping the coordinator alive.
- **StartClient returns a Task**: `Net.StartClient` begins an asynchronous connect. If you need to wait until the client worker has started, await the returned `Client.Connect` Task directly.
- **HeartbeatPosition reserved**: the `Net.HeartbeatPosition` constant (20) indicates a wire offset reserved for heartbeat sequence numbers. Avoid using that byte range in custom packet payloads.
- **`BroadcastPositions` serializes once, unicasts N times**: The `SPacketPlayerPositions` byte array is shared across all subscriber unicasts — do not modify `_positions` during the loop.

## Threading Summary

| Code location | Thread |
|---------------|--------|
| `ENetServer.OnPacket<T>` handler body (receives `PacketFromPeer<T>`) | ENet worker thread |
| `GodotClient.OnPacket<T>` handler body | Godot main thread (via `HandlePackets()`) |
| `GodotClient.Connected / Disconnected / TimedOut` events | Godot main thread |
| `GodotServer.Send / Broadcast / Kick / Ban` | Any thread (thread-safe) |
| `GodotClient.Send / Stop` | Any thread (thread-safe) |
| `ENetClient.OnConnected / OnDisconnected / OnTimedOut` virtuals | ENet worker thread |
| `GameServer.OnPeerDisconnected` virtual | ENet worker thread |
