---
applyTo: "**/Netcode/Opcodes/**/*.cs"
---

# Opcode Definitions

Opcodes live in the `Netcode/Opcodes` folder and represent special commands used
internally (disconnect reasons, ENet client/server commands, Godot thread messages).

The important wire‑format rules are handled by the packet system (see packet
instructions), but note:

- Disconnect codes (`DisconnectOpcode`) enumerate various reasons: Disconnected,
  Maintenance, Restarting, Stopping, Timeout, Kicked, Banned.
- Internal ENet opcodes (`ENetClientOpcode` / `ENetServerOpcode`) are used by
  the transport layer queues.
- `GodotOpcode` values indicate actions that the ENet thread requests the Godot
  main thread to perform.

These files are simple enums; no serialization logic is generated for them.

> **Remember**: wire packet opcodes are unrelated to these enum values — they
> are assigned by `PacketRegistry` and always occupy the first two bytes of every
> user packet.
