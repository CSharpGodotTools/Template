using Godot;

namespace GodotUtils;

/// <summary>
/// Factory helpers for GPU particles (3D).
/// </summary>
public static class GpuParticleFactory3D
{
    /// <summary>
    /// Instantiates a one-shot GPU particle and frees it when finished.
    /// </summary>
    /// <param name="parent">Parent node that receives the particle instance.</param>
    /// <param name="particleScene">Packed scene containing a <see cref="GpuParticles3D"/> root.</param>
    /// <returns>Configured one-shot particle instance.</returns>
    public static GpuParticles3D OneShot(Node parent, PackedScene particleScene)
    {
        GpuParticles3D particles = particleScene.Instantiate<GpuParticles3D>();
        particles.OneShot = true;
        particles.Finished += OnFinished;
        particles.TreeExited += OnExitedTree;
        parent.AddChild(particles);
        return particles;

        void OnFinished()
        {
            particles.QueueFree();
        }

        void OnExitedTree()
        {
            particles.Finished -= OnFinished;
            particles.TreeExited -= OnExitedTree;
        }
    }
}
