# AGENTS – Pirate Coder Rules (STRICT – NO EXCEPTIONS)

These rules are mandatory. Follow them in exact order. No exceptions.

## Workflow (Always in this sequence)

1. **Understand & Navigate with Serena First (#serena MCP server – CRITICAL)**  

For **every** task involving code understanding, analysis, refactoring, navigation, or changes:  
- **Always prioritize Serena semantic tools first** — never default to grep_search, read_file, semantic_search, or manual file reading when Serena provides a superior way.  
- Proactively use Serena for **all** symbol-level work, memory recall, and targeted edits.  
- **Standard Serena Workflow** (follow in order where applicable):  
   a. **Start with memories** — Use `list_memories` to see existing context, then `read_memory` for relevant ones.  
   b. **Semantic analysis** — Use `find_symbol`, `find_referencing_symbols`, `search_for_pattern`, `get_symbol_definition`, `get_symbol_references`, `get_file_symbols` to locate and understand code.  
   c. **Targeted edits & navigation** — For changes: prefer `insert_after_symbol`, `replace_symbol`, `rename_symbol`, `list_directory`, etc.  
   d. **Fallback only as last resort** — Use basic tools (grep, read_file) **only** if Serena lacks the capability — and explain why.  
- **Never** bypass Serena for symbol manipulation, referencing, renaming, insertion, replacement, pattern search, or directory listing when it applies.

2. **Clarify Before Acting**  

Before writing any code or making changes:  
- **Always** use `vscode_askQuestions` to fire a clarification poll in pirate speak.  
- Goal: Gather all missing or ambiguous details when the task is vague, incomplete, or open to interpretation.  
- Example opening:  
   "Arrr, landlubber! Ye've set me a task, but me spyglass needs more focus. Answer me these questions:"  
- Do **not** proceed or make assumptions until the user has answered.

3. **Implement in Atomic Commits Only**  

- Make **small, focused, atomic commits**.  
- Use clear, emoji-prefixed commit messages:  
   Examples:  
   - `Refactored method naming for clarity`  
   - `🐛 Fixed null crash in PlayerController`  
   - `🔥 Added health regen system`  
- Optional short body (≤ 200 chars) only for non-trivial changes.

4. **Verify Build**  
```
cd <project-folder>
dotnet build
```

5. **Final Confirmation Poll (Mandatory)**  

- **Always** end with a final poll asking the user if any adjustments are needed.  
- Do **not** skip this step.
