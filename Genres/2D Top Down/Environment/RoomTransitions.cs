using Godot;
using GodotUtils;
using System.Collections.Generic;

namespace __TEMPLATE__.TopDown2D;

public partial class RoomTransitions : Node
{
    [Export] private TileMapLayer _tileMap; // Reference to the tile map layer
    [Export] private Camera2D _playerCamera; // Reference to the player's camera

    private Player _player; // Reference to the player object

    private Vector2I _currentRoom; // Current room coordinates
    private Vector2I _roomSize; // Size of each room
    private Vector2I _tileSize; // Size of each tile

    // List to keep track of room boundary nodes
    private readonly List<Node2D> _roomBoundNodes = [];

    public override void _Ready()
    {
        InitializeRoomSize();
    }

    // Initializes the size of each room based on tile size and scale
    private void InitializeRoomSize()
    {
        _tileSize = _tileMap.TileSet.TileSize;
        int roomTileSize = 10;

        int roomWidth = (int)_tileMap.Scale.X * _tileSize.X * roomTileSize;
        int roomHeight = (int)_tileMap.Scale.Y * _tileSize.Y * roomTileSize;

        _roomSize = new(roomWidth, roomHeight);
    }

    // Initializes the room transitions with the player object
    public void Init(Player player)
    {
        _player = player;
        LimitCameraBoundsToRoom();
        CreateRoomBoundaries();
        CreateRoomDoorTriggers();
    }

    // Resets the room transitions to initial state
    public void Reset()
    {
        _currentRoom = Vector2I.Zero;
        ClearRoomBoundNodes();
    }

    // Clears all room boundary nodes
    private void ClearRoomBoundNodes()
    {
        _roomBoundNodes.ForEach(node => node.QueueFree());
        _roomBoundNodes.Clear();
    }

    // Removes camera bounds to allow free movement
    private void UnlimitedCameraBounds()
    {
        _playerCamera.LimitTop = int.MinValue;
        _playerCamera.LimitLeft = int.MinValue;
        _playerCamera.LimitBottom = int.MaxValue;
        _playerCamera.LimitRight = int.MaxValue;
    }

    // Creates a trigger for room transitions
    private void CreateRoomDoorTrigger(Vector2 position, Vector2I normal)
    {
        Area2D area = CreateArea2D("Room Door Trigger " + normal, false);
        CollisionShape2D collision = CreateWorldBoundaryShape(normal, Colors.Green);

        area.BodyEntered += body =>
        {
            if (body is Player)
            {
                HandleRoomTransition(normal);
            }
        };

        area.AddChild(collision);
        AddChild(area);
        area.Position = position;

        _roomBoundNodes.Add(area);
    }

    // Creates an Area2D node
    private static Area2D CreateArea2D(string name, bool monitorable)
    {
        return new Area2D
        {
            Name = name,
            Monitorable = monitorable
        };
    }

    // Creates a CollisionShape2D with a WorldBoundaryShape2D
    private static CollisionShape2D CreateWorldBoundaryShape(Vector2I normal, Color color)
    {
        return new CollisionShape2D
        {
            Shape = new WorldBoundaryShape2D
            {
                Normal = normal
            },
            Modulate = color
        };
    }

    // Handles the room transition when a trigger is entered
    private void HandleRoomTransition(Vector2I normal)
    {
        GD.Print("Entered the trigger " + normal);

        if (IsValidTransitionNormal(normal))
        {
            TransitionToNextRoom(normal);
        }
    }

    // Checks if the transition normal is valid
    private static bool IsValidTransitionNormal(Vector2I normal)
    {
        return normal == Vector2I.Right ||
               normal == Vector2I.Left ||
               normal == Vector2I.Up ||
               normal == Vector2I.Down;
    }

    // Transitions to the next room based on the normal vector
    private void TransitionToNextRoom(Vector2I normal)
    {
        _currentRoom += normal * -1;

        ClearRoomBoundNodes();
        PrepareCameraForTransition();
        AnimateCameraAndPlayer(normal);
    }

    // Prepares the camera for the transition animation
    private void PrepareCameraForTransition()
    {
        _playerCamera.SetPhysicsProcess(false);
        _playerCamera.PositionSmoothingEnabled = false;
        _playerCamera.Position = _playerCamera.GetScreenCenterPosition();
        UnlimitedCameraBounds();
    }

    // Animates the camera and player during the transition
    private void AnimateCameraAndPlayer(Vector2I normal)
    {
        const double duration = 1;
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;
        Vector2 transitionOffset = screenSize * (normal * -1);

        new GTween(_playerCamera)
            .SetAnimatingProp(Node2D.PropertyName.Position)
            .AnimateProp(_playerCamera.Position + transitionOffset, duration)
            .TransExpo();

        new GTween(_player)
            .SetAnimatingProp(Node2D.PropertyName.Position)
            .AnimateProp(_player.Position + new Vector2(150, 150) * (normal * -1), duration)
            .EaseIn()
            .Callback(FinalizeRoomTransition);
    }

    // Finalizes the room transition after the animation
    private void FinalizeRoomTransition()
    {
        _playerCamera.SetPhysicsProcess(true);
        _playerCamera.PositionSmoothingEnabled = true;
        LimitCameraBoundsToRoom();
        CreateRoomBoundaries();
        CreateRoomDoorTriggers();
    }

    // Limits the camera bounds to the current room
    private void LimitCameraBoundsToRoom()
    {
        _playerCamera.LimitTop = _roomSize.Y * _currentRoom.Y;
        _playerCamera.LimitLeft = _roomSize.X * _currentRoom.X;
        _playerCamera.LimitBottom = _roomSize.Y + (_roomSize.Y * _currentRoom.Y);
        _playerCamera.LimitRight = _roomSize.X + (_roomSize.X * _currentRoom.X);
    }

    // Creates triggers for room transitions at each door
    private void CreateRoomDoorTriggers()
    {
        Vector2 offset = new(32, 32);

        CreateRoomDoorTrigger(_roomSize * _currentRoom + offset, Vector2I.Down);
        CreateRoomDoorTrigger(_roomSize * _currentRoom + offset, Vector2I.Right);
        CreateRoomDoorTrigger(_roomSize + (_roomSize * _currentRoom) - offset, Vector2I.Up);
        CreateRoomDoorTrigger(_roomSize + (_roomSize * _currentRoom) - offset, Vector2I.Left);
    }

    // Creates boundaries for the current room
    private void CreateRoomBoundaries()
    {
        CreateWorldBoundary(_roomSize * _currentRoom, Vector2I.Down);
        CreateWorldBoundary(_roomSize * _currentRoom, Vector2I.Right);
        CreateWorldBoundary(_roomSize + (_roomSize * _currentRoom), Vector2I.Up);
        CreateWorldBoundary(_roomSize + (_roomSize * _currentRoom), Vector2I.Left);
    }

    // Creates a world boundary at the specified position and normal
    private void CreateWorldBoundary(Vector2 position, Vector2I normal)
    {
        StaticBody2D body = new()
        {
            Name = "Room Boundary " + normal
        };

        CollisionShape2D collision = CreateWorldBoundaryShape(normal, Colors.White);

        body.AddChild(collision);
        AddChild(body);
        body.Position = position;

        _roomBoundNodes.Add(body);
    }
}

