## What to work on?
See if there are any [good first issues](https://github.com/CSharpGodotTools/Template/issues?q=is%3Aissue%20state%3Aopen%20label%3A%22good%20first%20issue%22), these issues will always be beginner friendly. You can also have a look at the [discussions](https://github.com/CSharpGodotTools/Template/discussions).

Not a developer but want to suggest a new feature? Tell me all about it in a new issue [here](https://github.com/CSharpGodotTools/Template/issues) or tell me over Discord, my username is `valky5`.

## Low file size
This project should be as lightweight as possible so it will not include any game assets. For example an FPS scene with 4K textures is too large and as such should be developed in a separate repository. Such repositories if any exist will be mentioned in the projects [README.md](https://github.com/CSharpGodotTools/Template/blob/main/README.md).

## Setting up git hooks
While not required, setting up git hooks will make development much easier.

Currently git hooks do the following:
```
1. Sync root .editorconfig to all sub project .editorconfig's
2. Auto sync sub project changes with Template
3. Verify everything still works with dotnet build
```

Setup git hooks with `git config core.hooksPath .githooks` (and `chmod +x .githooks/*` if you are on Linux or Mac)
