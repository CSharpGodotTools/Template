<div align="center">
    <img width="1000" height="200" alt="Banner" src="https://github.com/user-attachments/assets/ac90a4be-170c-4294-9266-7e2a698fde63" />
    <a href="https://github.com/ValksGodotTools/Template/stargazers"><img src="https://img.shields.io/github/stars/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="GitHub stars" /></a>
    <a href="https://github.com/ValksGodotTools/Template/network"><img src="https://img.shields.io/github/forks/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="GitHub forks" /></a>
    <a href="https://github.com/ValksGodotTools/Template/commits/main"><img src="https://img.shields.io/github/last-commit/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="Last commit" /></a>
    <a href="https://github.com/ValksGodotTools/Template/graphs/contributors"><img src="https://img.shields.io/github/contributors/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="Contributors" /></a>
    <a href="https://github.com/ValksGodotTools/Template/watchers"><img src="https://img.shields.io/github/watchers/ValksGodotTools/Template?style=flat&labelColor=1a1a1a&color=000000" alt="Watchers" /></a>
    <a href="https://discord.gg/j8HQZZ76r8"><img src="https://img.shields.io/discord/955956101554266132?label=discord&style=flat&color=000000&labelColor=1a1a1a" alt="Discord" /></a>
</div>

This is an on-going project I've been working on and it has evolved into more than just a template. I hope you will find it as useful as I have.

