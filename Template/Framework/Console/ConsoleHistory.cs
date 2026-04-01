using System.Collections.Generic;

namespace __TEMPLATE__.Ui.Console;

/// <summary>
/// Stores and navigates entered console input history.
/// </summary>
public class ConsoleHistory
{
    private readonly List<string> _inputHistory = [];
    private int _inputHistoryNav;

    /// <summary>
    /// Add text to history
    /// </summary>
    /// <param name="text">Console input text to append to history.</param>
    public void Add(string text)
    {
        _inputHistory.Add(text);
        _inputHistoryNav = _inputHistory.Count;
    }

    /// <summary>
    /// Move up one in history
    /// </summary>
    /// <returns>The selected history entry after moving up, or an empty string when unavailable.</returns>
    public string MoveUpOne()
    {
        // Move upward only when not already at the oldest entry.
        if (_inputHistoryNav > 0)
        {
            _inputHistoryNav--;
        }

        return Get(_inputHistoryNav);
    }

    /// <summary>
    /// Move down one in history
    /// </summary>
    /// <returns>The selected history entry after moving down, or an empty string when unavailable.</returns>
    public string MoveDownOne()
    {
        // Move downward only while there are newer entries available.
        if (_inputHistoryNav < _inputHistory.Count)
        {
            _inputHistoryNav++;
        }

        return Get(_inputHistoryNav);
    }

    /// <summary>
    /// Returns whether no history entries are available.
    /// </summary>
    /// <returns><see langword="true"/> when history is empty; otherwise <see langword="false"/>.</returns>
    public bool NoHistory()
    {
        return _inputHistory.Count == 0;
    }

    /// <summary>
    /// Gets a history entry at the specified index.
    /// </summary>
    /// <param name="nav">Zero-based history index.</param>
    /// <returns>The history entry or an empty string when out of range.</returns>
    public string Get(int nav)
    {
        // Return empty text when requested index is outside history bounds.
        if (nav < 0 || nav >= _inputHistory.Count)
        {
            return string.Empty;
        }

        return _inputHistory[nav];
    }
}
