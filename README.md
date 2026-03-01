<div align="center">
    <img width="1000" height="200" alt="Banner" src="https://github.com/user-attachments/assets/ac90a4be-170c-4294-9266-7e2a698fde63" />
    <a href="https://github.com/ValksGodotTools/Template/stargazers"><img src="https://img.shields.io/github/stars/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="GitHub stars" /></a>
    <a href="https://github.com/ValksGodotTools/Template/network"><img src="https://img.shields.io/github/forks/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="GitHub forks" /></a>
    <a href="https://github.com/ValksGodotTools/Template/commits/main"><img src="https://img.shields.io/github/last-commit/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="Last commit" /></a>
    <a href="https://github.com/ValksGodotTools/Template/graphs/contributors"><img src="https://img.shields.io/github/contributors/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="Contributors" /></a>
    <a href="https://github.com/ValksGodotTools/Template/watchers"><img src="https://img.shields.io/github/watchers/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="Watchers" /></a>
    <a href="https://discord.gg/j8HQZZ76r8"><img src="https://img.shields.io/discord/955956101554266132?label=discord&style=flat&color=000000&labelColor=1a1a1a" alt="Discord" /></a>
</div>

This is an on-going project I've been working on. I know how hard it is to actually finish a game and I hope by making this, game development will be just a little bit easier.

![MainMenuPreview](https://github.com/user-attachments/assets/e2cf3c4f-ea22-4b26-85df-a953383a9b99)  
![OptionsPreview](https://github.com/user-attachments/assets/7d744362-7c70-4f17-9262-c5e77182e942)  

## Features
- Multiplayer
  - [Minimal setup required](docs/networking/Multiplayer.md)
  - [Minimal packet data sent over the network](docs/networking/Multiplayer.md#minimal-packet-data-sent)
  - [Packets automatically create their `Write`, `Read` methods for you](docs/networking/Packets.md)
- Templates
  - Minimal 2D scene
  - Minimal 3D scene
  - [FPS scene](https://github.com/CSharpGodotTools/Template/wiki/FPS-Template)
- Performance
  - [Centralized Component Scripts](docs/utilities/Component-Scripts.md)
- Debugging
  - [Visual In-Game Debugging](https://github.com/CSharpGodotTools/Template/blob/main/Template.Visualize/README.md)
  - [ImGui Metrics Overlay](docs/utilities/Metrics-Overlay.md)
  - [Console Commands](docs/utilities/Console-Commands.md)
  - [Custom Main Run Args](docs/utilities/Custom-Main-Run-Args.md)
  - [GDUnit4 Testing](docs/utilities/GDUnit-Testing.md)
- Utilities
  - [Cat Lips Source Generators](https://github.com/Cat-Lips/GodotSharp.SourceGenerators)
  - [Simplified Tweens](docs/utilities/Simplified-Tweens.md)
  - [Extensions](https://github.com/CSharpGodotTools/Template/wiki/Extensions)
  - [Services](docs/utilities/Services.md)
 
## Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Latest Godot C# Release](https://godotengine.org/)

## Install  
Download the [latest release](https://github.com/CSharpGodotTools/Template/releases/latest) and open `project.godot`.

<img width="501" height="344" alt="image" src="https://github.com/user-attachments/assets/9a0e7e22-2bfa-4cdc-8b5a-d92d209242b8" />

Click `Run Setup` to restart Godot with your template ready to go.

## Updates
Delete `Framework` and `addons/imgui-godot` folders and initialize them as submodules to fetch updates.
```
git init && git submodule add https://github.com/CSharpGodotTools/Framework Framework && git submodule add https://github.com/CSharpGodotTools/imgui-godot-csharp addons/imgui-godot
```

## Thank You
[Brian Shao](https://github.com/cydq)  
[Cat Lips](https://github.com/Cat-Lips)  
[Onryigit](https://github.com/onryigit)  
[Piep Matz](https://github.com/riffy)  
[Vaggelismsxp](https://github.com/vaggelismsxp)  
