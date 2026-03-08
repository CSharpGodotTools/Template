---
applyTo: "**/*.cs"
---

# C# Development

## **SOLID Principles**

* **Single Responsibility Principle (SRP)**: A class should have only one reason to change.
* **Open/Closed Principle (OCP)**: Software entities should be open for extension but closed for modification.
* **Liskov Substitution Principle (LSP)**: Subtypes must be substitutable for their base types.
* **Interface Segregation Principle (ISP)**: No client should be forced to depend on methods it does not use.
* **Dependency Inversion Principle (DIP)**: Depend on abstractions, not on concretions.

## **.NET Good Practices**

* **Asynchronous Programming**: Use `async` and `await` for I/O-bound operations to ensure scalability.
* **Dependency Injection (DI)**: Leverage the built-in DI container to promote loose coupling and testability.
* **LINQ**: Use Language-Integrated Query for expressive and readable data manipulation.
* **Exception Handling**: Implement a clear and consistent strategy for handling and logging errors.
* **Modern C# Features**: Utilize modern language features (e.g., records, pattern matching) to write concise and robust code.

## Naming Conventions
- Never abbreviate type, method, or variable names (be descriptive).
- Replace all magic numbers/strings with named constants.
- Organize scripts/files into logical subfolders by feature/responsibility.
- Use **PascalCase** for types, methods, properties, and public members (e.g., `UiManager`, not `UI` or `uiManager`).
- Use **_camelCase** for private fields and local variables (with leading underscore).
- Prefix interfaces with `I` (e.g., `IUserService`, `IRepository`).

## Documentation & Comments
- Never add meta-comments (e.g., "moved from...", "refactored because...").
- Always provide XML doc comments for **public** types/members (include `<summary>`, `<param>`, `<returns>`, and `<example>`/` <code>` when useful).
- Comments only when intent is non-obvious; keep them concise (~100 chars wrapped).

## Formatting & Structure
- Strictly follow `.editorconfig` for formatting (line length, indentation, etc.).
- Prefer **file-scoped namespaces** and group `using` directives at file top (remove unused).
- Prefer **primary constructors** for simple classes/records.
- Always insert newline before opening `{` of blocks (if, for, while, foreach, using, try, etc.).
- Expand braces `{}` for all blocks except very short one-liners (single expression).
- Place final `return` on its own line.
- Prefer pattern matching, switch expressions, and records over older constructs.
- Use `nameof` for member names instead of magic strings.
- Prefer qualified names via `using` imports over fully qualified names.
- Use collection/object initializers for empty/new instances (`new() { }`, `[]`, `{}`).
- Explicitly mark `private` on fields/methods.
- Make private methods `static` when they have no instance access.

## Nullable Reference Types
- Declare variables non-nullable by default.
- Check for `null` only at true entry points (parameters, external data).
- Always use `is null` / `is not null` instead of `== null` / `!= null`.
- Trust compiler nullability annotations — do **not** add redundant null checks.

## Performance & Best Practices
- Prefer async/await patterns
- Use response compression and benchmark with tools like BenchmarkDotNet when optimizing.
- Measure performance before/after changes (don't optimize prematurely).

## Hard Bans (Never Use)
- `var` keyword
- `#region` directives
- Godot signals (use direct method calls or events instead)
- Self-evident/redundant null checks
- Tuple literals `(x, y)` — create a dedicated class/struct/record
- Breaking API changes (unless explicitly requested)
