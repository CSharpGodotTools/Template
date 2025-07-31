namespace __TEMPLATE__;

public static class AutoloadPaths
{
    public const string Global         = BasePath; 
    public const string AudioManager   = BasePath + "ComponentManager/AudioManager";
    public const string Console        = BasePath + "ComponentManager/Debug/Console";
    public const string MetricsOverlay = BasePath + "ComponentManager/Debug/MetricsOverlay";
    public const string OptionsManager = BasePath + "ComponentManager/OptionsManager";
    public const string SceneManager   = BasePath + "ComponentManager/SceneManager";
    public const string Services       = BasePath + "ComponentManager/Services";

    private const string BasePath = "/root/Autoloads/";
}
