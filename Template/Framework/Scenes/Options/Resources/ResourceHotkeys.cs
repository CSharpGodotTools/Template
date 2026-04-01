using Godot;
using Godot.Collections;

// This was intentionally set to GodotUtils instead of __TEMPLATE__ as GodotUtils relies on MainMenuBtnPressed
// and GodotUtils should NOT have any trace of using __TEMPLATE__.
namespace __TEMPLATE__.Ui;

/* 
 * If the ResourceHotkeys.cs script is moved then the file path will not updated
 * in the .tres file. In order to fix this go to AppData\Roaming\Godot\app_userdata\Template
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
/// <summary>
/// Serializable resource that stores input action bindings for options hotkeys.
/// </summary>
public partial class ResourceHotkeys : Resource
{
    /// <summary>
    /// Gets or sets action-to-events mapping used to rebuild InputMap bindings.
    /// </summary>
    [Export] public Dictionary<StringName, Array<InputEvent>> Actions { get; set; } = [];
}
