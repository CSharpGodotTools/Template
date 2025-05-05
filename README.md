![github_banner](https://github.com/user-attachments/assets/46eb7938-3a35-4fd0-a7fd-4c045696ee6a)

[![GitHub stars](https://img.shields.io/github/stars/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF)](https://github.com/ValksGodotTools/Template/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF)](https://github.com/ValksGodotTools/Template/network)
[![License](https://img.shields.io/github/license/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF)](https://github.com/ValksGodotTools/Template/blob/main/LICENSE)
[![GitHub last commit](https://img.shields.io/github/last-commit/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF)](https://github.com/ValksGodotTools/Template/commits/main)
[![Contributors](https://img.shields.io/github/contributors/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF)](https://github.com/ValksGodotTools/Template/graphs/contributors)
[![GitHub watchers](https://img.shields.io/github/watchers/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF)](https://github.com/ValksGodotTools/Template/watchers)
[![Discord](https://img.shields.io/discord/955956101554266132?label=discord&style=flat&color=3399FF&labelColor=1a1a1a)](https://discord.gg/j8HQZZ76r8)

## Requirements
- [.NET SDK](https://dotnet.microsoft.com/download) is at least `8.0.400`. Check with `dotnet --version`.
- [Latest Godot C# Release](https://godotengine.org/)
- [Custom ENet build](https://github.com/CSharpGodotTools/Template/wiki/Custom-ENet-Builds) may be required if using Mac or Linux

## Setup
Clone Template with required submodules.
```bash
git clone https://github.com/CSharpGodotTools/Template && cd Template && git submodule update --init Template/GodotUtils addons/imgui-godot
```

Optionally download genres.
```bash
git submodule update --init "Genres/2D Top Down"
git submodule update --init "Genres/3D FPS"
git submodule update --init "Genres/2D Platformer"
```

Run the main scene and fill in the fields.

![Setup](https://github.com/user-attachments/assets/c924041f-b4d9-48bc-89ae-f7be01305f3e)
![Preview](https://github.com/user-attachments/assets/1d3eb4ee-eb60-49d2-96e8-fb132e02fb6b)

[Link to FAQ](https://github.com/CSharpGodotTools/Template/wiki/FAQ)  

## Features
**Hot Features**
- **[Multiplayer](https://github.com/CSharpGodotTools/Template/wiki/Multiplayer)** - Send minimal packet data with ENet.
- **[In-Game Debugging Tools](https://github.com/CSharpGodotTools/Template/wiki/In%E2%80%90Game-Debugging)**
- **[Menu UI](https://github.com/CSharpGodotTools/Template/wiki/Menu-UI)** - Main menu, options and credits.
- **[Simplified Tweens](https://github.com/CSharpGodotTools/Template/wiki/Simplified-Tweens)**
- **[Service Attribute](https://github.com/CSharpGodotTools/Template/wiki/Services)** - Alternative way of managing static members.
- **[Cat Lips Source Generators](https://github.com/CSharpGodotTools/Template/wiki/Source-Generators)**
- **[Useful Extensions](https://github.com/CSharpGodotTools/Template/wiki/Extensions)**

**WIP Features**
- **[3D FPS Scene](https://github.com/CSharpGodotTools/Template/wiki/3D-FPS)** - Minimal first-person shooter scene with character controller and pre-made weapon animations.
- **[2D Top Down Scene](https://github.com/CSharpGodotTools/Template/wiki/2D-Top-Down)** - A dungeon scene with enemies, room transitions and working multiplayer.
- **[Inventory](https://github.com/CSharpGodotTools/Template/wiki/Inventory)** - A WIP re-creation of my old inventory system.
- **[Draggable Nodes](https://github.com/CSharpGodotTools/Template/wiki/Draggable-Nodes)** - Make any node draggable.
- **[State Manager](https://github.com/CSharpGodotTools/Template/wiki/State-Manager)** - Implement states using delegates.
- **[Mod Loader](https://github.com/CSharpGodotTools/Template/wiki/Mod-Loader)** - Half-working mod loader.

## Contributing

Want to contribute? Start by looking [here](https://github.com/CSharpGodotTools/Template/wiki/Contributing)!

Thank you [Brian Shao](https://github.com/cydq) for helping with cross-platform compatibility.
