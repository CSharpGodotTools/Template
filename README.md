<div align="center">
    <img src="https://github.com/user-attachments/assets/46eb7938-3a35-4fd0-a7fd-4c045696ee6a" alt="github_banner" />
    <a href="https://github.com/ValksGodotTools/Template/stargazers"><img src="https://img.shields.io/github/stars/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF" alt="GitHub stars" /></a>
    <a href="https://github.com/ValksGodotTools/Template/network"><img src="https://img.shields.io/github/forks/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF" alt="GitHub forks" /></a>
    <a href="https://github.com/ValksGodotTools/Template/blob/main/LICENSE"><img src="https://img.shields.io/github/license/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF" alt="License" /></a>
    <a href="https://github.com/ValksGodotTools/Template/commits/main"><img src="https://img.shields.io/github/last-commit/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF" alt="Last commit" /></a>
    <a href="https://github.com/ValksGodotTools/Template/graphs/contributors"><img src="https://img.shields.io/github/contributors/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF" alt="Contributors" /></a>
    <a href="https://github.com/ValksGodotTools/Template/watchers"><img src="https://img.shields.io/github/watchers/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=3399FF" alt="Watchers" /></a>
    <a href="https://discord.gg/j8HQZZ76r8"><img src="https://img.shields.io/discord/955956101554266132?label=discord&style=flat&color=3399FF&labelColor=1a1a1a" alt="Discord" /></a>
</div>

----

**Highlights**
- **[Multiplayer](https://github.com/CSharpGodotTools/Template/wiki/Multiplayer)** - Send minimal packet data with ENet.
- **[In-Game Debugging Tools](https://github.com/CSharpGodotTools/Template/wiki/In%E2%80%90Game-Debugging)**
- **[Menu UI](https://github.com/CSharpGodotTools/Template/wiki/Menu-UI)** - Main menu, options and credits.
- **[Simplified Tweens](https://github.com/CSharpGodotTools/Template/wiki/Simplified-Tweens)**
- **[Service Attribute](https://github.com/CSharpGodotTools/Template/wiki/Services)** - Alternative way of managing static members.
- **[Cat Lips Source Generators](https://github.com/CSharpGodotTools/Template/wiki/Source-Generators)**
- **[Useful Extensions](https://github.com/CSharpGodotTools/Template/wiki/Extensions)**

**WIP**
- **[3D FPS Scene](https://github.com/CSharpGodotTools/Template/wiki/3D-FPS)** - Minimal first-person shooter scene with character controller and pre-made weapon animations.
- **[2D Top Down Scene](https://github.com/CSharpGodotTools/Template/wiki/2D-Top-Down)** - A dungeon scene with enemies, room transitions and working multiplayer.
- **[Inventory](https://github.com/CSharpGodotTools/Template/wiki/Inventory)** - A WIP re-creation of my old inventory system.
- **[Draggable Nodes](https://github.com/CSharpGodotTools/Template/wiki/Draggable-Nodes)** - Make any node draggable.
- **[State Manager](https://github.com/CSharpGodotTools/Template/wiki/State-Manager)** - Implement states using delegates.
- **[Mod Loader](https://github.com/CSharpGodotTools/Template/wiki/Mod-Loader)** - Half-working mod loader.

<div align="center">
    <h2>Download</h2>
</div>

First make sure you have the following.
- [.NET SDK](https://dotnet.microsoft.com/download) is at least `8.0.400`. Check with `dotnet --version`.
- [Latest Godot C# Release](https://godotengine.org/)
- [Custom ENet build](https://github.com/CSharpGodotTools/Template/wiki/Custom-ENet-Builds) may be required if using Mac or Linux

Clone this repository with the required submodules.
```bash
git clone https://github.com/CSharpGodotTools/Template && cd Template && git submodule update --init addons/Framework addons/imgui-godot
```

The following is optional, if you choose not to download any genre specific template, you will only be able to use the `No Genre` genre.
```bash
git submodule update --init "Genres/<genre>" # Replace "<genre>" with "2D Top Down" or "3D FPS" without the ""
```

<div align="center">
    <h2>Setup</h2>
</div>

Run the main scene, fill in the fields, click apply changes and you are done. [Link to FAQ](https://github.com/CSharpGodotTools/Template/wiki/FAQ)  

<img width="1541" height="953" alt="Setup" src="https://github.com/user-attachments/assets/79627179-7619-4e70-adf7-2983a8bfbea3" />
<img width="1553" height="966" alt="TopDown" src="https://github.com/user-attachments/assets/b3143d7a-8df7-4a38-b1ff-36a41360cab9" />
<img width="1564" height="880" alt="FPS" src="https://github.com/user-attachments/assets/37bd5c18-cbfb-4dda-824b-e69112343c04" />

<div align="center">
    <h2>Thank You</h2>
</div>

[Brian Shao](https://github.com/cydq) for helping with cross-platform compatibility.  
[Piep Matz](https://github.com/riffy) for helping with the drag drop system.  

New contributors please read [this guide](https://github.com/CSharpGodotTools/Template/wiki/Contributing).
