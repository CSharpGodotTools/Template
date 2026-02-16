using Godot;

namespace Framework.Netcode.Examples.Topdown;

internal sealed class LocalPlayer
{
    private const float MoveSpeed = 200f;
    private const float SendIntervalSeconds = 0.05f;
    private const float SendEpsilonSq = 0.25f;

    private readonly World _world;
    private GameClient _client;
    private ColorRect _node;
    private float _sendAccumulator;
    private Vector2 _lastSentPosition;

    public bool HasLocalPlayer => _node != null;

    public LocalPlayer(World world)
    {
        _world = world;
    }

    public void AttachClient(GameClient client)
    {
        _client = client;
    }

    public void DetachClient()
    {
        _client = null;
        Clear();
    }

    public void EnsureLocalPlayer()
    {
        if (_node == null && _client != null)
        {
            _node = CreateLocalPlayerNode();
            _world.AddChild(_node);
            _lastSentPosition = _node.Position;
            _sendAccumulator = 0f;
        }
    }

    public void ResetAtCenter()
    {
        if (_node != null && _client != null)
        {
            Vector2 center = _world.GetScreenCenter();
            _node.Position = center;
            _lastSentPosition = center;
            _sendAccumulator = 0f;
            _client.SendPosition(center);
        }
    }

    public void Tick(float deltaSeconds)
    {
        if (_node != null && _client != null)
        {
            UpdateMovement(_node, deltaSeconds);
            TrySendPosition(_node.Position, deltaSeconds);
        }
    }

    public void Clear()
    {
        _node?.QueueFree();
        _node = null;

        _sendAccumulator = 0f;
    }

    private ColorRect CreateLocalPlayerNode()
    {
        ColorRect localPlayer = World.CreatePlayerRect(new Color(0.2f, 0.8f, 1f));
        localPlayer.Name = "LocalPlayer";
        localPlayer.Position = _world.GetScreenCenter();
        return localPlayer;
    }

    private static void UpdateMovement(ColorRect node, float deltaSeconds)
    {
        Vector2 inputDirection = Input.GetVector(
            InputActions.MoveLeft,
            InputActions.MoveRight,
            InputActions.MoveUp,
            InputActions.MoveDown);

        if (inputDirection != Vector2.Zero)
        {
            node.Position += inputDirection * MoveSpeed * deltaSeconds;
        }
    }

    private void TrySendPosition(Vector2 position, float deltaSeconds)
    {
        _sendAccumulator += deltaSeconds;
        if (_sendAccumulator < SendIntervalSeconds)
        {
            return;
        }

        if (!HasSignificantMovement(position))
        {
            return;
        }

        _sendAccumulator = 0f;
        _lastSentPosition = position;
        _client.SendPosition(position);
    }

    private bool HasSignificantMovement(Vector2 position)
    {
        return (position - _lastSentPosition).LengthSquared() >= SendEpsilonSq;
    }
}