![MainMenuPreview](https://github.com/user-attachments/assets/e2cf3c4f-ea22-4b26-85df-a953383a9b99)  
![OptionsPreview](https://github.com/user-attachments/assets/7d744362-7c70-4f17-9262-c5e77182e942)  

## Overview
- [Multiplayer](#multiplayer)
  - [Why use this?](#why-use-this)
  - [Your first netcode](#your-first-netcode)
  - [Working with packets](#working-with-packets)
  - [Note about Apple ARM](#note-about-apple-arm)
- [Templates](#templates)
  - Minimal 2D scene
  - Minimal 3D scene
  - [FPS scene](#fps-scene)
- [Performance](#performance)
  - [Centralized Component Scripts](#centralized-component-scripts)
- [In-Game Debugging](#in-game-debugging)
  - [Visual Debugging](#visual-debugging)
  - [Metrics Overlay](#metrics-overlay)
  - [Console Commands](#console-commands)
- [Utilities](#utilities)
  - [Cat Lips Source Generators](https://github.com/Cat-Lips/GodotSharp.SourceGenerators)
  - [Improved Debugger Dock](#improved-debugger-dock)
  - [Simplified Tweens](#simplified-tweens)
  - [Services](#services)
  - [Custom Main Run Args](#custom-main-run-args)
  - [GDUnit4 Testing](#gdunit-testing)
- [Prerequisites](#prerequisites)
- [Install](#install)
- [Update](#update)
- [Thank You](#thank-you)

## Multiplayer
### Why use this?
Minimum packet data sent. For example if we send the string "Dog", we have 3 bytes for the data, 1 byte for the packet opcode and 1 byte from ENets overhead.

Packet scripts are kept small. The `Write` and `Read` methods are generated for you.

Tested and works with up to 500 clients sending positions to each other.

### Your First Netcode
Create these packets.

```cs
public partial class CPlayerJoined : ClientPacket
{
    public string Username { get; set; }
}
```

```cs
public partial class SPlayerPositions : ServerPacket
{
    public Dictionary<uint, Vector2> Positions { get; set; }
}
```

Create the game client.

```cs
public partial class GameClient : GodotClient
{
    public GameClient()
    {
        OnPacket<SPlayerPositions>(packet =>
        {
            Log($"Received {packet.Positions.Count} player positions");
        });
    }

    protected override void OnConnected()
    {
        Send(new CPlayerJoined("Valk"));
    }
}
```

Create the game server.

```cs
public partial class GameServer : GodotServer
{
    public Dictionary<uint, Vector2> Players { get; } = new();

    public GameServer()
    {
        OnPacket<CPlayerJoined>(peer =>
        {
            Players[peer.PeerId] = Vector2.Zero;

            Send(new SPlayerPositions(Players), peer.PeerId);
        });
    }

    protected override void OnPeerDisconnected(uint peerId)
    {
        Players.Remove(peerId);
    }
}
```

Start the server and client.

```cs
public partial class World : Node
{
    private const int Port = 25565;
    private const string Ip = "127.0.0.1";

    private Net<GameClient, GameServer> _net = null!;

    public override void _Ready()
    {
        _net = new Net<GameClient, GameServer>();
        _net.StartServer(Port);
        _net.StartClient(Ip, Port);
    }
}
```

### Working with Packets

Packets extend from `ClientPacket` or `ServerPacket`. All packets are prefixed with `C` for client or `S` for server, this is just convention and not required.
```cs
public partial class CPacketPlayerPosition : ClientPacket
{
    public uint Id { get; set; }
    public Vector2 Position { get; set; }

    // Properties can be excluded so their not part of the packet
    [NetExclude]
    public Vector2 PrevPosition { get; set; }
}
```

The following is what is outputted by the source gen for you.
```cs
public partial class CPacketPlayerPosition
{
    public override void Write(PacketWriter writer)
    {
        writer.Write(Id);
        writer.Write(Position);
    }

    public override void Read(PacketReader reader)
    {
        Id = reader.ReadUInt();
        Position = reader.ReadVector2();
    }
}
```

If a type is not supported, you will need to manually override `Write` and `Read`. Manually overriding the methods prevents the source gen from generating anything. You should get a warning in your IDE telling you the type is not supported.

| Type              | Supported | Example Types                       | Additional Notes |
| ----------------- | --------- | ----------------------------------- | -------------------------------------------------------- |
| Primitives        | ✅        | `int`, `bool`, `ulong`              |                                                          |
| Vectors & byte[]  | ✅        | `Vector2`, `Vector3`, `byte[]`      |                                                          |
| Generics          | ✅        | `List<List<int>>`, `Dictionary<string, List<char>>`      |                                     |
| Arrays            | ✅        | `int[]`, `bool[]`                             |                                                |
| Classes & Structs | ✅        | `PlayerData`                        |                                                          |
| Msc               | ❌        | `HashSet`, `PointLight2D`           | These types are too specific and will not be supported.  |

You have full control over the order of which data gets sent when you override `Write` and `Read`.

```cs
// Since we need to use if conditions we actually have to type out the Write and Read functions
public partial class SPacketPlayerJoinLeave : ServerPacket
{
    public uint Id { get; set; }
    public string Username { get; set; }
    public Vector2 Position { get; set; }
    public bool Joined { get; set; }

    public override void Write(PacketWriter writer)
    {
        // Not required to cast explicit types but helps prevent human error
        writer.Write((uint)Id);
        writer.Write((bool)Joined);

        if (Joined)
        {
            writer.Write((string)Username);
            writer.Write((Vector2)Position);
        }
    }

    public override void Read(PacketReader reader)
    {
        Id = reader.ReadUInt();

        Joined = reader.ReadBool();

        if (Joined)
        {
            Username = reader.ReadString();
            Position = reader.ReadVector2();
        }
    }
}
```

By default you can only have up to `256` different client packet classes and `256` different server packets for a total of `512` different packet classes. 

If you need more space, you have 2 options.

1. (Recommended) Add a `OpcodeEnum Opcode { get; set; }` property to any of your packet classes with a conditional if-else chain kind of like what you see above but instead of a bool it is a enum. Replace `OpcodeEnum` with your own enum. (All enums are converted to bytes so you cannot have more than `256` options in any enum you send over the network! _This may be changed in the future._)  
2. In `PacketRegistry.cs`, change `byte` to `ushort` but note that now all your packets will have a 2 byte overhead instead of just 1 byte. Note that `ushort` has a size of `65,535` compared to `byte` that only has `255`.

```cs
// res://Framework/Netcode/Packet/PacketRegistry.cs
[PacketRegistry(typeof(byte))]
public partial class PacketRegistry
{
}
```

### Note about Apple ARM

If you are running on an OS without a build for your platform (such as Apple ARM), you
may need to provide your own build of `ENet-CSharp`. To do so, follow the build instructions
[here](https://github.com/nxrighthere/ENet-CSharp), and place the resulting `ENet-CSharp.dll`
and `.so` or `.dylib` in the games root directory.

## Templates

### FPS Scene

<image src="https://github.com/user-attachments/assets/02bd2f01-e5f7-4856-93d8-d12471589c22" />

## Performance

### Centralized Component Scripts

There is a very noticeable gap in performance when we have 10,000 nodes all with their own `_Process` functions. If we use a centralized component manager that handles all processes we notice a [3.5x increase in performance](https://www.reddit.com/r/godot/comments/1mdrjce/you_can_save_a_lot_of_fps_by_centralizing_your/) (This only applies to C# users)

<details>
<summary>Component design typically looks like this.</summary>

```cs
public partial class Player : Node
{
    private EntityMovementComponent _movement;

    public override void _Ready()
    {
        _movement = new EntityMovementComponent(this);
    }

    public override void _Process(double delta)
    {
        _movement.Process(delta);
    }

    public override void _ExitTree()
    {
        _movement.Dispose();
    }
}
```

</details>

But we can do better! Let's extend from `Component`.

```cs
public class EntityMovementComponent(Player player) : Component(player)
{
    // Notice these methods do not start with an underscore
    protected override void Ready()
    {
        // Process is disabled by default and we must enable it ourselves
        SetProcess(true);
    }

    protected override void Process(double delta)
    {
        // Handle process...
    }

    protected override void ExitTree()
    {
        // Handle exit tree...
    }
}
```

Now the `Player` script looks much cleaner.

```cs
public partial class Player : Node
{
    // Optionally add components to a ComponentList to hide unused warnings
    private EntityMovementComponent _movement;

    public override void _Ready()
    {
        _movement = new EntityMovementComponent(this);
    }
}
```

## In Game Debugging

### Visual Debugging

Visual in-game debugging!

https://github.com/user-attachments/assets/1fe282b9-f044-42cd-b9be-0e26f20b1aa6

https://github.com/user-attachments/assets/2f44ae8e-0c99-4bd2-b15f-a72a70ffaa74

#### Example Usage 1
```cs
// Works in both 2D and 3D scenes
public partial class Player : CharacterBody3D
{
    [Visualize]
    private int _x;

    public override void _Ready()
    {
        Visualize.Register(this);
    }

    [Visualize]
    public void IncrementX(int amount)
    {
        _x += amount;
        Visualize.Log(amount);
    }
}
```

#### Example Usage 2
```cs
// Scripts do not have to extend from Node for Visualize to work
public class PlayerMovementComponent
{
    [Visualize]
    private float _y;

    public PlayerMovementComponent(Player player)
    {
        Visualize.Register(this, player);
    }
}
```

#### Supported Members

| Member Type       | Supported  | Example Types                                 | Additional Notes                                                      |
|-------------------|------------|-----------------------------------------------|-----------------------------------------------------------------------|
| **Numericals**    | ✅         | `int`, `float`, `double`                      | All numerical types are supported                                     |
| **Enums**         | ✅         | `Direction`, `Colors`                         | All enum types are supported                                          |
| **Booleans**      | ✅         | `bool`                                        |                                                                       |
| **Strings**       | ✅         | `string`                                      |                                                                       |
| **Color**         | ✅         | `Color`                                       |                                                                       |
| **Vectors**       | ✅         | `Vector2`, `Vector2I`, `Vector3`, `Vector3I`, `Vector4`, `Vector4I` |                                                 |
| **Quaternion**    | ✅         | `Quaternion`                                  |                                                                       |
| **NodePath**      | ✅         | `NodePath`                                    |                                                                       |
| **StringName**    | ✅         | `StringName`                                  |                                                                       |
| **Methods**       | ✅         |                                               | Method parameters support all listed types here                       |
| **Static Members**| ✅         |                                               | This includes static methods, fields, and properties                  |
| **Arrays**        | ✅         | `int[]`, `string[]`, `Vector2[]`              | Arrays support all listed types here                                  |
| **Lists**         | ✅         | `List<string[]>`, `List<Vector2>`             | Lists support all listed types here                                   |
| **Dictionaries**  | ✅         | `Dictionary<List<Color[]>, Vector2>`          | Dictionaries support all listed types here                            |
| **Structs**       | ✅         | `struct`                                      |                                                                       |
| **Classes**       | ✅         | `class`                                       |                                                                       |
| **Resources**     | ✅         | `Resource`                                    |                                                                       |
| **Godot Array**   | ✅         | `Godot.Collections.Array<int>`                | Both generic and non-generic types are supported.                     |
| **Godot Dictionary** | ✅      | `Godot.Collections.Dictionary<int, bool>`     | Both generic and non-generic types are supported.                     |
| **Godot Classes** | ❌         | `PointLight2D`, `CharacterBody3D`             |                                                                       |

> [!NOTE]
> No mouse interactions have been implemented in 3D, so you will only be able to use it for read only.

### Metrics Overlay

The metrics overlay can be toggled in-game with `F1`. All monitored variables or profiled code will appear under the 'Variables' section.

<img width="293" height="311" alt="image" src="https://github.com/user-attachments/assets/02c77eef-7295-4bf0-856f-d0d32e0993ed" />

## Monitor Variables
Track variables in your code.

```cs
Game.Metrics.StartMonitoring("My Variable", () => _someVariable);

// Specifiying a name is optional
Game.Metrics.StartMonitoring(() => _someVariable);
```

## Profile Code
Log the running time of your code.

```cs
// _Ready
Game.Profiler.Start("Player Init");
PlayerSetup();
Game.Profiler.Stop("Player Init"); // The running time will be printed to the console
```

```cs
// _Process
Game.Profiler.StartProcess("Player Firing Logic"); 
PlayerFire();
Game.Profiler.StopProcess("Player Firing Logic"); // The running time will be displayed in the Metrics Overlay (F1)
```

### Console Commands

> [!NOTE]
> The in-game console can be brought up with `F12`

Registering new commands is easy.
```cs
public override void _Ready()
{
    Game.Console.RegisterCommand("help",  CommandHelp);
    Game.Console.RegisterCommand("quit",  CommandQuit).WithAliases("exit");
    Game.Console.RegisterCommand("debug", CommandDebug);
}

private void CommandHelp(string[] args)
{
    IEnumerable<string> cmds = Game.Console.GetCommands().Select(x => x.Name);
    Logger.Log(cmds.ToFormattedString());
}

private async void CommandQuit(string[] args)
{
    await Global.Instance.ExitGame();
}

private void CommandDebug(string[] args)
{
    if (args.Length <= 0)
    {
        Logger.Log("Specify at least one argument");
        return;
    }

    Logger.Log(args[0]);
}
```

<img width="344" height="330" alt="image" src="https://github.com/user-attachments/assets/d5ccf33f-316a-44ca-9950-8898a6ee14e3" />

## Utilities

### Improved Debugger Dock
This is just like the built-in Godot debugger dock except with more features.
- Fully customizable
- Double click to open any entry in VSCode
- Hide methods from stack trace with `[StackTraceHidden]` attribute

<img width="1301" height="318" alt="image" src="https://github.com/user-attachments/assets/4283793b-134d-4731-b226-f4a69f7dc201" />

<img width="501" height="434" alt="image" src="https://github.com/user-attachments/assets/b9037052-08bc-4864-a00b-66d64387d6eb" />

### Dev Tools Dock
Contains useful utilities to clean stale uids, empty folders and so on.

#### Dev
<img width="1083" height="153" alt="image" src="https://github.com/user-attachments/assets/cb28d5ee-b9b6-46b5-b89e-b27fd0f8c1e7" />

#### Visual
<img width="1301" height="193" alt="image" src="https://github.com/user-attachments/assets/f1372a99-f878-48d6-b729-88f6ea979351" />

### Simplified Tweens

```cs
// Node tween
Tweens.Animate(colorRect)
    .Position(new Vector(100, 300), 2.5);

// Node2D tween
Tweens.Animate(playerSprite)
    .Position(new Vector2(100, 300), 2.5)
    .Property("position", Vector2.Zero, 1.0);

// Control tween with parallel
Tweens.Animate(colorRect)
    .Parallel().Scale(Vector2.One * 2, 2)
    .Parallel().Color(Colors.Green, 2)
    .Rotation(Mathf.Pi, 2)
    .Then(() => GD.Print("Finished!"));

// Tween specific properties
Tween.Animate(colorRect, "color")
    .PropertyTo(Colors.Red,   0.5).TransExpo().EaseIn()
    .PropertyTo(Colors.Green, 0.5).TransQuad().EaseOut()
    .PropertyTo(Colors.Blue,  0.5).TransSpring().EaseInOut()
    .Loop();
```

> [!TIP]
> Prefer strongly typed names over strings? Instead of typing for example `"scale"` do `Control.PropertyName.Scale`

### Services

> [!IMPORTANT]
> The service attribute is only valid on scripts that extend from Node and the node must be in the scene tree.

```cs
// Services assume there will only ever be one instance of this script.
// All services get cleaned up on scene change. 
public partial class Player : Node
{
    // Use _EnterTree() if the service is not being registered soon enough
    public override void _Ready()
    {
        Services.Register(this);
    }
}
```

```cs
// Get the service from anywhere in your code
Player player = Services.Get<Player>();
```

### Custom Main Run Args

In Godot top left `Debug > Customize Run Instances...` there are custom defined arguments you can use.

<image src="https://github.com/user-attachments/assets/1d8802ad-7825-466a-8887-23a2a3d9069a" />

<image src="https://github.com/user-attachments/assets/99714e1f-f76d-42f9-8af2-fb2503de17ec" />

### Window Position and Size Args
Current arguments are `top_left`, `top_right`, `bottom_left`, `bottom_right`, `middle_left`, `middle_right`, `middle_top`, `middle_bottom`.

These arguments will change the position and size of each instance. This is especially helpful when debugging multiplayer.

### GDUnit Testing

#### Why should I use tests?

Tests make sure your code does what it is suppose to do. Write tests first, then and only then add more features when all your tests pass. Learn more [here](https://github.com/godot-gdunit-labs/gdUnit4).

<details>
<summary>Create .runsettings in root of project.</summary>

Replace `PATH\TO\GODOT_EXE` with the path to your Godot executable.

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
    <RunConfiguration>
        <MaxCpuCount>1</MaxCpuCount>
        <TestAdaptersPaths>.</TestAdaptersPaths>
        <ResultsDirectory>./TestResults</ResultsDirectory>
        <TestSessionTimeout>1800000</TestSessionTimeout>
        <TreatNoTestsAsError>true</TreatNoTestsAsError>
        
        <!-- Environment variables available to tests -->
        <EnvironmentVariables>
            <GODOT_BIN>PATH\TO\GODOT_EXE</GODOT_BIN>
        </EnvironmentVariables>
    </RunConfiguration>

    <!-- Test result loggers -->
    <LoggerRunSettings>
        <Loggers>
            <Logger friendlyName="console" enabled="True">
                <Configuration>
                    <Verbosity>normal</Verbosity>
                </Configuration>
            </Logger>
            <Logger friendlyName="html" enabled="True">
                <Configuration>
                    <LogFileName>test-result.html</LogFileName>
                </Configuration>
            </Logger>
            <Logger friendlyName="trx" enabled="True">
                <Configuration>
                    <LogFileName>test-result.trx</LogFileName>
                </Configuration>
            </Logger>
        </Loggers>
    </LoggerRunSettings>

    <!-- GdUnit4-specific configuration -->
    <GdUnit4>
        <!-- Additional Godot runtime parameters -->
        <Parameters>--verbose</Parameters>
        
        <!-- Test display name format: SimpleName or FullyQualifiedName -->
        <DisplayName>FullyQualifiedName</DisplayName>
        
        <!-- Capture stdout from test cases -->
        <CaptureStdOut>true</CaptureStdOut>
        
        <!-- Compilation timeout for large projects (milliseconds) -->
        <CompileProcessTimeout>20000</CompileProcessTimeout>
    </GdUnit4>
</RunSettings>
```

</details>

<details>
<summary>Visual Studio</summary>

1. In Visual Studio, go to `Test > Configure Run Settings` and browse to and select the `.runsettings` file.
2. Restart Visual Studio.
3. Click on `Test > Test Explorer` and you should be able to run all tests.

<img width="846" height="200" alt="image" src="https://github.com/user-attachments/assets/b78d4e1c-e0d7-4c8a-b769-c5c73bcca798" />
    
</details>

<details>
<summary>VSCode</summary>

1. In VSCode, go to `Extensions` and search for `C# Dev Kit by Microsoft` (do not install this just yet)
2. On the `C# Dev Kit` extension page, click on the gear icon to the right and click on `Download Specific Version VSIX` and select `1.5.12`
3. Move `ms-dotnettools.csdevkit-1.5.12-win32-x64.vsix` to the root of the project
4. Run `code --install-extension ms-dotnettools.csdevkit-1.5.12-win32-x64.vsix --force` from within VSCode terminal
5. Delete `ms-dotnettools.csdevkit-1.5.12-win32-x64.vsix`
6. Restart VSCode
7. Click on the `Testing` tab on left and you should be able to run all tests.

<img width="413" height="245" alt="image" src="https://github.com/user-attachments/assets/758e8f86-f440-42dd-89b8-7479489d9b90" />

</details>

> [!NOTE]
> Running `dotnet test` requires the Godot executable path to be in an environment variable named `GODOT_BIN`.

#### Testing
```cs
using GdUnit4;
using static GdUnit4.Assertions;

namespace Template.Setup.Testing;

[TestSuite]
public class Tests
{
    [TestCase]
    public void StringToLower()
    {
        AssertString("AbcD".ToLower()).IsEqual("abcd");
    }
}
```

## Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Latest Godot C# Release](https://godotengine.org/)

## Install  
Download the [latest release](https://github.com/CSharpGodotTools/Template/releases/latest) and open `project.godot`. Click `Run Setup` to restart Godot with your template ready to go.

<img width="411" height="313" alt="image" src="https://github.com/user-attachments/assets/d5c663a9-29c1-40a0-b851-17f9e5cb7f00" />

## Update

Updates can be done from `Dev Tools > Update`.

<img width="1498" height="334" alt="image" src="https://github.com/user-attachments/assets/b3eef36d-57f3-4a15-954d-d65c59ddfa57" />

## Thank You
[Brian Shao](https://github.com/cydq)  
[Cat Lips](https://github.com/Cat-Lips)  
[Onryigit](https://github.com/onryigit)  
[Piep Matz](https://github.com/riffy)  
[Vaggelismsxp](https://github.com/vaggelismsxp)  
