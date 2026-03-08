---
applyTo: "**/Template.GodotUtils/**/*.cs, **/Template.PacketGen/**/*.cs, **/Template.Visualize/**/*.cs"
---

# Cross-Project Sync Rule (Mandatory)

**After making any change** in any of the following folders:

- Template.GodotUtils/
- Template.PacketGen/
- Template.Visualize/

**you must commit the changes** (even if small).

### Why this is required
Committing triggers the `.githooks/pre-commit` hook, which automatically:
- Builds the changed project
- Packages the updated output (DLL and/or NuGet package)
- Copies/syncs the fresh artifacts into the main `Template` project folder

This ensures the `Template` project immediately sees and uses the latest versions of Godot utilities, packet generation code, visualization helpers, etc.

### Rules
- **Never** skip the commit step after modifying files in the folders above.
- Use a clear commit message (follow atomic commit guidelines from main instructions).
- Only after committing (and hook success) is the change considered complete and usable by the `Template` project.
- If the hook fails → fix the issue and commit again.

Failure to commit breaks downstream usage in the `Template` project.
