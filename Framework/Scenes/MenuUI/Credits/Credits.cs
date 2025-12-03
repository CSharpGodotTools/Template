using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.UI;

public partial class Credits : Node
{
    // Constants
    private const string HeaderOneIdentifier = "[h1]";
    private const string HeaderTwoIdentifier = "[h2]";
    private const string LinkIdentifier = "http";
    private const string ForwardText = "F";
    private const string ReverseText = "R";
    private const string PauseText = "Pause";
    private const string ResumeText = "Resume";

    private const float StartingSpeed = 40;
    private const float SpeedBoostOffset = 60;

    private const int WhitespacePaddingSize = 10;
    private const int TextSize = 16;
    private const int HeaderOneSize = 32;
    private const int HeaderTwoSize = 24;
    private const int NumSpeedSettings = 3;

    // Fields
    private VBoxContainer _credits;
    private SceneManager _scene;
    private Vector2 _startingCreditsPosition;
    private Button _btnPause;
    private Button _btnSpeed;
    private Button _btnReverse;
    private float _speed = StartingSpeed;
    private bool _paused;
    private byte _curSpeedSetting = 1;
    private int _direction = 1;

    public override void _Ready()
    {
        SetupFields();
        BuildCredits("res://Credits.txt");
        PositionCredits();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.UICancel))
            _scene.SwitchToMainMenu();

        if (!_paused)
        {
            Vector2 position = _credits.Position;

            bool creditsAtStart = position.Y > _startingCreditsPosition.Y;
            bool creditsAtFinish = position.Y <= -_credits.Size.Y;
            bool isReverseDirection = _direction == -1;

            position.Y -= _speed * _direction * (float)delta;

            if (isReverseDirection && creditsAtStart)
            {
                position.Y = _startingCreditsPosition.Y;
            }

            _credits.Position = position;

            if (creditsAtFinish)
                _scene.SwitchToMainMenu();
        }
    }

    private void SetupFields()
    {
        _scene = Game.Scene;
        _btnPause = GetNode<Button>("%Pause");
        _btnSpeed = GetNode<Button>("%Speed");
        _btnReverse = GetNode<Button>("%Reverse");
    }

    private void BuildCredits(string filePath)
    {
        _credits = new VBoxContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        // Read the contents from credits.txt and construct the credits
        FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

        while (!file.EofReached())
        {
            string line = Tr(file.GetLine());
            ProcessLine(line);
        }

        file.Close();

        AddChild(_credits);
    }

    private void PositionCredits()
    {
        // Set starting position of the credits
        _credits.Position = new Vector2(
            GetViewport().GetVisibleRect().Size.X / 2 - _credits.Size.X / 2,
            GetViewport().GetVisibleRect().Size.Y);

        _startingCreditsPosition = _credits.Position;

        // Re-center credits when window size is changed
        /*GetViewport().SizeChanged += () =>
        {
            vbox.Position = new Vector2(
                DisplayServer.WindowGetSize().X / 2 - vbox.Size.X / 2,
                vbox.Size.Y);
        };*/
    }

    private void ProcessLine(string line)
    {
        int size = TextSize;

        if (line.Contains(HeaderOneIdentifier))
        {
            size = HeaderOneSize;
            line = line.Replace(HeaderOneIdentifier, "");
        }

        if (line.Contains(HeaderTwoIdentifier))
        {
            size = HeaderTwoSize;
            line = line.Replace(HeaderTwoIdentifier, "");
        }

        string trLine = string.Empty;

        foreach (string word in line.Split(' '))
            trLine += Tr(word) + " ";

        if (trLine.Contains(LinkIdentifier))
        {
            _credits.AddChild(GetHBoxTextWithLink(trLine));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(trLine))
            {
                Control padding = new()
                {
                    CustomMinimumSize = new Vector2(0, WhitespacePaddingSize),
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };

                _credits.AddChild(padding);
            }
            else
            {
                Label label = LabelFactory.Create(trLine, size);
                label.MouseFilter = Control.MouseFilterEnum.Ignore;

                _credits.AddChild(label);
            }
        }
    }

    private void _OnPausePressed()
    {
        _paused = !_paused;
        _btnPause.Text = _paused ? PauseText : ResumeText;
    }

    private void _OnSpeedPressed()
    {
        if (_curSpeedSetting < NumSpeedSettings)
        {
            _curSpeedSetting++;
            _btnSpeed.Text = $"{_curSpeedSetting}.0x";
            _speed += SpeedBoostOffset;
        }
        else
        {
            _curSpeedSetting = 1;
            _btnSpeed.Text = $"{_curSpeedSetting}.0x";
            _speed = StartingSpeed;
        }
    }

    private void _OnReversePressed()
    {
        _direction = -_direction;
        _btnReverse.Text = _direction > 0 ? ForwardText : ReverseText;
    }

    private static HBoxContainer GetHBoxTextWithLink(string text)
    {
        int indexOfHttp = text.IndexOf(LinkIdentifier, StringComparison.Ordinal);
        string textDesc = text.Substring(0, indexOfHttp);
        string textLink = text.Substring(indexOfHttp);
        
        HBoxContainer hbox = new()
        {
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };

        Label label = LabelFactory.Create(textDesc);
        LinkButton linkButton = LinkButtonFactory.Create(textLink);

        hbox.AddChild(linkButton);
        hbox.AddChild(label);
        return hbox;
    }
}
