Everything below is what `GodotClient` exposes as an API to you.

#### Config
`uint PingIntervalMs` - Ping interval in ms. Default is `1000`. 

`uint PeerTimeoutMs` - Peer timeout in ms. Default is `5000`.

`uint PeerTimeoutMinimumMs` - Peer timeout minimum in ms. Default is `5000`. 

`uint PeerTimeoutMaximumMs` - Peer timeout maximum in ms. Default is `5000`.

#### Properties
`bool IsRunning` - Returns `true` while the client worker thread is running.

`bool IsConnected` - Returns `true` if the client is connected to the server.

`uint PeerId` - The assigned ENet peer id for this client connection.

#### Events
`event Action Connected` - Client connected to the server.

`event Action<DisconnectOpcode> Disconnected` - Client disconnected or timed out from the server.

`event Action Timedout` - Client timed out from the server.

#### Virtuals
`virtual void OnConnect(Event netEvent)` - The client connects to the server.

`virtual void OnDisconnect(Event netEvent)` - The client disconnects from the server.

`virtual void OnTimeout(Event netEvent)` - The client timed out.

#### Methods
`Task Connect(string ip, ushort port, ENetOptions options = default, params Type[] ignoredPackets)` - Start and connect the client on a worker thread.

`void Stop()` - Stop the client.

`void HandlePackets()` - Process queued packet and connection events on the Godot thread (call from `_Process` or `_PhysicsProcess`).

`void RegisterPacketHandler<TPacket>(Action<TPacket> handler)` - Register and handle a received `ServerPacket`. _(Protected method for client subclasses.)_

`void Send(ClientPacket packet)` - Send a packet from the client to the server.

`void Log(object message, BBColor color = BBColor.Gray)` - Log a message as the client. _Using `GD.Print` may lead to crashes if printing from multiple threads so always use `Log`._
