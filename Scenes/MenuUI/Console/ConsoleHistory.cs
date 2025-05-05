using System.Collections.Generic;

namespace Template.UI;

public class ConsoleHistory
{
    private readonly Dictionary<int, string> _inputHistory = [];
    private int _inputHistoryIndex;
    private int _inputHistoryNav;

    /// <summary>
    /// Add text to history
    /// </summary>
    public void Add(string text)
    {
        _inputHistory.Add(_inputHistoryIndex++, text);
        _inputHistoryNav = _inputHistoryIndex;
    }

    /// <summary>
    /// Move up one in history
    /// </summary>
    public string MoveUpOne()
    {
        _inputHistoryNav--;

        if (!_inputHistory.ContainsKey(_inputHistoryNav))
        {
            _inputHistoryNav++;
        }

        return Get(_inputHistoryNav);
    }

    /// <summary>
    /// Move down one in history
    /// </summary>
    public string MoveDownOne()
    {
        _inputHistoryNav++;

        if (!_inputHistory.ContainsKey(_inputHistoryNav))
        {
            _inputHistoryNav--;
        }

        return Get(_inputHistoryNav);
    }

    public bool NoHistory()
    {
        return _inputHistory.Count == 0;
    }

    public string Get(int nav)
    {
        return _inputHistory[nav];
    }
}

