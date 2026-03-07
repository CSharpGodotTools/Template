Everything below is what `GodotServer` exposes as an API to you.

#### Properties
`bool IsRunning` - Returns `true` while the server worker thread is running.

#### Virtuals
`virtual void OnPeerDisconnect(Event netEvent)` - Called when a client disconnects or times out.

#### Methods
`void Start(ushort port, int maxClients, ENetOptions options, params Type[] ignoredPackets)` - Start the server on a worker thread.

`void Stop()` - Stop the server.

`void Send(ServerPacket packet, Peer peer)` - Send a packet to a single client.

`void Broadcast(ServerPacket packet, params Peer[] clients)` - Broadcast a packet. If `clients.Length == 0`, it broadcasts to everyone. If `clients.Length == 1`, it broadcasts to everyone except that peer. If `clients.Length > 1`, it broadcasts to only those peers.

`void RegisterPacketHandler<TPacket>(Action<TPacket, Peer> handler)` - Register and handle a received `ClientPacket`. _(Protected method for server subclasses.)_

`void Log(object message, BBColor color = BBColor.Gray)` - Log a message as the server. _Using `GD.Print` may lead to crashes if printing from multiple threads so always use `Log`._

`void Ban(uint id)` - Ban someone.

`void BanAll()` - Ban everyone.

`void Kick(uint id, DisconnectOpcode opcode)` - Kick someone.

`void KickAll(DisconnectOpcode opcode)` - Kick everyone.
