# AGENT CONSTITUTION

> **Preserve this file's structure exactly — it is machine-consumed.**
> Last updated: 2026-03-01

---

## §1 Priority Hierarchy

When rules conflict, higher priority wins.

| Rank | Domain | Scope |
|------|--------|-------|
| **P0** | **Build & Functionality** | Code compiles, runs correctly, no regressions. |
| **P1** | **Safety & Nullability** | No defensive null checks on expected non-null values; let crashes expose bugs. |
| **P2** | **Core Logic** | DRY, explicit types, readability, correct APIs. |
| **P3** | **Structure & Naming** | File layout, line limits, naming conventions. |
| **P4** | **Formatting & Style** | Whitespace, brace style, comment aesthetics. |
| **P5** | **Communication** | Summaries, code-comment hygiene. |

---

## §2 Operational Workflow

Execute every task as this loop:

1. **Identify subproject.** Determine target: `Template`, `Template.GodotUtils`, or `Template.Visualize`. If ambiguous, ask the user **before proceeding**. Each subfolder is a separate project root (`Template/` = main game project).
2. **Apply all rules in this document** to every file touched. Never assume existing code is correct — fix bugs and inconsistencies in the affected area or raise them if out of scope.
3. **Build.** Run `cd <project-folder> && dotnet build`. Fix all compiler errors. Iterate until clean.
4. **Self-review.** Rebuild. Run relevant tests. Verify style/formatting compliance. Step through changes mentally for regressions.
5. **Declare complete** only after a green build and full review.

---

## §3 Core Logic Principles

* **Explicit types over `var`.** Declare concrete types; never rely on implicit inference.
* **Primary constructors.** Use when parameters are simple.
* **DRY.** Factor shared behaviour into helpers/methods. Reuse over duplication.
* **Readability wins.** Clear names, simple control flow, comments where intent is non-obvious. Bias toward readability in generation and refactoring.
* **Inline comments.** Concise, human-readable comments for non-obvious steps or performance considerations within method bodies.
* **Short type names.** No excessively long nested type names.
* **No external private field access.** Never reach into another file's private members (e.g. `custom._optionsManager`). Expose getters or use proper APIs.
* **Constants over magic values.** All literal numbers/strings become descriptive PascalCase constants.
* **Named types over tuples.** Return a class/struct, not `Tuple`/`ValueTuple`, for multiple return values.
* **C# events, not Godot signals.** Use standard C# events for decoupled notifications. Signals are banned.

---

## §4 Structural & Naming Constraints

* **File names:** max 21 characters (excluding extension). Single dot (extension only).
* **Trailing acronyms:** PascalCase (`Ui` not `UI`). Example: `OptionsCustomUiDropdown.cs`.
* **Descriptive names.** No abbreviations in filenames or type identifiers.
* **One type per file.** One class/struct/enum per `.cs` file.
* **Max 100 lines per file.** Extract helpers/types when a file exceeds this.
* **No `partial` for line-count reduction.** Refactor non-`Node` classes into helpers or nested types instead.
* **Subfolder grouping.** Organise related scripts: `Definitions/`, `Registry/`, `Ui/`, etc.

---

## §5 Formatting & Style

### Whitespace & Layout

* Separate logical blocks with blank lines.
* Comments wrap at ~100 characters; longer code lines need not break.
* Method parameter lists: one line unless >~100 characters.
* Expand braces to multiple lines; exception: very short one-liners.

### Collections & Initialisation

* Empty collections: use `[]`/`{}` initialisers, not `new()`.

### Usings & Namespaces

* `using` directives above the namespace declaration.
* Use short names for imported types (`Debug.Assert`, not `System.Diagnostics.Debug.Assert`).

### Access & Naming

* Mark fields and methods `private` explicitly; never rely on defaults.
* Private fields: `_camelCase`. Constants: `PascalCase`.

### Documentation

* Public API methods: human-readable XML doc comments.
* Omit summary comments on trivial types (self-evident enums, etc.).

### Organisation

* No `#region` blocks. Use well-named comment headers.
* **Member order** (flexible):
  1. Godot exports
  2. Events
  3. Properties
  4. Fields
  5. Godot overrides
  6. Public methods
  7. Private methods
  8. Private static methods
  9. Nested classes/enums

---

## §6 Communication Protocols

* **Post-task summaries:** max 100 characters.
* **No meta-comments in code** ("moved from X", "refactored because Y"). Write clear, self-contained comments only.
