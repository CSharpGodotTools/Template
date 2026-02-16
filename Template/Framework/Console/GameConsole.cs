using Godot;
using System;
using System.Collections.Generic;

namespace Framework.UI.Console;

public partial class GameConsole : Node
{
    private const int MaxTextFeed = 1000;

    private readonly List<ConsoleCommandInfo> _commands = [];
    private readonly ConsoleHistory _history = new();
    private readonly Queue<string> _pendingMessages = [];

    private PanelContainer _mainContainer;
    private PopupPanel _settingsPopup;
    private CheckBox _settingsAutoScroll;
    private TextEdit _feed;
    private LineEdit _input;
    private Button _settingsBtn;

    private bool _autoScroll = true;
    private bool _isReady;

    public bool Visible => _mainContainer.Visible;

    public override void _Ready()
    {
        CacheNodes();
        BindEvents();

        _mainContainer.Hide();
        _isReady = true;
        FlushPendingMessages();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.ToggleConsole))
        {
            ToggleVisibility();
            return;
        }

        HandleHistoryNavigation();
    }

    public override void _ExitTree()
    {
        UnbindEvents();
    }

    public List<ConsoleCommandInfo> GetCommands()
    {
        return [.. _commands];
    }

    public ConsoleCommandInfo RegisterCommand(string cmd, Action<string[]> code)
    {
        ConsoleCommandInfo info = new()
        {
            Name = cmd,
            Code = code
        };

        _commands.Add(info);
        return info;
    }

    public void AddMessage(object message)
    {
        string line = $"\n{message}";

        if (!_isReady)
        {
            _pendingMessages.Enqueue(line);
            return;
        }

        AppendMessage(line);
    }

    public void ToggleVisibility()
    {
        if (_mainContainer.Visible)
        {
            CloseConsole();
            return;
        }

        OpenConsole();
    }

    private void CacheNodes()
    {
        _feed = GetNode<TextEdit>("%Output");
        _input = GetNode<LineEdit>("%CmdsInput");
        _settingsBtn = GetNode<Button>("%Settings");
        _mainContainer = GetNode<PanelContainer>("%MainContainer");
        _settingsPopup = GetNode<PopupPanel>("%PopupPanel");

        _settingsAutoScroll = GetNode<CheckBox>("%PopupAutoScroll");
        _settingsAutoScroll.ButtonPressed = _autoScroll;
    }

    private void BindEvents()
    {
        _input.TextSubmitted += OnConsoleInputEntered;
        _settingsBtn.Pressed += OnSettingsBtnPressed;
        _settingsAutoScroll.Toggled += OnAutoScrollToggled;
    }

    private void UnbindEvents()
    {
        _input.TextSubmitted -= OnConsoleInputEntered;
        _settingsBtn.Pressed -= OnSettingsBtnPressed;
        _settingsAutoScroll.Toggled -= OnAutoScrollToggled;
    }

    private void FlushPendingMessages()
    {
        while (_pendingMessages.TryDequeue(out string message))
        {
            AppendMessage(message);
        }
    }

    private void AppendMessage(string line)
    {
        double previousScroll = _feed.ScrollVertical;
        _feed.Text += line;

        if (_feed.Text.Length > MaxTextFeed)
        {
            _feed.Text = _feed.Text[^MaxTextFeed..];
        }

        _feed.ScrollVertical = previousScroll;
        ScrollDownIfEnabled();
    }

    private void OpenConsole()
    {
        _mainContainer.Show();
        _input.GrabFocus();
        CallDeferred(nameof(ScrollDownIfEnabled));
    }

    private void CloseConsole()
    {
        _mainContainer.Hide();
        GameFramework.FocusOutline.ClearFocus();
    }

    private void ScrollDownIfEnabled()
    {
        if (_autoScroll)
        {
            _feed.ScrollVertical = (int)_feed.GetVScrollBar().MaxValue;
        }
    }

    private bool ProcessCommand(string text)
    {
        string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        string commandName = parts[0];
        if (!TryGetCommand(commandName, out ConsoleCommandInfo commandInfo))
        {
            GameFramework.Logger.Log($"The command '{commandName}' does not exist");
            return false;
        }

        int argCount = parts.Length - 1;
        string[] args = new string[argCount];
        for (int index = 0; index < argCount; index++)
        {
            args[index] = parts[index + 1];
        }

        commandInfo.Code.Invoke(args);
        return true;
    }

    private bool TryGetCommand(string text, out ConsoleCommandInfo commandInfo)
    {
        for (int commandIndex = 0; commandIndex < _commands.Count; commandIndex++)
        {
            ConsoleCommandInfo candidate = _commands[commandIndex];
            if (IsCommandMatch(candidate, text))
            {
                commandInfo = candidate;
                return true;
            }
        }

        commandInfo = default!;
        return false;
    }

    private static bool IsCommandMatch(ConsoleCommandInfo commandInfo, string text)
    {
        if (string.Equals(commandInfo.Name, text, StringComparison.OrdinalIgnoreCase))
            return true;

        string[] aliases = commandInfo.Aliases;
        for (int aliasIndex = 0; aliasIndex < aliases.Length; aliasIndex++)
        {
            if (string.Equals(aliases[aliasIndex], text, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private void HandleHistoryNavigation()
    {
        if (!_mainContainer.Visible || _history.NoHistory())
            return;

        if (Input.IsActionJustPressed(InputActions.UIUp))
        {
            string historyText = _history.MoveUpOne();
            ApplyHistoryText(historyText);
        }

        if (Input.IsActionJustPressed(InputActions.UIDown))
        {
            string historyText = _history.MoveDownOne();
            ApplyHistoryText(historyText);
        }
    }

    private void ApplyHistoryText(string historyText)
    {
        _input.Text = historyText;
        SetCaretColumn(historyText.Length);
    }

    private void OnSettingsBtnPressed()
    {
        if (!_settingsPopup.Visible)
        {
            _settingsPopup.PopupCentered();
        }
    }

    private void OnAutoScrollToggled(bool value)
    {
        _autoScroll = value;
    }

    private void OnConsoleInputEntered(string text)
    {
        string trimmedInput = text.Trim();
        if (string.IsNullOrWhiteSpace(trimmedInput))
            return;

        _history.Add(trimmedInput);
        ProcessCommand(trimmedInput);

        _input.Clear();
        CallDeferred(nameof(RefocusInput));
    }

    private void RefocusInput()
    {
        _input.Edit();
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
