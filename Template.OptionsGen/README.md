Generates strongly typed properties for all settings.

For example:

```csharp
int difficulty = optionsService.Settings.Difficulty;
optionsService.Settings.Difficulty = 2;

float sensitivity = optionsService.Settings.MouseSensitivity;
optionsService.Settings.MouseSensitivity = 0.9f;
```
