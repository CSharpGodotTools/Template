using Godot;

namespace GodotUtils;

/// <summary>
/// Extension helpers for RigidBody2D.
/// </summary>
public static class RigidBody2dExtensions
{
    /// <summary>
    /// Smoothly rotates a rigid body toward a target position.
    /// </summary>
    /// <param name="rigidBody">Rigid body to rotate.</param>
    /// <param name="targetPosition">Target world position.</param>
    /// <param name="rotationSpeed">Speed multiplier for angular correction.</param>
    /// <param name="maxAngularSpeed">Maximum absolute angular velocity.</param>
    /// <param name="smoothness">Lerp factor for angular velocity smoothing.</param>
    public static void RotateTowards(this RigidBody2D rigidBody, Vector2 targetPosition, float rotationSpeed, float maxAngularSpeed, float smoothness = 0.1f)
    {
        // Compute direction and target angle
        Vector2 direction = targetPosition - rigidBody.GlobalPosition;
        float targetAngle = direction.Angle();

        // Shortest angle difference (-pi to pi)
        float angleDiff = Mathf.Wrap(targetAngle - rigidBody.Rotation, -Mathf.Pi, Mathf.Pi);

        // Desired angular velocity
        float desiredAngularVelocity = angleDiff * rotationSpeed;

        // Clamp angular velocity
        desiredAngularVelocity = Mathf.Clamp(desiredAngularVelocity, -maxAngularSpeed, maxAngularSpeed);

        // Smoothly apply angular velocity
        rigidBody.AngularVelocity = Mathf.Lerp(rigidBody.AngularVelocity, desiredAngularVelocity, smoothness);
    }
}
