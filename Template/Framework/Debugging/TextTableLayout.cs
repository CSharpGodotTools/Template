using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Debugging;

public sealed class TextTableLayout
{
    internal readonly struct ColumnDefinition
    {
        public ColumnDefinition(string header, Alignment alignment, int extraWidth)
        {
            Header = header;
            Alignment = alignment;
            ExtraWidth = extraWidth;
        }

        public string Header { get; }
        public Alignment Alignment { get; }
        public int ExtraWidth { get; }
    }

    private readonly List<ColumnDefinition> _columns = new();
    private readonly List<TextTable> _tables = new();
    private int[]? _sharedWidths;
    private bool _widthsDirty = true;

    public void AddColumn(string header, Alignment alignment = Alignment.Center, int extraWidth = 0)
    {
        if (header is null)
            throw new ArgumentNullException(nameof(header));
        if (extraWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(extraWidth), "extraWidth cannot be negative.");
        if (_tables.Count > 0)
            throw new InvalidOperationException("Cannot add columns after tables have been created.");

        _columns.Add(new ColumnDefinition(header, alignment, extraWidth));
        _widthsDirty = true;
    }

    public TextTable CreateTable()
    {
        TextTable table = new(this);
        _tables.Add(table);
        _widthsDirty = true;
        return table;
    }

    internal IReadOnlyList<ColumnDefinition> GetColumnDefinitions() => _columns;

    internal void InvalidateSharedWidths() => _widthsDirty = true;

    internal int[] GetSharedWidths()
    {
        if (!_widthsDirty && _sharedWidths is not null)
            return _sharedWidths;

        int columnCount = _columns.Count;
        int[] widths = new int[columnCount];

        for (int i = 0; i < columnCount; i++)
            widths[i] = GetHeaderWidth(_columns[i].Header);

        foreach (TextTable table in _tables)
        {
            int[] headerWidths = table.GetHeaderWidths();
            int[] naturalWidths = table.GetNaturalCellWidths();
            if (headerWidths.Length != columnCount || naturalWidths.Length != columnCount)
                continue;

            for (int i = 0; i < columnCount; i++)
            {
                if (headerWidths[i] > widths[i])
                    widths[i] = headerWidths[i];
                if (naturalWidths[i] > widths[i])
                    widths[i] = naturalWidths[i];
            }
        }

        for (int i = 0; i < columnCount; i++)
            widths[i] += _columns[i].ExtraWidth;

        _sharedWidths = widths;
        _widthsDirty = false;
        return widths;
    }

    private static int GetHeaderWidth(string header)
    {
        string label = " " + header + " ";
        return label.Length;
    }
}
