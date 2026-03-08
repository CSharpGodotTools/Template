---
applyTo: "**/Netcode/ENet/Common/**/*.cs"
---

# ENet Common

Shared transport primitives used by both client and server.

## ENetOptions

`ENetOptions` is the central logging/diagnostics struct used by both client and
server wrappers. Fields are all booleans and default to `false` for packet‑data
and byte‑size printing, `true` for packet send/receive and `ShowLogTimestamps`.
Ignored packet types supplied to connect/start methods are automatically filtered
out of any logging.

## Layer 1 — Transport: ENetLow

`ENetLow` is the abstract base for both client and server.

- Spins a **worker loop** on a `LongRunning` TPL task.
- Each iteration: `ConcurrentQueues()` → `PumpNetworkEvents()`.
- `PumpNetworkEvents` calls `Host.CheckEvents` then `Host.Service(15ms timeout)`.
- Dispatches `Connect / Disconnect / Timeout / Receive` to abstract hooks.
- `CancellationTokenSource` (CTS) controls loop termination; cancelling it causes graceful exit.

**Thread rule**: Everything inside `WorkerLoop` runs on the **ENet worker thread**.

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

- **Fragment**: stored in `_reassemblyBuffers`.
  - On the **server**, the buffers are nested by `peerId` then by `streamId` so each client has
    its own set of in‑flight messages. When the last fragment for a stream is received the
    bytes are reassembled and handed off to the usual dispatch path.
  - On the **client**, a single map keyed by `streamId` is used; the entire map is cleared when
    the connection is torn down to avoid leaking buffers.

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
