using Godot;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui.Console;

/// <summary>
/// Provides an in-game command console with command registration, output feed, and input history.
/// </summary>
public partial class GameConsole : Node, ISceneDependencyReceiver
{
    private const int MaxTextFeed = 1000;

    private readonly List<ConsoleCommandInfo> _commands = [];
    private readonly ConsoleHistory _history = new();
    private readonly Queue<string> _pendingMessages = [];

    private PanelContainer _mainContainer = null!;
    private PopupPanel _settingsPopup = null!;
    private CheckBox _settingsAutoScroll = null!;
    private TextEdit _feed = null!;
    private LineEdit _input = null!;
    private Button _settingsBtn = null!;
    private FocusOutlineManager _focusOutline = null!;
    private ILoggerService _logger = null!;

    private bool _autoScroll = true;
    private bool _isReady;
    private bool _isConfigured;

    /// <summary>
    /// Injects framework services required by the console.
    /// </summary>
    /// <param name="services">Runtime service bundle from the framework.</param>
    public void Configure(GameServices services)
    {
        _focusOutline = services.FocusOutline;
        _logger = services.Logger;
        _isConfigured = true;
    }

    /// <summary>
    /// Gets whether the console panel is currently visible.
    /// </summary>
    public bool Visible => _mainContainer.Visible;

    /// <inheritdoc />
    public override void _EnterTree()
    {
        SceneComposition.ConfigureNodeFromGame(this);
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        // Ensure runtime services were provided before node initialization.
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(GameConsole)} was not configured before _Ready.");

        CacheNodes();
        BindEvents();

        _mainContainer.Hide();
        _isReady = true;
        FlushPendingMessages();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        // Toggle console visibility when the configured shortcut is pressed.
        if (Input.IsActionJustPressed(InputActions.ToggleConsole))
        {
            ToggleVisibility();
            return;
        }

