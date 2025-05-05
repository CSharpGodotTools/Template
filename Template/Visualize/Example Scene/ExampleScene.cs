using Godot;
using GodotUtils;

namespace Template.Example;

public partial class ExampleScene : Node
{
    private Camera2D _camera;

	private const int CAMERA_SPEED = 5;

	public override void _Ready()
	{
		_camera = GetNode<Camera2D>("Camera2D");

        VisualizeExampleSprite sprite = VisualizeExampleSprite.Instantiate();

        // As you can see the visualize info is created at the moment of node creation
        _ = new GTween(this)
			.Delay(0.1)
			.Callback(() =>
			{
                AddChild(sprite);
            });
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key)
		{
			if (key.Keycode == Key.A)
			{
				_camera.Position -= new Vector2(CAMERA_SPEED, 0);
			}

			if (key.Keycode == Key.D)
			{
				_camera.Position += new Vector2(CAMERA_SPEED, 0);
			}

			if (key.Keycode == Key.W)
			{
				_camera.Position -= new Vector2(0, CAMERA_SPEED);
			}

			if (key.Keycode == Key.S)
			{
				_camera.Position += new Vector2(0, CAMERA_SPEED);
			}
		}
    }
}
