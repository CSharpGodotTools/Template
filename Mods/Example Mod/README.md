# Example Mod (Managed C#)

This example mod uses `Framework.Mods.IModEntrypoint`.

## Build

1. Build from this folder:
   - `dotnet build ExampleMod.csproj -c Release`
2. Copy the built DLL to this mod root as `Mod.dll`:
   - From: `bin/Release/net10.0/Mod.dll`
   - To: `Mods/Example Mod/Mod.dll`

## Build Against a Shipped Game

If the game is shipped and source is unavailable, build with explicit references:

`dotnet build ExampleMod.csproj -c Release -p:TemplateAssemblyPath="C:\Path\To\Game\Template.dll" -p:GodotSharpAssemblyPath="C:\Path\To\Game\GodotSharp.dll"`

## Required Runtime Files

- `mod.json` (required)
- `Mod.dll` (optional, required for managed C# mods)
- `mod.pck` (optional, asset/scene pack)
