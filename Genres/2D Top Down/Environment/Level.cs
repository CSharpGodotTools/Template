using Godot;
using GodotUtils;
using System.Collections.Generic;
using __TEMPLATE__.UI;
using __TEMPLATE__.Netcode.Client;
using __TEMPLATE__.Netcode;

namespace __TEMPLATE__.TopDown2D;

[Service]
public partial class Level : Node
{
    // Exports
    [Export] private Node            _entities;
    [Export] private PlayerCamera    _playerCamera;
    [Export] private RoomTransitions _roomTransitions;

    // Properties
    public Dictionary<uint, OtherPlayer> OtherPlayers    { get; } = [];
    public List<EnemyComponent>          EnemyComponents { get; } = [];
    public Player                        Player          { get; private set; }
    public string                        PlayerUsername  { get; set; }

    // Fields
    private static Vector2 _playerSpawnPosition = new(100, 100);

    public override void _Ready()
    {
        Services.Get<NetControlPanel>().Net.ClientCreated += OnClientCreated;
    }

    // Called when the client receives a SPacketPlayerConnectionAcknowleged
    public void AddLocalPlayer()
    {
        Player = Player.Instantiate(_playerSpawnPosition);
        _entities.AddChild(Player);

        _playerCamera.StartFollowingPlayer(Player);
        _roomTransitions.Init(Player);
    }

    public void AddOtherPlayer(uint id, PlayerData playerData)
    {
        OtherPlayer otherPlayer = OtherPlayer.Instantiate(id, playerData);

        _entities.AddChild(otherPlayer);
        OtherPlayers.Add(id, otherPlayer);
    }

    public void RemoveOtherPlayer(uint id)
    {
        OtherPlayers[id].QueueFree();
        OtherPlayers.Remove(id);
    }

    private void OnClientCreated(ENetClient client)
    {
        client.Connected += () => OnClientConnected(client);
        client.Disconnected += OnClientDisconnected;

        Services.Get<UIPopupMenu>().OnMainMenuBtnPressed += client.Stop;
    }

    private void OnClientConnected(ENetClient client)
    {
        client.Send(new CPacketJoin
        {
            Username = PlayerUsername,
            Position = _playerSpawnPosition
        });
    }

    private void OnClientDisconnected(DisconnectOpcode opcode)
    {
        // WARNING:
        // Do not reset world here with Global.Servers.Get<SceneManager>().ResetCurrentScene()
        //
        // REASON:
        // Resetting the world will reset the contents of Net.cs and Level.cs which will result
        // in numerous problems. Attempts have been made to turn Net into a persistent autoload
        // however doing so will require Player and OtherPlayers to be defined in Net.cs and
        // doing stuff like playerCamera.StartFollowingPlayer(Player); and entities.AddChild(otherPlayer);
        // will need to be done in Net.cs because Level.cs will get reset. Getting the playerCamera
        // and a path to the entities node from Net.cs is miserable. Imagine doing
        // GetTree().Root.GetNode<PlayerCamera>("/root/Level/Camera2D")...
        // Another reason to avoid resetting the entire world is to avoid seeing the lag created
        // from the world reset.

        Player.QueueFree();
        Player = null;

        OtherPlayers.Values.ForEach(x => x.QueueFree());
        OtherPlayers.Clear();

        _playerCamera.StopFollowingPlayer();
        _roomTransitions.Reset();
    }
}
