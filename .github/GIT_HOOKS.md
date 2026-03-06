# Git Hooks Setup

All hooks live in the version-controlled `.githooks/` directory. Tell git to use them instead of the default `.git/hooks/` directory by running the one-time setup command below, then follow any platform-specific notes.

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

Git Bash ships with `bash`, `grep`, `sed`, `find`, and `diff`, so all hooks work without any extra tools. Executable bits are not meaningful on NTFS, so `chmod` is optional.

> **PowerShell / CMD**: The hooks are bash scripts and require a POSIX-compatible shell. Use Git Bash, WSL, or any other bash installation — the hooks do not have native PowerShell equivalents.

---

## What Each Hook Does

| Hook | Trigger | Action |
|---|---|---|
| `pre-commit` | Any commit that stages `.editorconfig` | Copies root `.editorconfig` to every subproject root and stages the copies automatically so they land in the same commit. |
| `post-commit` | After any commit | **GodotUtils** — builds and copies `GodotUtils.dll` + `GodotUtils.xml` to `Template/Framework/Libraries/` when files under `Template.GodotUtils/` were committed. **Visualize** — same for `Visualize.dll` + `Visualize.xml` when files under `Template.Visualize/` were committed. **PacketGen** — builds a release nupkg, replaces the old `PacketGen.*.nupkg` in `Template/Framework/Libraries/`, then bumps the patch version in `PacketGen.csproj` for the next release when files under `Template.PacketGen/` were committed. |
| `post-checkout` | Switching branches | Syncs root `.editorconfig` to all subproject roots when the files differ, so the correct config is always in place after a checkout. |
| `post-merge` | After `git merge` / `git pull` | Syncs root `.editorconfig` to all subproject roots when `.editorconfig` was part of the merged changes. |

---

## Requirements

| Tool | Used by |
|---|---|
| `dotnet` (.NET 10 SDK) | `post-commit` (all three builds) |
| `bash` 4+ | All hooks |
| Standard POSIX utils — `grep`, `sed`, `find`, `diff`, `cp` | All hooks |

The Godot SDK (`Godot.NET.Sdk`) required by the Visualize build must also be resolvable by `dotnet build`. This is automatically satisfied when Godot is installed with the Mono / .NET module.

---

## Verifying the Setup

```bash
git config core.hooksPath   # should print: .githooks
ls -l .githooks/            # should show the four hook scripts as executable
```

To test the `pre-commit` hook manually without making a real commit:

```bash
git stash                   # ensure a clean state first
git hook run pre-commit     # requires git 2.36+, or just make a test commit
```