        HandleHistoryNavigation();
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        UnbindEvents();
    }

    /// <summary>
    /// Gets a snapshot of all registered console commands.
    /// </summary>
    /// <returns>A copy of the command list.</returns>
    public List<ConsoleCommandInfo> GetCommands()
    {
        return [.. _commands];
    }

    /// <summary>
    /// Registers a command handler and returns its metadata for optional alias configuration.
    /// </summary>
    /// <param name="cmd">Primary command name.</param>
    /// <param name="code">Delegate executed when the command is invoked.</param>
    /// <returns>The created command metadata object.</returns>
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

    /// <summary>
    /// Appends a message to the output feed, or buffers it until the console is ready.
    /// </summary>
    /// <param name="message">Message object to render in the feed.</param>
    public void AddMessage(object message)
    {
        string line = $"\n{message}";

        // Queue messages until controls are initialized and ready to render text.
        if (!_isReady)
        {
            _pendingMessages.Enqueue(line);
            return;
        }

        AppendMessage(line);
    }

    /// <summary>
    /// Toggles console visibility and focus handling.
    /// </summary>
    public void ToggleVisibility()
    {
        // Close console when already open.
        if (_mainContainer.Visible)
        {
            CloseConsole();
            return;
        }

        OpenConsole();
    }

    /// <summary>
    /// Resolves and caches required console scene nodes.
    /// </summary>
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

    /// <summary>
    /// Subscribes UI events used by the console input and settings controls.
    /// </summary>
    private void BindEvents()
    {
        _input.TextSubmitted += OnConsoleInputEntered;
        _settingsBtn.Pressed += OnSettingsBtnPressed;
        _settingsAutoScroll.Toggled += OnAutoScrollToggled;
    }

    /// <summary>
    /// Unsubscribes UI events used by the console input and settings controls.
    /// </summary>
    private void UnbindEvents()
    {
        _input.TextSubmitted -= OnConsoleInputEntered;
        _settingsBtn.Pressed -= OnSettingsBtnPressed;
        _settingsAutoScroll.Toggled -= OnAutoScrollToggled;
    }

    /// <summary>
    /// Flushes messages queued before the console finished initialization.
    /// </summary>
    private void FlushPendingMessages()
    {
        while (_pendingMessages.TryDequeue(out string? message))
        {
            AppendMessage(message);
        }
    }

    /// <summary>
    /// Appends one line to the feed and enforces the configured text-length cap.
    /// </summary>
    /// <param name="line">Line to append.</param>
    private void AppendMessage(string line)
    {
        double previousScroll = _feed.ScrollVertical;
        _feed.Text += line;

        // Keep feed size bounded to prevent unbounded text growth.
        if (_feed.Text.Length > MaxTextFeed)
        {
            _feed.Text = _feed.Text[^MaxTextFeed..];
        }

        _feed.ScrollVertical = previousScroll;
        ScrollDownIfEnabled();
    }

    /// <summary>
    /// Opens the console panel and focuses the command input.
    /// </summary>
    private void OpenConsole()
    {
        _mainContainer.Show();
        _input.GrabFocus();
        CallDeferred(nameof(ScrollDownIfEnabled));
    }

    /// <summary>
    /// Closes the console panel and clears focus outline state.
    /// </summary>
    private void CloseConsole()
    {
        _mainContainer.Hide();
        _focusOutline.ClearFocus();
    }

    /// <summary>
    /// Scrolls the output feed to the bottom when auto-scroll is enabled.
    /// </summary>
    private void ScrollDownIfEnabled()
    {
        // Auto-scroll feed only when the setting is enabled.
        if (_autoScroll)
        {
            _feed.ScrollVertical = (int)_feed.GetVScrollBar().MaxValue;
        }
    }

    /// <summary>
    /// Parses and executes a console command line.
    /// </summary>
    /// <param name="text">Raw command line input.</param>
    /// <returns><see langword="true"/> when a matching command was executed.</returns>
    private bool ProcessCommand(string text)
    {
        string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Ignore empty command input after splitting.
        if (parts.Length == 0)
            return false;

        string commandName = parts[0];

        // Report unknown commands and stop processing.
        if (!TryGetCommand(commandName, out ConsoleCommandInfo commandInfo))
        {
            _logger.Log($"The command '{commandName}' does not exist");
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

    /// <summary>
    /// Resolves command metadata by matching command name or aliases.
    /// </summary>
    /// <param name="text">Command token to resolve.</param>
    /// <param name="commandInfo">Resolved command metadata when found.</param>
    /// <returns><see langword="true"/> when a command match is found.</returns>
    private bool TryGetCommand(string text, out ConsoleCommandInfo commandInfo)
    {
        for (int commandIndex = 0; commandIndex < _commands.Count; commandIndex++)
        {
            ConsoleCommandInfo candidate = _commands[commandIndex];

            // Return first command whose name or alias matches input text.
            if (IsCommandMatch(candidate, text))
            {
                commandInfo = candidate;
                return true;
            }
        }

        commandInfo = default!;
        return false;
    }

    /// <summary>
    /// Tests whether an input token matches a command name or any alias.
    /// </summary>
    /// <param name="commandInfo">Command metadata to evaluate.</param>
    /// <param name="text">Input token to compare.</param>
    /// <returns><see langword="true"/> when the token matches the command.</returns>
    private static bool IsCommandMatch(ConsoleCommandInfo commandInfo, string text)
    {
        // Match primary command name case-insensitively.
        if (string.Equals(commandInfo.Name, text, StringComparison.OrdinalIgnoreCase))
            return true;

        string[] aliases = commandInfo.Aliases;
        for (int aliasIndex = 0; aliasIndex < aliases.Length; aliasIndex++)
        {
            // Match any configured alias case-insensitively.
            if (string.Equals(aliases[aliasIndex], text, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Handles keyboard history navigation while the console is visible.
    /// </summary>
    private void HandleHistoryNavigation()
    {
        // Skip history navigation when console is hidden or history is empty.
        if (!_mainContainer.Visible || _history.NoHistory())
            return;

        // Navigate to previous command entry.
        if (Input.IsActionJustPressed(InputActions.UIUp))
        {
            string historyText = _history.MoveUpOne();
            ApplyHistoryText(historyText);
        }

        // Navigate to next command entry.
        if (Input.IsActionJustPressed(InputActions.UIDown))
        {
            string historyText = _history.MoveDownOne();
            ApplyHistoryText(historyText);
        }
    }

    /// <summary>
    /// Applies a history item to the input field and updates caret position.
    /// </summary>
    /// <param name="historyText">History entry text.</param>
    private void ApplyHistoryText(string historyText)
    {
        _input.Text = historyText;
        SetCaretColumn(historyText.Length);
    }

    /// <summary>
    /// Opens the settings popup from the settings button.
    /// </summary>
    private void OnSettingsBtnPressed()
    {
        // Open settings popup only when it is currently hidden.
        if (!_settingsPopup.Visible)
        {
            _settingsPopup.PopupCentered();
        }
    }

    /// <summary>
    /// Updates the auto-scroll preference when the toggle changes.
    /// </summary>
    /// <param name="value">New auto-scroll value.</param>
    private void OnAutoScrollToggled(bool value)
    {
        _autoScroll = value;
    }

    /// <summary>
    /// Processes submitted console input, stores history, and executes matched commands.
    /// </summary>
    /// <param name="text">Submitted input text.</param>
    private void OnConsoleInputEntered(string text)
    {
        string trimmedInput = text.Trim();

        // Ignore blank submissions.
        if (string.IsNullOrWhiteSpace(trimmedInput))
            return;

        _history.Add(trimmedInput);
        ProcessCommand(trimmedInput);

        _input.Clear();
        CallDeferred(nameof(RefocusInput));
    }

    /// <summary>
    /// Restores text-edit mode and focus to the command input field.
    /// </summary>
    private void RefocusInput()
    {
        _input.Edit();
        _input.GrabFocus();
        _input.CaretColumn = _input.Text.Length;
    }

    /// <summary>
    /// Defers caret placement after updating input text.
    /// </summary>
    /// <param name="pos">Caret column position.</param>
    private void SetCaretColumn(int pos)
    {
        _input.CallDeferred(Control.MethodName.GrabFocus);
        _input.CallDeferred(GodotObject.MethodName.Set, LineEdit.PropertyName.CaretColumn, pos);
    }
}

/// <summary>
/// Extension helpers for fluently configuring console command metadata.
/// </summary>
public static class ConsoleCommandInfoExtensions
{
    /// <summary>
    /// Assigns aliases to a command and returns the same command for chaining.
    /// </summary>
    /// <param name="cmdInfo">Command metadata to update.</param>
    /// <param name="aliases">Alias list to assign.</param>
    /// <returns>The original command metadata instance.</returns>
    public static ConsoleCommandInfo WithAliases(this ConsoleCommandInfo cmdInfo, params string[] aliases)
    {
        cmdInfo.Aliases = aliases;
        return cmdInfo;
    }
}
