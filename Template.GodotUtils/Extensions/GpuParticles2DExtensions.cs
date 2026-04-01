using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for 2D GPU particles.
/// </summary>
public static class GpuParticles2DExtensions
{
    /// <summary>
    /// Gets the ParticleProcessMaterial assigned to the particle node.
    /// </summary>
    /// <param name="particles">Particle node to inspect.</param>
    /// <returns>Assigned particle process material.</returns>
    public static ParticleProcessMaterial GetParticleProcessMaterial(this GpuParticles2D particles)
    {
        return (ParticleProcessMaterial)particles.ProcessMaterial;
    }
}
