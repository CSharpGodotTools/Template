> [!NOTE]
> Metrics Overlay can be toggled in-game with `F1`

The metrics overlay shows useful information in-game and can be used to monitor the performance of your script variables.

```cs
Game.Metrics.StartMonitoring(() => _someVariable);
Game.Metrics.StartMonitoring("My Variable", () => _someVariable);
```

<img width="293" height="311" alt="image" src="https://github.com/user-attachments/assets/02c77eef-7295-4bf0-856f-d0d32e0993ed" />

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
