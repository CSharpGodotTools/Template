# Git Hooks Setup

All hooks live in the `.githooks/` directory. Configure Git to use them instead of `.git/hooks/` by running the one-time setup command below.

### Linux / macOS

```bash
git config core.hooksPath .githooks
chmod +x .githooks/*
```

### Windows

```bash
git config core.hooksPath .githooks
```
