---
applyTo: "**/Netcode/{Net.cs,NetControlPanelLow.cs}"
---

# Coordinator & UI Panel

`Net<TGameClient, TGameServer>` is the single entry point for managing a server+
client pair.

- Initializes/deinitializes the ENet native library.
- Creates new instances of `TGameClient` and `TGameServer` on each `StartServer`/`StartClient` call.
- Events: `ServerCreated`, `ClientCreated`, `ClientDestroyed`.
- Properties: `Server`, `Client`, `ServerPort`, `ServerMaxClients`.
- `Dispose()` stops both server and client and tears down ENet.
- Registered with `Autoloads.PreQuit` to stop threads on Godot shutdown.
- `RequestShutdown()` offers an async, non‑disposing way to stop any running server or client and deinitialise ENet.
- **New**: exposes a static `HeartbeatPosition` constant (byte offset reserved for heartbeat sequences).
- `StartClient` is async; `Connect` returns a `Task`, and options mirror `ENetOptions` clones.

**Important**: `StartServer` and `StartClient` allocate new instances. Do not hold stale references to
old `Server` / `Client` objects after calling these methods.

`NetControlPanelLow<TGameClient, TGameServer> : Control` provides a small
Godot‑UI wrapper around a `Net<>` instance.

- Wires Start/Stop buttons and parses `<ip>:<port>` text input.
- Exposes `CurrentPort`/`CurrentMaxClients`.
- Subscribes to client lifecycle events to disable server controls while connected;
  unbinds events on `_ExitTree` to avoid leaked handlers.
- Calls `Net.Client.HandlePackets()` in `_Process`.
- Subclass overrides:
  * `DefaultPort`, `DefaultMaxClients`, `DefaultLocalIp` (used on startup),
  * `Options` (logging/diagnostic flags).

Common example subclass (`TopDown/NetControlPanel`) sets `DefaultMaxClients = 500`,
disables packet logging and uses the default local IP of `127.0.0.1`.
