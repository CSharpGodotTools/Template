using Godot;
using GodotUtils;
using System.Collections.Generic;

namespace __TEMPLATE__.FPS3D;

public partial class Player
{
    [Export] private BoneAttachment3D _cameraBone;
    [Export] private AnimationTree _animTree;
    [Export] private Node3D _fpsRig;
    [Export] private Node3D _nodeItems;

    //bool isReloading { get => animTree.GetCondition("reload"); }

    private readonly List<Item> _items = [];
    private int _curItemIndex;
    private Camera3D _camera;
    private Vector3 _camOffset;
    private bool _switchingGuns;

    private void OnReadyAnimation()
    {
        _camera = GetNode<Camera3D>("%Camera3D");
        _camOffset = _camera.Position - Position;

        foreach (Node3D node in _nodeItems.GetChildren<Node3D>(false))
        {
            _items.Add(new Item(node));
        }

        RecreateCameraBone(_curItemIndex);

        _animTree.AnimationStarted += anim =>
        {
            switch (anim)
            {
                case "Holster":
                    AnimationNodeStateMachinePlayback stateMachine = _animTree.GetStateMachine();
                    _switchingGuns = stateMachine.GetCurrentNode() == "Holster";
                    break;
            }
        };

        _animTree.AnimationFinished += async anim =>
        {
            switch (anim)
            {
                case "Rest to ADS":
                case "ADS":
                    if (!Input.IsActionPressed(InputActions.Ads))
                    {
                        _animTree.SetCondition("rest", true);
                    }

                    break;
                case "Reload":
                    OnFinishedReload?.Invoke();
                    break;
                case "Holster":
                    if (_switchingGuns)
                    {
                        int nextItemIndex = (_curItemIndex + 1) % _items.Count;

                        _animTree.AnimPlayer = _items[nextItemIndex].AnimationPlayer.GetPath();
                        
                        _items[_curItemIndex].SetVisible(false);
                        
                        // If we do not wait for one frame then the wrong animation will play
                        // in the next frame creating a sort of visual glitch. This appears to
                        // be a bug with the Godot engine.
                        await this.WaitOneFrame();

                        RecreateCameraBone(nextItemIndex);
                        _items[nextItemIndex].SetVisible(true);

                        AnimationNodeStateMachinePlayback stateMachine = _animTree.GetStateMachine();
                        stateMachine.Start("Draw");

                        _curItemIndex = (_curItemIndex + 1) % _items.Count;
                    }

                    break;
            }
        };
    }

    private void OnPhysicsProcessAnimation()
    {
        // Mouse motion
        Quaternion camTarget = Quaternion.FromEuler(_cameraTarget);

        _camera.Position = Position + _camOffset;
        _camera.Quaternion = (camTarget * GetAnimationRotations()).Normalized();

        _fpsRig.Position = _camera.Position;
        _fpsRig.Quaternion = camTarget;

        if (Input.IsActionJustPressed(InputActions.Reload))
        {
            _animTree.SetCondition("reload", true);
        }

        if (Input.IsActionJustPressed(InputActions.Ads))
        {
            _animTree.SetCondition("ads", true);
        }

        if (Input.IsActionJustReleased(InputActions.Ads))
        {
            _animTree.SetCondition("rest", true);
        }

        if (Input.IsActionJustPressed(InputActions.Inspect))
        {
            _animTree.SetCondition("inspect", true);
        }
    }

    private Quaternion GetAnimationRotations()
    {
        // The camera bone
        Quaternion camBoneQuat = new(_cameraBone.Basis);

        // Account for annoying offset from the camera bone
        Quaternion offset = Quaternion.FromEuler(new Vector3(-Mathf.Pi / 2, -Mathf.Pi, 0));

        // The end result (multiplying order matters and always normalize to prevent errors)
        return (camBoneQuat * offset).Normalized();
    }

    // We have to re-create the camera bone everytime because if we do not, for some reason
    // Godot has decided to output an error if we try to set the external skeleton to
    // another skeleton
    private void RecreateCameraBone(int itemIndex)
    {
        if (GodotObject.IsInstanceValid(_cameraBone))
        {
            _cameraBone.QueueFree();
        }

        _cameraBone = new();
        _fpsRig.AddChild(_cameraBone);
        _cameraBone.BoneName = "Camera";

        _cameraBone.SetExternalSkeleton(_items[itemIndex].SkeletonRig.GetPath());

        // Will output an error if this is the same camera bone so that's why we re-create
        // it every time
        _cameraBone.SetUseExternalSkeleton(true);
    }
}
