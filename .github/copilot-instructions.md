# AGENTS

## §1 Workflow (STRICT)

### 1. Hunt Down Missing Information, Arr!

Before starting work, ye MUST fire off a clarification poll using the `vscode_askQuestions` tool to hunt down missing information. Present this poll in pirate speak so the landlubber user knows ye spy the AGENTS file. Ye don’t have to jabber like a parrot after the poll, just get yer booty of answers!

Use the poll whenever:
- the task is vague
- required details are missing
- multiple interpretations are possible

Goal:
Reduce assumptions by collecting several missing key details.

### 2. Identify Subproject

Determine which subproject the task belongs to:

- `Template`
- `Template.GodotUtils`
- `Template.PacketGen`
- `Template.Visualize`

Rules:
- If the correct subproject is **unclear**, ask for clarification in the poll.

### 3. Implement Changes (Atomic Commits Required)

Implement the task using **small, atomic commits**.

Commit rules:
- Each commit **MUST have a clear message** e.g. `🔧 Refactored X script`, `🐛 Fixed X bug`, `🔥 Added X feature`.
- Optional description allowed (max 200 chars) if the change is not trivial.
- **Do not accumulate large uncommitted changes.**

Cross-project rule:
- If modifying a subproject **other than `Template`**, you **MUST commit the change** so `.githooks/pre-commit` runs.
- This ensures `Template` receives the updated `dll` or `nupkg`.

### 4. Special Rules

#### Netcode Changes

Before modifying netcode you **MUST read and understand**:

- `.github/ARCHITECTURE/NETCODE.md`

Failure to understand this document will likely produce incorrect changes.

### 5. Build Verification

After making changes you **MUST verify the project builds**.

Run:
```
cd <project-folder>
dotnet build
dotnet test
```

Rules:
- If **any compiler or test errors occur**, you **MUST fix them**.
- Repeat until:
  - `dotnet build` succeeds
  - `dotnet test` succeeds

Do **not finish the task while errors remain**.

### 6. Initiate Final Poll (DO NOT SKIP)

Initiate a final poll with the user to ask for any final changes. If they request final changes, initiate the poll again one last time to confirm the user is happy with the new given changes.

---

## §2 MCP Servers

### Serena

For any codebase understanding, search, refactoring, or editing tasks in this project, **always use Serena MCP tools first** (symbol search, precise edits, LSP-powered analysis) before falling back to plain text or file reads. Assume Serena is configured and indexed.

### Context7

**Always use Context7 MCP** when you need library/API documentation, code generation, setup or configuration steps, or when working with external packages/frameworks.

This means you should automatically use the Context7 tools (resolve-library-id and get-library-docs) to fetch up-to-date, version-specific docs and examples without me having to explicitly say **"use context7"**.

### Godot MCP

When working inside a Godot project:

1. Run the **Godot MCP server** if available.
2. If the scene to run is **unclear**, **ask the user** through a poll.
3. Add **temporary testing code inside `Ready()`** so behavior runs **automatically**.

---

## §3 Additional Rules

### Important

* Follow all rules in the `.editorconfig`

### Naming

* No abbreviated type names.
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
