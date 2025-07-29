using Godot;

namespace __TEMPLATE__.FPS3D;

public partial class Env : WorldEnvironment
{
    public override void _Ready()
    {
        ResourceOptions options = GetNode<OptionsManager>(Autoloads.OptionsManager).Options;
        
        Environment.GlowEnabled = options.Glow;
        Environment.SsrEnabled = options.Reflections;
        Environment.SsaoEnabled = options.AmbientOcclusion;
        Environment.SsilEnabled = options.IndirectLighting;
    }
}
