using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Displays and controls the scrolling credits scene.
/// </summary>
public partial class Credits : Node, ISceneDependencyReceiver
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

    private const int PaddingBetweenLines = 10;
    private const int TextSize = 16;
    private const int HeaderOneSize = 32;
    private const int HeaderTwoSize = 24;
    private const int NumSpeedSettings = 3;

    // Fields
    private VBoxContainer _credits = null!;
    private SceneManager _scene = null!;
    private Vector2 _startingCreditsPosition;
    private Button _btnPause = null!;
    private Button _btnSpeed = null!;
    private Button _btnReverse = null!;
    private float _speed = StartingSpeed;
    private bool _paused;
    private byte _curSpeedSetting = 1;
    private int _direction = 1;
    private bool _isConfigured;

    /// <summary>
    /// Injects runtime dependencies required by the credits scene.
    /// </summary>
    /// <param name="services">Resolved game service bundle.</param>
    public void Configure(GameServices services)
    {
        _scene = services.SceneManager;
        _isConfigured = true;
    }

    public override void _EnterTree()
    {
        SceneComposition.ConfigureNodeFromGame(this);
    }

    // Godot Overrides
    public override void _Ready()
    {
        SetupFields();
        BuildCredits("res://Credits.txt");
        PositionCredits();
    }

    public override void _Process(double delta)
    {
        // Allow cancel input to exit back to main menu immediately.
        if (Input.IsActionJustPressed(InputActions.UICancel))
            _scene.SwitchToMainMenu();

        // Advance scrolling only while playback is not paused.
        if (!_paused)
        {
            Vector2 position = _credits.Position;

            bool creditsAtStart = position.Y > _startingCreditsPosition.Y;
            bool creditsAtFinish = position.Y <= -_credits.Size.Y;
            bool isReverseDirection = _direction == -1;

            position.Y -= _speed * _direction * (float)delta;

            // Clamp reverse scrolling so credits do not move above start position.
            if (isReverseDirection && creditsAtStart)
            {
                position.Y = _startingCreditsPosition.Y;
            }

            _credits.Position = position;

            // Return to main menu when credits reach the end.
            if (creditsAtFinish)
                _scene.SwitchToMainMenu();
        }
    }

    // Private Methods
    /// <summary>
    /// Resolves required node references and validates dependency configuration.
    /// </summary>
    private void SetupFields()
    {
        // Fail fast when dependencies were not configured before initialization.
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(Credits)} was not configured before _Ready.");

        _btnPause = GetNode<Button>("%Pause");
        _btnSpeed = GetNode<Button>("%Speed");
        _btnReverse = GetNode<Button>("%Reverse");
    }

    /// <summary>
    /// Loads and builds all credits lines from a text source file.
    /// </summary>
    /// <param name="filePath">Path to the credits text file.</param>
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

    /// <summary>
    /// Positions the credits container at the bottom-center start location.
    /// </summary>
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

    /// <summary>
    /// Parses one credits line and appends the corresponding control.
    /// </summary>
    /// <param name="line">Raw line from the credits source file.</param>
    private void ProcessLine(string line)
    {
        int size = TextSize;

        // Promote first-level headers to larger typography.
        if (line.Contains(HeaderOneIdentifier))
        {
            size = HeaderOneSize;
            line = line.Replace(HeaderOneIdentifier, "");
        }

        // Promote second-level headers to medium typography.
        if (line.Contains(HeaderTwoIdentifier))
        {
            size = HeaderTwoSize;
            line = line.Replace(HeaderTwoIdentifier, "");
        }

        string trLine = string.Empty;

        foreach (string word in line.Split(' '))
            trLine += Tr(word) + " ";

        // Render lines containing URLs with dedicated link controls.
        if (trLine.Contains(LinkIdentifier))
        {
            _credits.AddChild(GetHBoxTextWithLink(trLine));
        }
        else
        {
            // Insert vertical spacing for blank lines.
            if (string.IsNullOrWhiteSpace(trLine))
            {
                Control paddingBetweenLines = new()
                {
                    CustomMinimumSize = new Vector2(0, PaddingBetweenLines),
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };

                _credits.AddChild(paddingBetweenLines);
            }
            else
            {
                Label label = LabelFactory.Create(trLine, size);
                label.MouseFilter = Control.MouseFilterEnum.Ignore;

                _credits.AddChild(label);
            }
        }
    }

    /// <summary>
    /// Builds a horizontal row where detected URL text is rendered as a link.
    /// </summary>
    /// <param name="text">Line text containing an inline URL.</param>
    /// <returns>Container with label/link/label segments in original order.</returns>
    private static HBoxContainer GetHBoxTextWithLink(string text)
    {
        // Find the start of the URL
        int startIndex = text.IndexOf(LinkIdentifier, StringComparison.Ordinal);

        // Fall back to plain label row when no URL marker is present.
        if (startIndex < 0)
        {
            HBoxContainer fallback = new();
            Label fallbackLabel = LabelFactory.Create(text);
            fallback.AddChild(fallbackLabel);
            return fallback;
        }

        // Extract exact URL: it ends at next whitespace or end of line
        int endIndex = text.IndexOf(' ', startIndex);

        // Extend URL to line end when no trailing whitespace exists.
        if (endIndex < 0)
            endIndex = text.Length;

        string leftText = text[..startIndex];
        string url = text[startIndex..endIndex];
        string rightText = text[endIndex..];

        HBoxContainer hbox = new()
        {
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };

        hbox.AddThemeConstantOverride("separation", 0);

        // Preserve order: LEFT -> URL -> RIGHT
        if (!string.IsNullOrWhiteSpace(leftText))
            hbox.AddChild(LabelFactory.Create(leftText));

        hbox.AddChild(LinkButtonFactory.Create(url));

        // Append trailing non-link text when present.
        if (!string.IsNullOrWhiteSpace(rightText))
            hbox.AddChild(LabelFactory.Create(rightText));

        return hbox;
    }

    /// <summary>
    /// Toggles credits scrolling pause state.
    /// </summary>
    private void OnPausePressed()
    {
        _paused = !_paused;
        _btnPause.Text = _paused ? ResumeText : PauseText;
    }

    /// <summary>
    /// Cycles through predefined scroll speed multipliers.
    /// </summary>
    private void OnSpeedPressed()
    {
        // Step through predefined speed multipliers before wrapping to baseline.
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

    /// <summary>
    /// Toggles credits scroll direction and updates button label.
    /// </summary>
    private void OnReversePressed()
    {
        _direction = -_direction;
        _btnReverse.Text = _direction > 0 ? ForwardText : ReverseText;
    }
}
