using System.Collections.Generic;

namespace Framework.UI.Console;

public class ConsoleHistory
{
    private readonly List<string> _inputHistory = [];
    private int _inputHistoryNav;

    /// <summary>
    /// Add text to history
    /// </summary>
    public void Add(string text)
    {
        _inputHistory.Add(text);
        _inputHistoryNav = _inputHistory.Count;
    }

    /// <summary>
    /// Move up one in history
    /// </summary>
    public string MoveUpOne()
    {
        if (_inputHistoryNav > 0)
        {
            _inputHistoryNav--;
        }

        return Get(_inputHistoryNav);
    }

    /// <summary>
    /// Move down one in history
    /// </summary>
    public string MoveDownOne()
    {
        if (_inputHistoryNav < _inputHistory.Count)
        {
            _inputHistoryNav++;
        }

        return Get(_inputHistoryNav);
    }

    public bool NoHistory()
    {
        return _inputHistory.Count == 0;
    }

    public string Get(int nav)
    {
        if (nav == _inputHistory.Count)
        {
            return string.Empty;
        }

        return _inputHistory[nav];
    }
}
