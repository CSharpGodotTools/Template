using Godot;
using GodotUtils;

namespace __TEMPLATE__.Platformer2D.Retro;

// Entity
public abstract partial class Entity : CharacterBody2D
{
    protected AnimatedSprite2D Sprite;
    private Label _stateLabel;
    private State _curState;

    public override void _Ready()
    {
        Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        _stateLabel = new Label();
        AddChild(_stateLabel);

        Init();

        _curState = InitialState();
        UpdateStateLabel(_curState);

        _curState.Enter();
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();

        Update();
        _curState.Update(delta);
    }

    protected abstract State InitialState();

    public void SwitchState(State newState)
    {
        _curState.Exit();
        newState.Enter();
        _curState = newState;

        UpdateStateLabel(newState);
    }

    public virtual void Init() { }
    public virtual void Update() { }

    private void UpdateStateLabel(State state)
    {
        _stateLabel.Text = state.ToString();
        _stateLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterBottom);
        _stateLabel.Position -= new Vector2(0, _stateLabel.Size.Y / 2);
    }
}

