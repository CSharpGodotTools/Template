using Godot;

namespace __TEMPLATE__.FPS3D;

public partial class Env : WorldEnvironment
{
    [Export] private OptionsManager _options;

    public override void _Ready()
    {
        ResourceOptions options = _options.Options;
        
        Environment.GlowEnabled = options.Glow;
        Environment.SsrEnabled = options.Reflections;
        Environment.SsaoEnabled = options.AmbientOcclusion;
        Environment.SsilEnabled = options.IndirectLighting;
    }
}

