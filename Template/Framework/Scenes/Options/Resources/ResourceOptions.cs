using __TEMPLATE__.Ui;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using QualityP = __TEMPLATE__.Ui.QualityPreset;
using VSyncMode = Godot.DisplayServer.VSyncMode;

namespace __TEMPLATE__;

// Keep this class partial so game projects can extend options outside Framework
// by adding `public partial class ResourceOptions` files in their own folders.
public partial class ResourceOptions
{
    public const int CurrentSchemaVersion = 2;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    // General
    public Language Language { get; set; } = Language.English;

    // Volume                                   
    public float MusicVolume { get; set; } = 100;
    public float SFXVolume { get; set; } = 100;

    // Display                                  
    public WindowMode WindowMode { get; set; } = WindowMode.Windowed;
    public VSyncMode VSyncMode { get; set; } = VSyncMode.Enabled;
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public int MaxFPS { get; set; } = 60;
    public int Resolution { get; set; } = 1;

    // Graphics                                 
    public QualityP QualityPreset { get; set; } = QualityP.High;
    // Antialiasing values can be               
    // 0 - Disabled                             
    // 1 - 2x                                   
    // 2 - 4x                                   
    // 3 - 8x                                   
    public int Antialiasing { get; set; } = 3;
    public bool AmbientOcclusion { get; set; }
    public bool Glow { get; set; }
    public bool IndirectLighting { get; set; }
    public bool Reflections { get; set; }

    // Custom options are persisted inline at the root of options.json.
    [JsonExtensionData]
    public Dictionary<string, JsonElement> CustomOptionValues { get; set; } = [];

    public void Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        MusicVolume = Math.Clamp(MusicVolume, 0f, 100f);
        SFXVolume = Math.Clamp(SFXVolume, 0f, 100f);
        WindowWidth = Math.Max(0, WindowWidth);
        WindowHeight = Math.Max(0, WindowHeight);
        MaxFPS = Math.Max(0, MaxFPS);
        Resolution = Math.Clamp(Resolution, 1, 36);
        Antialiasing = Math.Clamp(Antialiasing, 0, 3);
        CustomOptionValues ??= [];

        NormalizeExtended();
    }

    partial void NormalizeExtended();
}
