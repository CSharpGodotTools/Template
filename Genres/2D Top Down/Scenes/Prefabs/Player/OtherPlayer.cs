using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using __TEMPLATE__.Netcode;

namespace __TEMPLATE__.TopDown2D;

public partial class OtherPlayer : Node2D
{
    public Vector2 LastServerPosition { get; set; }

    private float _smoothFactor;

    [OnInstantiate]
    private void Init(uint id, PlayerData playerData)
    {
        LastServerPosition = playerData.Position;
        Position = playerData.Position;
        SetLabelText($"{playerData.Username} ({id})");
    }

    public override void _Ready()
    {
        _smoothFactor = CalculateSmoothFactor(Net.HeartbeatPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
        float distance = Position.DistanceTo(LastServerPosition);

        // Move the current position towards the last known server position by a fraction of the distance.
        // The _smoothFactor determines how much of the distance to cover in this step. A _smoothFactor of
        // 1 would mean the position is instantly moved to the last known server position, while a
        // _smoothFactor of 0.5 would mean the position is moved halfway towards the last known server
        // position.

        if (distance > 1000)
        {
            Position = LastServerPosition;
        }
        else
        {
            Position = Position.MoveToward(LastServerPosition, distance * _smoothFactor);
        }
    }

    public void SetLabelText(string text)
    {
        GetNode<Label>("Label").Text = text;
    }

    private static float CalculateSmoothFactor(int heartbeatPosition)
    {
        if (heartbeatPosition < 1)
        {
            throw new ArgumentException("Heartbeat position must be greater than or equal to 1.", nameof(heartbeatPosition));
        }

        // These values were all estimated manually, some values might be slightly inaccurate
        // If the heartbeat is set to 500 then the client will bounce towards next position regardless of what
        // value smooth factor is

        // The lower the smooth factor the smoother the transition
        // If the smooth factor is too low then the player will start to lag behind
        // If the smooth factor is too high then you will start to see glitchy movements because the
        // the position is constantly being clamped the last received server position
        Dictionary<int, float> predefinedFactors = new()
        {
            { 20, 0.1f },
            { 50, 0.075f },
            { 100, 0.05f },
            { 200, 0.02f }
        };

        // If the heartbeat position is exactly one of the predefined values, return the corresponding smooth factor
        if (predefinedFactors.TryGetValue(heartbeatPosition, out float smoothFactor))
        {
            return smoothFactor;
        }

        // Find the closest heartbeat positions in the dictionary
        int lowerKey = predefinedFactors.Keys.LastOrDefault(k => k <= heartbeatPosition);
        int upperKey = predefinedFactors.Keys.FirstOrDefault(k => k >= heartbeatPosition);

        // If the heartbeat position is outside the predefined range, use the closest value
        if (lowerKey == 0)
        {
            return predefinedFactors[upperKey];
        }
        if (upperKey == 0)
        {
            return predefinedFactors[lowerKey];
        }

        // Perform linear interpolation between the closest values
        float lowerValue = predefinedFactors[lowerKey];
        float upperValue = predefinedFactors[upperKey];
        float t = (float)(heartbeatPosition - lowerKey) / (upperKey - lowerKey);

        return Mathf.Lerp(lowerValue, upperValue, t);
    }
}
