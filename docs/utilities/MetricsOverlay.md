> [!NOTE]
> Metrics Overlay can be toggled in-game with `F1`

The metrics overlay shows useful information in-game and can be used to monitor the performance of your script variables.

```cs
Game.Metrics.StartMonitoring("My Variable", () => _someVariable);
//Game.Metrics.StopMonitoring("My Variable");
```

<img width="214" height="163" alt="image" src="https://github.com/user-attachments/assets/e8174be2-f7fe-483e-990d-9d7f2fc67eda" />

You can also profile the rest of your codes performance.

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