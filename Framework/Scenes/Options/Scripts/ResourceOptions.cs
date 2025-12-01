using GodotUtils.UI;
using QualityP = GodotUtils.UI.QualityPreset;
using VSyncMode = Godot.DisplayServer.VSyncMode;

// This was intentionally set to GodotUtils instead of __TEMPLATE__ as GodotUtils relies on MainMenuBtnPressed
// and GodotUtils should NOT have any trace of using __TEMPLATE__.
namespace GodotUtils;

public class ResourceOptions
{
    // General
    public Language   Language         { get; set; } = Language.English;
                                                
    // Volume                                   
    public float      MusicVolume      { get; set; } = 100;
    public float      SFXVolume        { get; set; } = 100;
                                                
    // Display                                  
    public WindowMode WindowMode       { get; set; } = WindowMode.Windowed;
    public VSyncMode  VSyncMode        { get; set; } = VSyncMode.Enabled;
    public int        WindowWidth      { get; set; }
    public int        WindowHeight     { get; set; }
    public int        MaxFPS           { get; set; } = 60;
    public int        Resolution       { get; set; } = 1;
                                                
    // Graphics                                 
    public QualityP   QualityPreset    { get; set; } = QualityP.High;
    // Antialiasing values can be               
    // 0 - Disabled                             
    // 1 - 2x                                   
    // 2 - 4x                                   
    // 3 - 8x                                   
    public int        Antialiasing     { get; set; } = 3;
    public bool       AmbientOcclusion { get; set; }
    public bool       Glow             { get; set; }
    public bool       IndirectLighting { get; set; }
    public bool       Reflections      { get; set; }
                                                
    // Gameplay                                 
    public Difficulty Difficulty       { get; set; } = Difficulty.Normal;
    public float      MouseSensitivity { get; set; } = 25;
}
