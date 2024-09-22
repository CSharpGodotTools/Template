using CSharpUtils;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Template.TopDown2D;

[Service]
public partial class Level : Node
{
    [Export] private Node _entities;
    [Export] private PlayerCamera _playerCamera;
    [Export] private RoomTransitions _roomTransitions;
    [Export] private Timer _enemySendPositions;

    public Player Player { get; private set; }
    public Dictionary<uint, OtherPlayer> OtherPlayers { get; } = [];
    public List<EnemyComponent> EnemyComponents { get; } = [];

    public string PlayerUsername { get; set; }
    private static Vector2 PlayerSpawnPosition { get; } = new Vector2(100, 100);

    public override void _Ready()
    {
        Net net = ServiceProvider.Services.Get<UINetControlPanel>().Net;
        
        _enemySendPositions.Timeout += () =>
        {
            net.Client.Send(new CPacketEnemyPositions
            {
                Positions = EnemyComponents.ToDictionary(x => x.GetInstanceId(), x => x.GlobalPosition)
            });
        };

        net.OnClientCreated += client =>
        {
            client.OnConnected += () =>
            {
                client.Send(new CPacketJoin
                {
                    Username = PlayerUsername,
                    Position = PlayerSpawnPosition
                });

                if (net.Server.IsRunning)
                {
                    client.Send(new CPacketLevelInit
                    {
                        EnemyInstanceIds = EnemyComponents.Select(x => x.GetInstanceId()).ToList()
                    });

                    _enemySendPositions.Start();
                }
            };

            client.OnDisconnected += opcode =>
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

                if (net.Server.IsRunning)
                {
                    _enemySendPositions.Stop();
                }
            };
        };
    }

    // This is called when the client receives a SPacketPlayerConnectionAcknowleged
    public void AddLocalPlayer()
    {
        Player = Player.Instantiate(PlayerSpawnPosition);
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
}
