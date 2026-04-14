using __TEMPLATE__.Netcode.Client;
using Godot;
using System.Collections.Generic;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

/// <summary>
/// Coordinates local and remote player visuals for the TopDown example scene.
/// </summary>
public partial class World : Node2D
{
    /// <summary>
    /// Default square size used for player markers.
    /// </summary>
    private const float PlayerSize = 18f;

    private NetControlPanel _netControlPanel = null!;
    private GameClient? _client;
    private WorldStressTest _stressTest = null!;
    private LocalPlayer _localPlayer = null!;
    private RemotePlayers _remotePlayers = null!;

    public override void _Ready()
    {
        _netControlPanel = GetNode<NetControlPanel>("CanvasLayer/Multiplayer");
        _localPlayer = new LocalPlayer(this);
        _remotePlayers = new RemotePlayers(this);
        _stressTest = new WorldStressTest(this);

        _netControlPanel.Net!.ClientCreated += OnClientCreated;
        _netControlPanel.Net!.ClientDestroyed += OnClientDestroyed;
        RefreshProcessingState();
    }

    public override void _ExitTree()
    {
        // Unsubscribe only when the multiplayer panel and net layer are available.
        if (_netControlPanel?.Net != null)
        {
            _netControlPanel.Net.ClientCreated -= OnClientCreated;
            _netControlPanel.Net.ClientDestroyed -= OnClientDestroyed;
        }

        _stressTest?.Dispose();

        DetachClient();
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        // Tick simulation only when all player and stress-test helpers exist.
        if (_localPlayer != null && _remotePlayers != null && _stressTest != null)
        {
            float deltaSeconds = (float)delta;
            _localPlayer.Tick(deltaSeconds);
            _remotePlayers.Tick(deltaSeconds);
            _stressTest.Tick(deltaSeconds);
        }
    }

    public static ColorRect CreatePlayerRect(Color color)
    {
        return new ColorRect
        {
            Color = color,
            Size = new Vector2(PlayerSize, PlayerSize)
        };
    }

    /// <summary>
    /// Gets current viewport center in local world coordinates.
    /// </summary>
    /// <returns>Screen center point.</returns>
    public Vector2 GetScreenCenter()
    {
        return GetViewportRect().Size * 0.5f;
    }

    /// <summary>
    /// Clears all tracked remote player visuals.
    /// </summary>
    internal void ClearRemotePlayers()
    {
        _remotePlayers?.ClearAll();
    }

    /// <summary>
    /// Handles net client creation and attaches compatible game clients.
    /// </summary>
    /// <param name="client">Created client instance.</param>
    private void OnClientCreated(GodotClient client)
    {
        // Attach only the TopDown game-client implementation.
        if (client is GameClient gameClient)
            AttachClient(gameClient);
    }

    /// <summary>
    /// Handles net client destruction and detaches active client if needed.
    /// </summary>
    /// <param name="client">Destroyed client instance.</param>
    private void OnClientDestroyed(GodotClient client)
    {
        // Detach only when the destroyed client is the currently active one.
        if (client is GameClient gameClient && gameClient == _client)
            DetachClient();
    }

    /// <summary>
    /// Attaches a game client and subscribes gameplay events.
    /// </summary>
    /// <param name="client">Client to attach.</param>
    private void AttachClient(GameClient client)
    {
        DetachClient();

        _client = client;
        _client.Connected += OnClientConnected;
        _client.Disconnected += OnClientDisconnected;
        _client.LocalPlayerReady += OnLocalPlayerReady;
        _client.RemotePlayerJoined += OnRemotePlayerJoined;
        _client.RemotePlayerLeft += OnRemotePlayerLeft;
        _client.RemotePositionsUpdated += OnRemotePositionsUpdated;

        _localPlayer.AttachClient(client);
        RefreshProcessingState();
    }

    /// <summary>
    /// Detaches current client and clears player state.
    /// </summary>
    private void DetachClient()
    {
        // Remove subscriptions only when an active client is currently attached.
        if (_client != null)
        {
            _client.Connected -= OnClientConnected;
            _client.Disconnected -= OnClientDisconnected;
            _client.LocalPlayerReady -= OnLocalPlayerReady;
            _client.RemotePlayerJoined -= OnRemotePlayerJoined;
            _client.RemotePlayerLeft -= OnRemotePlayerLeft;
            _client.RemotePositionsUpdated -= OnRemotePositionsUpdated;
            _client = null;
        }

        _localPlayer?.DetachClient();

        ClearPlayers();
        RefreshProcessingState();
    }

    /// <summary>
    /// Handles connected event and initializes local player.
    /// </summary>
    private void OnClientConnected()
    {
        _localPlayer.EnsureLocalPlayer();
        _localPlayer.ResetAtCenter();
        RefreshProcessingState();
    }

    /// <summary>
    /// Handles disconnected event and clears players.
    /// </summary>
    /// <param name="opcode">Disconnect reason opcode.</param>
    private void OnClientDisconnected(DisconnectOpcode opcode)
    {
        ClearPlayers();
        RefreshProcessingState();
    }

    /// <summary>
    /// Handles local-player-ready event.
    /// </summary>
    private void OnLocalPlayerReady()
    {
        _localPlayer.EnsureLocalPlayer();
        RefreshProcessingState();
    }

    /// <summary>
    /// Handles remote-player-joined event.
    /// </summary>
    /// <param name="id">Remote player id.</param>
    private void OnRemotePlayerJoined(uint id)
    {
        _remotePlayers.EnsureRemote(id);
    }

    /// <summary>
    /// Handles remote-player-left event.
    /// </summary>
    /// <param name="id">Remote player id.</param>
    private void OnRemotePlayerLeft(uint id)
    {
        _remotePlayers.Remove(id);
    }

    /// <summary>
    /// Handles remote player position snapshots.
    /// </summary>
    /// <param name="positions">Snapshot map keyed by player id.</param>
    private void OnRemotePositionsUpdated(IReadOnlyDictionary<uint, Vector2> positions)
    {
        _remotePlayers.UpdateTargets(positions);
    }

    /// <summary>
    /// Clears local and remote player state.
    /// </summary>
    private void ClearPlayers()
    {
        _localPlayer?.Clear();
        _remotePlayers?.ClearAll();
    }

    /// <summary>
    /// Updates world processing state from network/stress-test activity.
    /// </summary>
    private void RefreshProcessingState()
    {
        bool hasReadyNetworkPlayer = _client != null
            && _client.IsConnected
            && _localPlayer?.HasLocalPlayer == true;

        bool serverRunning = _netControlPanel?.Net?.Server?.IsRunning == true;
        bool stressTestRunning = _stressTest?.IsRunning == true;
        SetProcess(stressTestRunning || hasReadyNetworkPlayer || serverRunning);
    }
}
