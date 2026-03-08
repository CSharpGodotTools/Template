using Framework.Netcode.Client;
using Godot;
using System.Collections.Generic;

namespace Framework.Netcode.Examples.Topdown;

public partial class World : Node2D
{
    private const float PlayerSize = 18f;

    private NetControlPanel _netControlPanel;
    private GameClient _client;
    private WorldStressTest _stressTest;
    private LocalPlayer _localPlayer;
    private RemotePlayers _remotePlayers;

    public override void _Ready()
    {
        _netControlPanel = GetNode<NetControlPanel>("CanvasLayer/Multiplayer");
        _localPlayer = new LocalPlayer(this);
        _remotePlayers = new RemotePlayers(this);
        _stressTest = new WorldStressTest(this);

        _netControlPanel.Net.ClientCreated += OnClientCreated;
        _netControlPanel.Net.ClientDestroyed += OnClientDestroyed;
        RefreshProcessingState();
    }

    public override void _ExitTree()
    {
        if (_netControlPanel != null && _netControlPanel.Net != null)
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

    public Vector2 GetScreenCenter()
    {
        return GetViewportRect().Size * 0.5f;
    }

    internal void ClearRemotePlayers()
    {
        _remotePlayers?.ClearAll();
    }

    private void OnClientCreated(GodotClient client)
    {
        if (client is GameClient gameClient)
        {
            AttachClient(gameClient);
        }
    }

    private void OnClientDestroyed(GodotClient client)
    {
        if (client is GameClient gameClient && gameClient == _client)
        {
            DetachClient();
        }
    }

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

    private void DetachClient()
    {
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

    private void OnClientConnected()
    {
        _localPlayer.EnsureLocalPlayer();
        _localPlayer.ResetAtCenter();
        RefreshProcessingState();
    }

    private void OnClientDisconnected(DisconnectOpcode opcode)
    {
        ClearPlayers();
        RefreshProcessingState();
    }

    private void OnLocalPlayerReady(uint localId)
    {
        _localPlayer.EnsureLocalPlayer();
        RefreshProcessingState();
    }

    private void OnRemotePlayerJoined(uint id)
    {
        _remotePlayers.EnsureRemote(id);
    }

    private void OnRemotePlayerLeft(uint id)
    {
        _remotePlayers.Remove(id);
    }

    private void OnRemotePositionsUpdated(IReadOnlyDictionary<uint, Vector2> positions)
    {
        _remotePlayers.UpdateTargets(positions);
    }

    private void ClearPlayers()
    {
        _localPlayer?.Clear();
        _remotePlayers?.ClearAll();
    }

    private void RefreshProcessingState()
    {
        bool hasReadyNetworkPlayer = _client != null
            && _client.IsConnected
            && _client.HasLocalId
            && _localPlayer != null
            && _localPlayer.HasLocalPlayer;

        bool stressTestRunning = _stressTest != null && _stressTest.IsRunning;
        SetProcess(stressTestRunning || hasReadyNetworkPlayer);
    }
}
