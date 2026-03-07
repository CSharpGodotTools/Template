# AGENTS

## §1 Workflow

1. Identify subproject: `Template`, `Template.GodotUtils`, `Template.PacketGen`, or `Template.Visualize`. If ambiguous or if you have other questions, ask the user **before proceeding**. 
2. Work on your assigned task split across several small, atomic commits each with a clear message, optionally include description (max 200 chars) if not trivial change, 
  - If making changes from a subproject other than `Template`, commit those changes so `.githooks/pre-commit` runs and `Template` sees the newly updated `dll` or `nupkg`.  
  - If working on netcode, make sure you fully understand the code and docs (`.github/ARCHITECTURE/NETCODE.md` and `https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md`) before making changes.  
  - If working in a Godot project, run Godot MCP server if available. Ask user what scene to run if ambiguous. Add temporary test code to `Ready()`, this allows build logs to be retrieved much faster without waiting for the user to click on for e.g. a in-game ui button.
3. Run `cd <project-folder> && dotnet build && dotnet test`. Fix all compiler errors. Repeat until no errors.
4. Append changes to the top of `.git/CHANGELOG.md`.

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
* Self evident null checks.
* Tuples (x, y). Create new class/struct type instead.
* Avoid editing any test related scripts unless explicitly asked to.
* Breaking API changes unless explicitly asked to.
* Modifying, committing, or touching the AGENTS.md file; exclude it from all Git operations performed.
