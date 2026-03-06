# AGENTS

> **Do not modify this files contents — it is machine-consumed.**
> Godot Version: v4.6.1.stable.mono.official [14d19694e]
> net10.0 latest C# version

---

## §1 Workflow

1. Identify subproject: `Template`, `Template.GodotUtils`, `Template.PacketGen`, or `Template.Visualize`. If ambiguous, ask the user **before proceeding**.
2. Work on your assigned task. Break task into small changes. Commit each separately with clear message. No single big commit. use context7
3. Run `cd <project-folder> && dotnet build && dotnet test`. Fix all compiler errors. Repeat until no errors.

---

## §2 Rules

### Naming

* No abbreviated type names.
* Private fields: `_camelCase`. Constants: `PascalCase`.
* For PascalCase use for e.g. `Ui` not `UI`.
* All type names max 22 characters.

### Readability

* Write comments where intent is non-obvious.
* Separate logical blocks with blank lines.
* Replace magic values with constants.
* Comments wrap at ~100 characters; longer code lines need not break.
* Expand braces to multiple lines; exception: very short one-liners.
* No meta-comments in code ("moved from X", "refactored because Y").
* Public API methods: human-readable XML doc comments.
* Omit summary comments on trivial types (self-evident enums, etc.).
* Create subfolders as needed to organize scripts.

### Flow

* Use proper OOP principles.
* Use primary constructors.
* `using` directives above the namespace declaration.
* Use usings; e.g. write `Debug.Assert` instead of `System.Diagnostics.Debug.Assert`.
* Use `[]`/`{}` initialisers for empty collections, not `new()`.
* Mark fields and methods `private` explicitly.
* Mark private methods `static` if they do not access instance members.

### Bans

The following are banned in C#:
* `var`.
* `#region`.
* Godot signals.
* Trivial self evident null checks.
* Tuples (x, y). Create new class/struct type instead.
* Avoid editing any test related scripts unless explicitly asked to.
* Breaking API changes unless explicitly asked to.
