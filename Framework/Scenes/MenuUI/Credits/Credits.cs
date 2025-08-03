using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.UI;

[SceneTree]
public partial class Credits : Node
{
    private const float STARTING_SPEED = 0.75f;

    private VBoxContainer _vbox;
    private Button _btnPause;
    private Button _btnSpeed;
    private bool _paused;
    private byte _curSpeedSetting = 1;
    private float _speed = STARTING_SPEED;

    public override void _Ready()
    {
        _btnPause = Pause;
        _btnSpeed = Speed;

        _vbox = new VBoxContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin
        };

        // Read the contents from credits.txt and construct the credits
        FileAccess file = FileAccess.Open("res://Credits.txt", FileAccess.ModeFlags.Read);

        while (!file.EofReached())
        {
            string line = Tr(file.GetLine());

            int size = 16;

            if (line.Contains("[h1]"))
            {
                size = 32;
                line = line.Replace("[h1]", "");
            }

            if (line.Contains("[h2]"))
            {
                size = 24;
                line = line.Replace("[h2]", "");
            }

            string translatedLine = string.Empty;

            foreach (string word in line.Split(' '))
            {
                translatedLine += Tr(word) + " ";
            }

            if (translatedLine.Contains("http"))
            {
                AddTextWithLink(translatedLine);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(translatedLine))
                {
                    Control padding = new()
                    {
                        CustomMinimumSize = new Vector2(0, 10),
                        MouseFilter = Control.MouseFilterEnum.Ignore
                    };

                    _vbox.AddChild(padding);
                }
                else
                {
                    Label label = LabelFactory.Create(translatedLine, size);
                    label.MouseFilter = Control.MouseFilterEnum.Ignore;

                    _vbox.AddChild(label);
                }
            }   
        } 

        file.Close();

        AddChild(_vbox);

        _vbox.MouseFilter = Control.MouseFilterEnum.Ignore;

        // Set starting position of the credits
        _vbox.Position = new Vector2(
            GetViewport().GetVisibleRect().Size.X / 2 - _vbox.Size.X / 2,
            GetViewport().GetVisibleRect().Size.Y);

        // Re-center credits when window size is changed
        /*GetViewport().SizeChanged += () =>
        {
            vbox.Position = new Vector2(
                DisplayServer.WindowGetSize().X / 2 - vbox.Size.X / 2,
                vbox.Size.Y);
        };*/
    }

    public override void _PhysicsProcess(double delta)
    {
        // Animate the credits
        Vector2 pos = _vbox.Position;
        pos.Y -= _speed;
        _vbox.Position = pos;

        // Go back to the main menu when the credits are finished
        if (pos.Y <= -_vbox.Size.Y)
        {
            SceneManager.SwitchScene(Scene.MainMenu);
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.UICancel))
        {
            SceneManager.SwitchScene(Scene.MainMenu);
        }
    }

    private void AddTextWithLink(string text)
    {
        int indexOfHttp = text.IndexOf("http", StringComparison.Ordinal);

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

        _vbox.AddChild(hbox);
    }

    private void _OnPausePressed()
    {
        _paused = !_paused;

        if (_paused)
        {
            SetPhysicsProcess(false);
            _btnPause.Text = "Resume";
        }
        else
        {
            SetPhysicsProcess(true);
            _btnPause.Text = "Pause";
        }
    }

    private void _OnSpeedPressed()
    {
        if (_curSpeedSetting < 3)
        {
            _curSpeedSetting++;
            _btnSpeed.Text = $"{_curSpeedSetting}.0x";
            _speed += 1;
        }
        else
        {
            _curSpeedSetting = 1;
            _btnSpeed.Text = $"{_curSpeedSetting}.0x";
            _speed = STARTING_SPEED;
        }
    }
}
