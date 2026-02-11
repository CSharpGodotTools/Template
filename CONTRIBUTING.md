### Prerequisites

In order to run the game you will need the latest [Godot C#](https://godotengine.org/), [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and a [Custom Build of ENet](https://github.com/CSharpGodotTools/Template/wiki/Custom-ENet-Builds) if you are a Linux or Mac user.

You will need to setup an IDE, here are setup guides for [Visual Studio](https://github.com/CSharpGodotTools/Template/wiki/Setup-Visual-Studio) (highly recommended) and [Visual Studio Code](https://github.com/CSharpGodotTools/Template/wiki/Configuring-VSCode). While not required, I highly recommend installing [GitHub Desktop App](https://github.com/CSharpGodotTools/Template/wiki/Working-with-GitHub-Desktop) as it greatly simplifies interactions with git.

Clone the repository with `git clone --recurse-submodules https://github.com/CSharpGodotTools/Template` and open the `project.godot` file to open the project.

In Godot > Editor > Editor Settings, search for "external" and make sure `Use External Editor` is enabled and `External Editor` is set to your IDE.

<img width="896" height="317" alt="image" src="https://github.com/user-attachments/assets/4cebcad1-848e-4a70-815b-5769350d5c0d" />
<img width="896" height="316" alt="image" src="https://github.com/user-attachments/assets/d84a5912-4ec5-4f35-999a-5994b96c19f4" />

Open up a script from Godot at least once so Godot has a chance to generate the proper assemblies otherwise you may see some missing types.

### Guidelines
Normally, when setting up a new project using Template, you would run the setup scene, which replaces the root namespace `__TEMPLATE__` in various scripts with the game name you specify. However, since we are contributing directly to Template, we should avoid running this setup.

Please have a brief look at the [Template's Coding Standards](https://github.com/ValksGodotTools/Template/wiki/Code-Style-Document).

Ready to start contributing? Start by looking to see if there are any [issues](https://github.com/CSharpGodotTools/Template/issues) or [pull requests](https://github.com/CSharpGodotTools/Template/pulls).

### Contact

If you have any questions you can send me a message over Discord, my username is `valky5`.

### Resource Links
- [Godot C# Documentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
- [ENet-CSharp Documentation](https://github.com/nxrighthere/ENet-CSharp?tab=readme-ov-file#api-reference)
- [Microsoft Learn - C#](https://learn.microsoft.com/en-us/dotnet/csharp/)
