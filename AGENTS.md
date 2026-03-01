# AGENTS

**Last updated:** 2026-03-01

* **IMPORTANT — maintain this file's formatting for agent consumption.**
  The structure and clarity are designed for automated readers.

## Task workflow

1. When editing C# sources, `cd Template && dotnet build` and fix any compiler errors.
   Iterate until the solution compiles; `Template/` is the game project root.
2. All scripts you touch must follow the guidelines in this document. Before
giving a change the ✅, re‑run the build and review new or modified files for
compliance.

## Core principles

* **Explicit types over `var`.** Declare concrete types rather than relying on
  implicit inference.
* **Prefer primary constructors.** Use them when parameters are simple; avoid
  empty constructors and initialise readonly fields at declaration.
* **DRY:** don’t duplicate logic. Factor shared behaviour into helpers or
  methods; reuse is more important than copy/paste.
* **Readable code wins.** Choose clear names, simple control flow, and add
  comments when intent might be unclear. The agent biases towards
  readability in generation and refactoring.
* **Inline comments.** Within method bodies provide concise, human‑readable
  comments explaining non‑obvious steps or performance considerations.
* **Short type names.** Avoid excessively long nested type names.
* **Avoid external private field access.** Never reach into another file’s
  private members (e.g. `custom._optionsManager`); expose getters or use
  proper APIs.
* **Magic values are constants.** Define all numbers/strings as descriptive
  constants with PascalCase names to prevent hard‑coded literals.
* **Return named types, not tuples.** If you need to return multiple related
  values, define a class or struct instead of a `Tuple`/`ValueTuple`.
* **No Godot signals.** Use standard C# events for decoupled notifications;
  signals are discouraged throughout the codebase.

## Formatting rules

* Separate logical blocks with blank lines; cramped code is hard to follow.
* Comments may run up to ~100 characters before wrapping; longer lines are fine
  and need not be manually broken.
* Use `[]`/`{}` initialisers for empty collections instead of `new()`.
* Keep method parameter lists on one line unless the signature exceeds ~100
  characters.
* Expand curly braces to multiple lines, except for very short one‑liners.
* Place `using` directives above the namespace declaration.
* When a type’s namespace is imported at the top of a file, refer to it by its short name rather than repeating the full path (`Debug.Assert` not `System.Diagnostics.Debug.Assert`).
* Always mark fields and methods `private` explicitly; never rely on defaults.
* Private fields use `_camelCase` naming; constants use PascalCase.
* Public API methods must have human‑readable XML documentation comments.
* Do not add summary comments for trivial types (e.g. enums with obvious names or members).
* Avoid `#region` blocks; use well‑named comment headers instead.
* Typical code order (flexible):
  1. Godot exports
  2. Events
  3. Properties
  4. Fields
  5. Godot overrides
  6. Public functions
  7. Private functions
  8. Private static functions
  9. Nested classes/enums

## Structural guidelines

* Keep file names under 21 characters (including `.cs`).
* Do not add extra dots to file names; only the extension may contain a dot.
* When a name ends in an acronym, prefer Pascal‑case (`Ui` not `UI`).
  e.g. `OptionsCustomUiDropdown.cs` not `OptionsCustomUIDropdown.cs`.
* Choose descriptive names and avoid abbreviating words in both filenames and
  type identifiers; readers shouldn’t need a decoder ring to understand what
  something is for.
* Group related scripts in subfolders (`Definitions`, `Registry`, `Ui`, etc.)
  as you work.
* Strive to keep each source file under 100 lines; extract helpers/types when
  a file grows too large.
* One class/struct/enum per file. Don’t bunch multiple definitions just to hit
  the line limit.
* Avoid `partial` solely for line‑count reasons. If a non‑`Node` class is too
  big, refactor into helpers or nested types instead.

## Safety & nullability

* Don’t add defensive null checks where values are expected to be non‑null; a
  crash reveals the real problem. Use Godot validity checks in place of
  `null` where appropriate.

## Communication

* Keep post‑task summaries under 100 characters when possible.
* Do not add meta‑comments in code explaining where something was moved from or
  why it was refactored; they clutter sources. Write clear, self‑contained
  comments instead.

