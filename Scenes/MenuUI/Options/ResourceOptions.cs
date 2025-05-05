using Godot;
using QualityP = Template.UI.QualityPreset;
using VSyncMode = Godot.DisplayServer.VSyncMode;
using Template.UI;

namespace Template;

/* 
 * If the ResourceOptions.cs script is moved then the file path will not updated
 * in the .tres file. In order to fix this go to 
 * C:\Users\VALK-DESKTOP\AppData\Roaming\Godot\app_userdata\Template
 * and delete the .tres file so Godot will be forced to generate it from
 * scratch. This is not a Godot bug it is just something to look out for.
 * 
 * Resource props must have [Export] attribute otherwise they will not save 
 * properly.
 * 
 * The 'recommended' way of storing config files can be found here
 * https://docs.godotengine.org/en/stable/classes/class_configfile.html
 * However this is undesired because values are saved through string keys
 * instead of props.
 */
public partial class ResourceOptions : Resource
{
    // General
    [Export] public Language   Language         { get; set; } = Language.English;
                                                
    // Volume                                   
    [Export] public float      MusicVolume      { get; set; } = 100;
    [Export] public float      SFXVolume        { get; set; } = 100;
                                                
    // Display                                  
    [Export] public WindowMode WindowMode       { get; set; } = WindowMode.Windowed;
    [Export] public VSyncMode  VSyncMode        { get; set; } = VSyncMode.Enabled;
    [Export] public Vector2I   WindowSize       { get; set; }
    [Export] public int        MaxFPS           { get; set; } = 60;
    [Export] public int        Resolution       { get; set; } = 1;
                                                
    // Graphics                                 
    [Export] public QualityP   QualityPreset    { get; set; } = QualityP.High;
    // Antialiasing values can be               
    // 0 - Disabled                             
    // 1 - 2x                                   
    // 2 - 4x                                   
    // 3 - 8x                                   
    [Export] public int        Antialiasing     { get; set; } = 3;
    [Export] public bool       AmbientOcclusion { get; set; }
    [Export] public bool       Glow             { get; set; }
    [Export] public bool       IndirectLighting { get; set; }
    [Export] public bool       Reflections      { get; set; }
                                                
    // Gameplay                                 
    [Export] public Difficulty Difficulty       { get; set; } = Difficulty.Normal;
    [Export] public float      MouseSensitivity { get; set; } = 25;
}

