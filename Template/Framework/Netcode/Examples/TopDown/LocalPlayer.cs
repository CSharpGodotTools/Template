using Godot;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// Manages local-player node creation, movement, and position send throttling.
/// </summary>
internal sealed class LocalPlayer
{
    private const float MoveSpeed = 200f;
    private const float SendIntervalSeconds = 0.05f;
    private const float SendEpsilonSq = 0.25f;

    // Manually defining the actions in the event that the developer decides to remove them
    // Ideally we should expose an option to the developer to delete the entire netcode
    // example folders but since the netcode examples are not fully complete I do not want
    // to expose them just yet as 'templates' in the setup plugin.
    private static readonly StringName _moveLeft = "move_left";
    private static readonly StringName _moveRight = "move_right";
    private static readonly StringName _moveUp = "move_up";
    private static readonly StringName _moveDown = "move_down";

    private readonly World _world;
    private GameClient? _client;
    private ColorRect? _node;
    private float _sendAccumulator;
    private Vector2 _lastSentPosition;

    /// <summary>
    /// Gets whether a local-player node currently exists.
    /// </summary>
    public bool HasLocalPlayer => _node != null;

    /// <summary>
    /// Creates local-player controller for a world instance.
    /// </summary>
    /// <param name="world">TopDown world host node.</param>
    public LocalPlayer(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Attaches a game client used to send local position updates.
    /// </summary>
    /// <param name="client">Connected game client instance.</param>
    public void AttachClient(GameClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Detaches active client and clears local-player node state.
    /// </summary>
    public void DetachClient()
    {
        _client = null;
        Clear();
    }

    /// <summary>
    /// Creates local-player node when client is attached and node is missing.
    /// </summary>
    public void EnsureLocalPlayer()
    {
        // Create the local avatar only once after a client is attached.
        if (_node == null && _client != null)
        {
            _node = CreateLocalPlayerNode();
            _world.AddChild(_node);
            _lastSentPosition = _node.Position;
            _sendAccumulator = 0f;
        }
    }

    /// <summary>
    /// Resets local-player position to screen center and sends immediate update.
    /// </summary>
    public void ResetAtCenter()
    {
        // Reset only when both the local node and active client are available.
        if (_node != null && _client != null)
        {
            Vector2 center = _world.GetScreenCenter();
            _node.Position = center;
            _lastSentPosition = center;
            _sendAccumulator = 0f;
            _client.SendPosition(center);
        }
    }

    /// <summary>
    /// Updates local movement and sends throttled position updates.
    /// </summary>
    /// <param name="deltaSeconds">Frame delta in seconds.</param>
    public void Tick(float deltaSeconds)
    {
        // Skip movement and network sends until local node and client both exist.
        if (_node != null && _client != null)
        {
            UpdateMovement(_node, deltaSeconds);
            TrySendPosition(_node.Position, deltaSeconds);
        }
    }

    /// <summary>
    /// Clears local-player node and send-throttle state.
    /// </summary>
    public void Clear()
    {
        _node?.QueueFree();
        _node = null;

        _sendAccumulator = 0f;
    }

    /// <summary>
    /// Creates a local-player visual node centered in the world.
    /// </summary>
    /// <returns>Configured local-player color-rect node.</returns>
    private ColorRect CreateLocalPlayerNode()
    {
        ColorRect localPlayer = World.CreatePlayerRect(new Color(0.2f, 0.8f, 1f));
        localPlayer.Name = "LocalPlayer";
        localPlayer.Position = _world.GetScreenCenter();
        return localPlayer;
    }

    /// <summary>
    /// Applies frame movement input to the local-player node.
    /// </summary>
    /// <param name="node">Local-player node.</param>
    /// <param name="deltaSeconds">Frame delta in seconds.</param>
    private static void UpdateMovement(ColorRect node, float deltaSeconds)
    {
        Vector2 inputDirection = Input.GetVector(_moveLeft, _moveRight, _moveUp, _moveDown);

        // Move only when there is non-zero directional input this frame.
        if (inputDirection != Vector2.Zero)
        {
            node.Position += inputDirection * MoveSpeed * deltaSeconds;
        }
    }

    /// <summary>
    /// Sends position updates at a fixed interval when movement exceeds epsilon.
    /// </summary>
    /// <param name="position">Current local-player position.</param>
    /// <param name="deltaSeconds">Frame delta in seconds.</param>
    private void TrySendPosition(Vector2 position, float deltaSeconds)
    {
        _sendAccumulator += deltaSeconds;

        // Throttle updates to the configured send interval.
        if (_sendAccumulator < SendIntervalSeconds)
        {
            return;
        }

        // Avoid network sends for sub-epsilon jitter.
        if (!HasSignificantMovement(position))
        {
            return;
        }

        _sendAccumulator = 0f;
        _lastSentPosition = position;
        _client!.SendPosition(position);
    }

    /// <summary>
    /// Returns whether movement delta is large enough to justify a network update.
    /// </summary>
    /// <param name="position">Current local-player position.</param>
    /// <returns><see langword="true"/> when movement exceeds configured epsilon.</returns>
    private bool HasSignificantMovement(Vector2 position)
    {
        return (position - _lastSentPosition).LengthSquared() >= SendEpsilonSq;
    }
}
