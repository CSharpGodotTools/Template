using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotUtils.UI.Console;

public partial class GameConsole : Node
{
    private const int MaxTextFeed = 1000;

    public static GameConsole Instance { get; private set; }

    private List<ConsoleCommandInfo> _commands = [];
    private ConsoleHistory           _history = new();
    private PanelContainer           _mainContainer;
    private PopupPanel               _settingsPopup;
    private CheckBox                 _settingsAutoScroll;
    private TextEdit                 _feed;
    private LineEdit                 _input;
    private Button                   _settingsBtn;
    private bool                     _autoScroll = true;

    public override void _Ready()
    {
        if (Instance != null)
            throw new InvalidOperationException($"{nameof(GameConsole)} was initialized already");

        Instance = this;

        _feed          = GetNode<TextEdit>("%Output");
        _input         = GetNode<LineEdit>("%CmdsInput");
        _settingsBtn   = GetNode<Button>("%Settings");
        _mainContainer = GetNode<PanelContainer>("%MainContainer");
        _settingsPopup = GetNode<PopupPanel>("%PopupPanel");

        _settingsAutoScroll = GetNode<CheckBox>("%PopupAutoScroll");
        _settingsAutoScroll.ButtonPressed = _autoScroll;

        _input.TextSubmitted += OnConsoleInputEntered;
        _settingsBtn.Pressed += OnSettingsBtnPressed;
        _settingsAutoScroll.Toggled += OnAutoScrollToggeled;

        _mainContainer.Hide();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.ToggleConsole))
        {
            ToggleVisibility();
            return;
        }

        InputNavigateHistory();
    }

    public override void _ExitTree()
    {
        _input.TextSubmitted -= OnConsoleInputEntered;
        _settingsBtn.Pressed -= OnSettingsBtnPressed;
        _settingsAutoScroll.Toggled -= OnAutoScrollToggeled;

        Instance = null;
    }

    public List<ConsoleCommandInfo> GetCommands()
    {
        return _commands;
    }

    public static ConsoleCommandInfo RegisterCommand(string cmd, Action<string[]> code)
    {
        ConsoleCommandInfo info = new()
        {
            Name = cmd,
            Code = code
        };

        Instance._commands.Add(info);

        return info;
    }

    public void AddMessage(object message)
    {
        double prevScroll = _feed.ScrollVertical;
        
        // Prevent text feed from becoming too large
        if (_feed.Text.Length > MaxTextFeed)
        {
            // If there are say 2353 characters then 2353 - 1000 = 1353 characters
            // which is how many characters we need to remove to get back down to
            // 1000 characters
            _feed.Text = _feed.Text.Remove(0, _feed.Text.Length - MaxTextFeed);
        }

        _feed.Text += $"\n{message}";

        // Removing text from the feed will mess up the scroll, this is why the
        // scroll value was stored previous, we set this to that value now to fix
        // this
        _feed.ScrollVertical = prevScroll;

        // Autoscroll if enabled
        ScrollDown();
    }

    public static bool Visible => Instance._mainContainer.Visible;

    public static void ToggleVisibility()
    {
        Instance._mainContainer.Visible = !Instance._mainContainer.Visible;

        if (Instance._mainContainer.Visible)
        {
            Instance._input.GrabFocus();
            Instance.CallDeferred(nameof(ScrollDown));
        }
    }

    private void ScrollDown()
    {
        if (_autoScroll)
        {
            _feed.ScrollVertical = (int)_feed.GetVScrollBar().MaxValue;
        }
    }

    private bool ProcessCommand(string text)
    {
        string[] parts = text.ToLower().Split();
        string cmd = parts[0];

        ConsoleCommandInfo cmdInfo = TryGetCommand(cmd);

        if (cmdInfo == null)
        {
            Logger.Log($"The command '{cmd}' does not exist");
            return false;
        }

        string[] args = [.. parts.Skip(1)];

        cmdInfo.Code.Invoke(args);

        return true;
    }

    private ConsoleCommandInfo TryGetCommand(string text)
    {
        ConsoleCommandInfo cmd = _commands.Find(IsMatchingCommand);

        return cmd;

        bool IsMatchingCommand(ConsoleCommandInfo cmd)
        {
            if (string.Equals(cmd.Name, text, StringComparison.OrdinalIgnoreCase))
                return true;

            return cmd.Aliases != null && cmd.Aliases.Any(alias => string.Equals(alias, text, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void InputNavigateHistory()
    {
        // If console is not visible or there is no history to navigate do nothing
        if (!_mainContainer.Visible || _history.NoHistory())
            return;

        if (Input.IsActionJustPressed(InputActions.UIUp))
        {
            string historyText = _history.MoveUpOne();

            _input.Text = historyText;

            // if deferred is not used then something else will override these settings
            SetCaretColumn(historyText.Length);
        }

        if (Input.IsActionJustPressed(InputActions.UIDown))
        {
            string historyText = _history.MoveDownOne();

            _input.Text = historyText;

            // if deferred is not used then something else will override these settings
            SetCaretColumn(historyText.Length);
        }
    }

    private void OnSettingsBtnPressed()
    {
        if (!_settingsPopup.Visible)
        {
            _settingsPopup.PopupCentered();
        }
    }

    private void OnAutoScrollToggeled(bool value)
    {
        _autoScroll = value;
    }

    private void OnConsoleInputEntered(string text)
    {
        // case sensitivity and trailing spaces should not factor in here
        string inputToLowerTrimmed = text.Trim().ToLower();
        string[] inputArr = inputToLowerTrimmed.Split(' ');

        // extract command from input
        string cmd = inputArr[0];

        // do not do anything if cmd is just whitespace
        if (string.IsNullOrWhiteSpace(cmd))
            return;

        // keep track of input history
        _history.Add(inputToLowerTrimmed);

        // process the command
        ProcessCommand(text);

        // clear the input after the command is executed
        _input.Clear();

        CallDeferred(nameof(RefocusInput));
    }

    // Put focus back on the input and move caret to end so user can type immediately
    private void RefocusInput()
    {
        _input.Edit(); // MUST do this otherwise refocus on LineEdit will NOT work
        _input.GrabFocus();
        _input.CaretColumn = _input.Text.Length;
    }

    private void SetCaretColumn(int pos)
    {
        _input.CallDeferred(Control.MethodName.GrabFocus);
        _input.CallDeferred(GodotObject.MethodName.Set, LineEdit.PropertyName.CaretColumn, pos);
    }
}

public static class ConsoleCommandInfoExtensions
{
    public static ConsoleCommandInfo WithAliases(this ConsoleCommandInfo cmdInfo, params string[] aliases)
    {
        cmdInfo.Aliases = aliases;
        return cmdInfo;
    }
}
