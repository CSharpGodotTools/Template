<div align="center">
    <img width="1000" height="200" alt="Banner" src="https://github.com/user-attachments/assets/ac90a4be-170c-4294-9266-7e2a698fde63" />
    <a href="https://github.com/ValksGodotTools/Template/stargazers"><img src="https://img.shields.io/github/stars/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="GitHub stars" /></a>
    <a href="https://github.com/ValksGodotTools/Template/network"><img src="https://img.shields.io/github/forks/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="GitHub forks" /></a>
    <a href="https://github.com/ValksGodotTools/Template/blob/main/LICENSE"><img src="https://img.shields.io/github/license/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="License" /></a>
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
  - [Minimal setup required](https://github.com/CSharpGodotTools/Template/wiki/Multiplayer)
  - [Minimal packet data sent over the network](https://github.com/CSharpGodotTools/Template/wiki/Multiplayer#minimal-packet-data-sent)
  - [Packets automatically create their `Write`, `Read` methods for you](https://github.com/CSharpGodotTools/Template/wiki/Packets)
- Templates
  - Minimal 2D scene
  - Minimal 3D scene
  - [FPS scene](https://github.com/CSharpGodotTools/Template/wiki/FPS-Template)
- Performance
  - [Centralized Component Scripts](https://github.com/CSharpGodotTools/Template/wiki/Component-Scripts)
- Debugging
  - [Visual In-Game Debugging](https://github.com/CSharpGodotTools/Template/wiki/Visualize)
  - [ImGui Metrics Overlay](https://github.com/CSharpGodotTools/Template/wiki/Metrics-Overlay)
  - [Console Commands](https://github.com/CSharpGodotTools/Template/wiki/Console-Commands)
  - [Custom Main Run Args](https://github.com/CSharpGodotTools/Template/wiki/Custom-Main-Run-Args)
  - [GDUnit4 Testing](https://github.com/CSharpGodotTools/Template/wiki/GDUnit-Testing)
- Utilities
  - [Cat Lips Source Generators](https://github.com/CSharpGodotTools/Template/wiki/Cat-Lips-Source-Generators)
  - [Simplified Tweens](https://github.com/CSharpGodotTools/Template/wiki/Simplified-Tweens)
  - [Extensions](https://github.com/CSharpGodotTools/Template/wiki/Extensions)
  - [Services](https://github.com/CSharpGodotTools/Template/wiki/Services)
 
## Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Latest Godot C# Release](https://godotengine.org/)
- [Custom ENet build](https://github.com/CSharpGodotTools/Template/wiki/Custom-ENet-Builds) may be required if using Mac or Linux

## Install  
Download the [latest release](https://github.com/CSharpGodotTools/Template/releases/latest) and open `project.godot`.

> [!TIP]
> After extracting the `.zip`, delete the `Framework` and `addons/imgui-godot` folders and run the following command.
> ```
> git init && git submodule add https://github.com/CSharpGodotTools/Framework Framework && git submodule add https://github.com/CSharpGodotTools/imgui-godot-csharp addons/imgui-godot
> ```
> This will allow you to fetch updates without having to re-clone.

<img width="602" height="212" alt="image" src="https://github.com/user-attachments/assets/8411cb31-7a15-4f16-a265-0ee28412f052" />

Ignore the warning about an addon failing to load and build the game with
<img width="27" height="25" alt="image" src="https://github.com/user-attachments/assets/ecb5dba4-22d7-4bef-8e86-726908390318" /> and then enable the addon `Setup Plugin` in `Project > Project Settings > Plugins`

<img width="501" height="344" alt="image" src="https://github.com/user-attachments/assets/9a0e7e22-2bfa-4cdc-8b5a-d92d209242b8" />

Click `Run Setup` to restart Godot with your template ready to go.

## Thank You
[Brian Shao](https://github.com/cydq)  
[Cat Lips](https://github.com/Cat-Lips)  
[Onryigit](https://github.com/onryigit)  
[Piep Matz](https://github.com/riffy)  
[Vaggelismsxp](https://github.com/vaggelismsxp)  
