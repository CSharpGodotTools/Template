# Metrics Overlay
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
