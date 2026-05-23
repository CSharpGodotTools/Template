> [!IMPORTANT]
> If setting up this repo for the first time you will need to run `dotnet build` from root so `Template/Framework/Libraries` populates with all the required nuget packages. **You only need to do this once.**

> [!NOTE]
> This project should be as lightweight as possible so it will not include any game assets. For example an FPS scene with 4K textures is too large and as such should be developed in a separate repository. Such repositories if any exist will be mentioned in the projects [README.md](https://github.com/CSharpGodotTools/Template/blob/main/README.md).

> [!TIP]
> `.editorconfig` and `.vscode/settings.json` are duplicated in root and `Template/Template`. We can use git hooks to make maintaining these a bit easier. Setup git hooks with `git config core.hooksPath .githooks` _(or `chmod +x .githooks/*` if you are on Linux or Mac)_. Only edit files from root and creating a commit will auto copy over the changes to the subfolder for you.

### Support

- If you want to talk to me directly, my discord username is `valky5`
- If you want to chat publicly, join https://discord.gg/j8HQZZ76r8
- If your message is more technical, consider opening up an issue in this repo
