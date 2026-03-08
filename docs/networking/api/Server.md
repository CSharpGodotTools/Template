Everything below is what `GodotServer` exposes as an API to you.

#### Properties
`bool IsRunning` - Returns `true` while the server worker thread is running.

#### Virtuals
`virtual void OnPeerDisconnect(Event netEvent)` - Called when a client disconnects or times out.

#### Methods
`void Start(ushort port, int maxClients, ENetOptions options, params Type[] ignoredPackets)` - Start the server on a worker thread.

`void Stop()` - Stop the server.

`void Send(ServerPacket packet, uint peerId)` - Send a packet to a single client.

`void Send(ServerPacket packet, IEnumerable<uint> peerIds)` - Send a packet to a collection of peers. The packet is serialized once and the resulting bytes are reused for each unicast, making this overload efficient for broadcasting to a list you already maintain.

`void Send(uint peerId, IEnumerable<ServerPacket> packets)` - Send multiple packets to a single peer. Useful when you’ve already constructed a sequence of payloads (e.g. via LINQ) and want to send them in one call.

`void Broadcast(IEnumerable<ServerPacket> packets)` - Broadcast a sequence of packets to all clients.

`void Broadcast(IEnumerable<ServerPacket> packets, uint excludePeerId)` - Broadcast a sequence of packets to all clients except the specified peer.

`bool SendThrottled(ServerPacket packet, IEnumerable<uint> peerIds, int intervalMs, bool force = false)` - Send to a set of peers, but only once per interval. The packet's type is used automatically as the throttle key; returns true if sent.

`bool BroadcastThrottled(ServerPacket packet, int intervalMs, bool force = false)` - Broadcast to everyone, rate‑limited by <paramref name="intervalMs"/>. The packet's type is used as the throttle key.

`bool BroadcastThrottled(ServerPacket packet, uint excludePeerId, int intervalMs, bool force = false)` - Rate‑limited broadcast with exclusion; uses packet type as key.

`void Broadcast(ServerPacket packet, params Peer[] clients)` - Broadcast a packet. If `clients.Length == 0`, it broadcasts to everyone. If `clients.Length == 1`, it broadcasts to everyone except that peer. If `clients.Length > 1`, it broadcasts to only those peers.

`void RegisterPacketHandler<TPacket>(Action<TPacket, Peer> handler)` - Register and handle a received `ClientPacket`. _(Protected method for server subclasses; call in your constructor.)_

`void Log(object message, BBColor color = BBColor.Gray)` - Log a message as the server. _Using `GD.Print` may lead to crashes if printing from multiple threads so always use `Log`._

`void Ban(uint id)` - Ban someone.

`void BanAll()` - Ban everyone.

`void Kick(uint id, DisconnectOpcode opcode)` - Kick someone.

`void KickAll(DisconnectOpcode opcode)` - Kick everyone.
