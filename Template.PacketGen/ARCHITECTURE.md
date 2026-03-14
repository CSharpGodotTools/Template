# PacketGen Architecture

This document explains how PacketGen works so new contributors can navigate the code quickly.

## Project Layout

- `PacketGen/`: Roslyn incremental source generator.
- `PacketGen.Tests/`: Generator tests that compile generated code and validate behavior.

## Generation Flow

1. `PacketGenerator` discovers packet types (`ClientPacket`/`ServerPacket`) and registry targets (`[PacketRegistry]`).
2. `PacketGenerationOrchestrator` analyzes each packet and emits:
   - `Write(PacketWriter writer)`
   - `Read(PacketReader reader)`
   - `Equals(...)`
   - `GetHashCode()`
3. Type-specific code is delegated to `ITypeHandler` implementations via `TypeHandlerRegistry`.

## Key Files

- `PacketGen/PacketGenerator.cs`: incremental generator entry point.
- `PacketGen/Generators/PacketGenerationOrchestrator.cs`: per-packet source assembly.
- `PacketGen/Generators/PacketRegistryGenerator.cs`: registry source generation.
- `PacketGen/Generators/TypeHandlers/*`: serialization logic per type shape.
- `PacketGen/Generators/TypeHandlers/SerializablePropertySelector.cs`: central filter for serializable properties.
- `PacketGen/PacketGenConstants.cs`: shared string constants.

## Type Handler Model

Each type handler implements:

- `CanHandle(ITypeSymbol)`: whether this handler supports the type.
- `EmitWrite(...)`: emits write lines.
- `EmitRead(...)`: emits read lines.

Current handlers:

- `PrimitiveTypeHandler`
- `ArrayTypeHandler`
- `ListTypeHandler`
- `DictionaryTypeHandler`
- `ComplexTypeHandler`

`CollectionLoopEmitter` is shared by array/list handlers to reduce loop emission duplication.

## Safety and Limits

- `ComplexTypeHandler` caps recursive traversal depth (`MaxTraversalDepth`) to avoid infinite recursion.
- Unsupported shapes (for example rectangular arrays) emit diagnostics through `Logger`.

## How To Add A New Supported Collection Type

1. Add a new handler in `PacketGen/Generators/TypeHandlers/`.
2. Implement `CanHandle`, `EmitWrite`, and `EmitRead`.
3. Register it in `PacketGenerationOrchestrator.BuildRegistry()` in desired priority order.
4. Add generator tests in `PacketGen.Tests/Tests/`.

## Test Strategy

Tests use `GeneratedAssemblyHarness` to:

1. Run generator.
2. Compile generated source with packet stubs.
3. Instantiate generated packet types via reflection.
4. Assert runtime read/write and equality behavior.
