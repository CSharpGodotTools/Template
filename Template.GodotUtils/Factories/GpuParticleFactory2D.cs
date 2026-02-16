using Godot;

namespace GodotUtils;

/// <summary>
/// Factory helpers for GPU particles (2D).
/// </summary>
public static class GpuParticleFactory2D
{
    /// <summary>
    /// Instantiates a one-shot GPU particle and frees it when finished.
    /// </summary>
    public static GpuParticles2D OneShot(Node parent, PackedScene particleScene)
    {
        GpuParticles2D particles = particleScene.Instantiate<GpuParticles2D>();
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
            particles.Finished -= particles.QueueFree;
            particles.TreeExited -= OnExitedTree;
        }
    }

    /// <summary>
    /// Instantiates a looping GPU particle without auto-free.
    /// </summary>
    public static GpuParticles2D Looping(Node parent, PackedScene particleScene)
    {
        GpuParticles2D particles = particleScene.Instantiate<GpuParticles2D>();
        particles.OneShot = false;
        parent.AddChild(particles);
        return particles;
    }
}
