# Git Hooks Setup

All hooks live in the version-controlled `.githooks/` directory. Configure Git to use them instead of `.git/hooks/` by running the one-time setup command below.

---

## Quick Setup

### Linux / macOS

```bash
git config core.hooksPath .githooks
chmod +x .githooks/*
```

### Windows (Git Bash)

Open a **Git Bash** terminal in the repository root:

```bash
git config core.hooksPath .githooks
```

Git Bash already ships with all required utilities (`bash`, `grep`, `sed`, `find`, `diff`), so no additional dependencies are needed.

> Hooks are written in **bash**, so PowerShell or CMD alone cannot run them. Use Git Bash, WSL, or another POSIX shell.

---

## Flatpak GitHub Desktop

If you are using the Flatpak build of GitHub Desktop (`io.github.shiftey.Desktop`), the hooks automatically detect the sandbox and execute `dotnet` on the host system using:

```bash
flatpak-spawn --host dotnet
```

No additional configuration is required.

---

## Hook Overview

| Hook            | Trigger                        | Action                                                                                                                                |
| --------------- | ------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------- |
| `pre-commit`    | Before any commit              | Handles `.editorconfig` sync and automated builds for GodotUtils, Visualize, and PacketGen. The commit is aborted if any build fails. |
| `post-commit`   | After commit                   | Intentionally empty. GitHub Desktop does not execute this hook.                                                                       |
| `post-checkout` | Switching branches             | Syncs `.editorconfig` from root to all subprojects when differences are detected.                                                     |
| `post-merge`    | After `git merge` / `git pull` | Syncs `.editorconfig` when the root version changed during the merge.                                                                 |

---

## pre-commit Behavior

### `.editorconfig`

When the root `.editorconfig` file is staged:

1. The file is copied to each subproject root
2. The copies are automatically staged
3. All changes land in the same commit

Subprojects receiving the sync:

```
Template/
Template.GodotUtils/
Template.PacketGen/
Template.Visualize/
```

---

### GodotUtils

When files inside:

```
Template.GodotUtils/
```

are staged:

```
dotnet build Template.GodotUtils/GodotUtils.csproj
```

Generated files staged automatically:

```
Template/Framework/Libraries/GodotUtils.dll
Template/Framework/Libraries/GodotUtils.xml
```

---

### Visualize

When files inside:

```
Template.Visualize/
```

are staged:

```
dotnet build Template.Visualize/Visualize.csproj
```

Generated files staged automatically:

```
Template/Framework/Libraries/Visualize.dll
Template/Framework/Libraries/Visualize.xml
```

Visualize depends on **GodotUtils**, so if Visualize changes both projects build.

---

### PacketGen

When files inside:

```
Template.PacketGen/
```

are staged:

1. Patch version in `PacketGen.csproj` is automatically incremented
2. A release build is executed
3. A new `.nupkg` is generated
4. The old package in `Template/Framework/Libraries/` is replaced
5. The new package is staged for the commit

---

## Requirements

| Tool                                | Purpose                      |
| ----------------------------------- | ---------------------------- |
| `.NET SDK`                          | Builds projects and packages |
| `bash`                              | Executes hooks               |
| `grep`, `sed`, `find`, `cp`, `diff` | Script utilities             |

The Visualize project requires **Godot.NET.Sdk**, which must be available to `dotnet build`.

---

## Verifying Hook Setup

Confirm Git is using the repository hooks:

```bash
git config core.hooksPath
```

Expected output:

```
.githooks
```

Verify hooks exist:

```bash
ls .githooks
```

---

## Testing Hooks

You can trigger the pre-commit hook by making a test commit:

```bash
git commit --allow-empty -m "hook test"
```

If builds fail, the commit will be aborted.
