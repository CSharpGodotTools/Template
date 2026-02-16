using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for 2D GPU particles.
/// </summary>
public static class GPUParticles2DExtensions
{
    /// <summary>
    /// Gets the ParticleProcessMaterial assigned to the particle node.
    /// </summary>
    public static ParticleProcessMaterial GetParticleProcessMaterial(this GpuParticles2D particles)
    {
        return (ParticleProcessMaterial)particles.ProcessMaterial;
    }
}
