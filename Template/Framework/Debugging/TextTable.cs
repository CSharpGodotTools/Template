using System;
using System.Collections.Generic;
using System.Text;

namespace __TEMPLATE__.Debugging;

public enum Alignment
{
    Left,
    Center,
    Right
}

public sealed class TextTable
{
    private sealed class Column
    {
        public Column(string header, Alignment alignment, int extraWidth)
        {
            Header = header;
            Alignment = alignment;
            ExtraWidth = extraWidth;
        }

        public string Header { get; private set; }
        public Alignment Alignment { get; }
        public int ExtraWidth { get; }

        public void SetHeader(string header) => Header = header;
    }

    private readonly List<Column> _columns = new();
    private readonly List<string[]> _rows = new();
    private readonly TextTableLayout? _layout;

    public TextTable()
    {
    }

    internal TextTable(TextTableLayout layout)
    {
        if (layout is null)
            throw new ArgumentNullException(nameof(layout));

        _layout = layout;
        foreach (TextTableLayout.ColumnDefinition definition in layout.GetColumnDefinitions())
            _columns.Add(new Column(definition.Header, definition.Alignment, definition.ExtraWidth));
    }

    public void AddColumn(string header, Alignment alignment = Alignment.Center, int extraWidth = 0)
    {
        if (_layout is not null)
            throw new InvalidOperationException("Cannot add columns to a table created from a layout.");
        if (header is null)
            throw new ArgumentNullException(nameof(header));
        if (extraWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(extraWidth), "extraWidth cannot be negative.");

        _columns.Add(new Column(header, alignment, extraWidth));
    }

    public void AddRow(params string[] cells)
    {
        if (cells is null)
            throw new ArgumentNullException(nameof(cells));
        if (cells.Length != _columns.Count)
            throw new ArgumentException("Number of cells must match the number of columns.", nameof(cells));

        string[] normalized = new string[cells.Length];
        for (int i = 0; i < cells.Length; i++)
            normalized[i] = cells[i] ?? string.Empty;

        _rows.Add(normalized);
        _layout?.InvalidateSharedWidths();
    }

    internal void SetColumnHeader(int columnIndex, string header)
    {
        if (header is null)
            throw new ArgumentNullException(nameof(header));
        if (columnIndex < 0 || columnIndex >= _columns.Count)
            throw new ArgumentOutOfRangeException(nameof(columnIndex));

        _columns[columnIndex].SetHeader(header);
        _layout?.InvalidateSharedWidths();
    }

    public string Render()
    {
        if (_columns.Count == 0)
            return string.Empty;

        int[] widths = _layout?.GetSharedWidths() ?? CalculateColumnWidths();
        if (_layout is not null)
            return RenderTable(widths);

        return _columns.Count == 1 ? RenderPanel() : RenderTable(widths);
    }

    private string RenderTable(int[] widths)
    {
        StringBuilder sb = new();

        List<string> headerCells = new(_columns.Count);
        for (int i = 0; i < _columns.Count; i++)
            headerCells.Add(BuildHeaderCell(_columns[i].Header, widths[i], _columns[i].Alignment));
        AppendRow(sb, "┌", "┬", "┐", headerCells);

        foreach (string[] row in _rows)
        {
            List<string> valueCells = new(_columns.Count);
            for (int i = 0; i < _columns.Count; i++)
                valueCells.Add(BuildValueCell(row[i], widths[i], _columns[i].Alignment));
            AppendRow(sb, "│", "│", "│", valueCells);
        }

        List<string> footerCells = new(_columns.Count);
        for (int i = 0; i < _columns.Count; i++)
            footerCells.Add(BuildSeparatorCell(widths[i]));
        AppendRow(sb, "└", "┴", "┘", footerCells);

        return sb.ToString();
    }

    private string RenderPanel()
    {
        Column column = _columns[0];
        int maxContentWidth = 0;

        foreach (string[] row in _rows)
        {
            string value = row.Length > 0 ? row[0] ?? string.Empty : string.Empty;
            if (value.Length > maxContentWidth)
                maxContentWidth = value.Length;
        }

        string title = " " + column.Header + " ";
        int minBoxWidth = 7 + title.Length; // "┌── " + title + "──┐"
        int boxWidth = Math.Max(maxContentWidth + 4, minBoxWidth) + column.ExtraWidth;
        if (boxWidth < 2)
            boxWidth = 2;

        int dashesAfterTitle = boxWidth - 5 - title.Length;
        if (dashesAfterTitle < 2)
            dashesAfterTitle = 2;

        StringBuilder sb = new();
        sb.AppendLine("┌── " + title + new string('─', dashesAfterTitle) + "┐");

        int columnWidth = Math.Max(0, boxWidth - 2);
        foreach (string[] row in _rows)
        {
            string value = row.Length > 0 ? row[0] ?? string.Empty : string.Empty;
            string cell = BuildValueCell(value, columnWidth, column.Alignment);
            sb.Append('│');
            sb.Append(cell);
            sb.AppendLine("│");
        }

        sb.AppendLine("└" + new string('─', boxWidth - 2) + "┘");
        return sb.ToString();
    }

    private int[] CalculateColumnWidths()
    {
        int[] widths = new int[_columns.Count];

        for (int i = 0; i < _columns.Count; i++)
        {
            Column column = _columns[i];
            int width = GetHeaderWidth(column.Header);

            foreach (string[] row in _rows)
            {
                string value = row[i] ?? string.Empty;
                int required = value.Length + 2;
                if (required > width)
                    width = required;
            }

            widths[i] = width + column.ExtraWidth;
        }

        return widths;
    }

    internal int[] GetNaturalCellWidths()
    {
        int[] widths = new int[_columns.Count];

        for (int i = 0; i < _columns.Count; i++)
        {
            int maxValueWidth = 0;
            foreach (string[] row in _rows)
            {
                string value = row[i] ?? string.Empty;
                int required = value.Length + 2;
                if (required > maxValueWidth)
                    maxValueWidth = required;
            }

            widths[i] = maxValueWidth;
        }

        return widths;
    }

    internal int[] GetHeaderWidths()
    {
        int[] widths = new int[_columns.Count];

        for (int i = 0; i < _columns.Count; i++)
            widths[i] = GetHeaderWidth(_columns[i].Header);

        return widths;
    }

    private static int GetHeaderWidth(string header)
    {
        string label = " " + header + " ";
        return label.Length;
    }

    private static void AppendRow(StringBuilder sb, string left, string middle, string right, IReadOnlyList<string> cells)
    {
        sb.Append(left);
        for (int i = 0; i < cells.Count; i++)
        {
            sb.Append(cells[i]);
            sb.Append(i == cells.Count - 1 ? right : middle);
        }
        sb.AppendLine();
    }

    private static string BuildHeaderCell(string title, int width, Alignment alignment)
    {
        if (width <= 0)
            return string.Empty;

        string label = " " + title + " ";
        if (label.Length >= width)
            return label.Length > width ? label[..width] : label;

        int dashCount = width - label.Length;
        return alignment switch
        {
            Alignment.Left => label + new string('─', dashCount),
            Alignment.Right => new string('─', dashCount) + label,
            _ => new string('─', dashCount / 2) + label + new string('─', dashCount - (dashCount / 2))
        };
    }

    private static string BuildSeparatorCell(int width) => width > 0 ? new string('─', width) : string.Empty;

    private static string BuildValueCell(string value, int width, Alignment alignment)
    {
        if (width <= 0)
            return string.Empty;

        int innerWidth = Math.Max(0, width - 2);
        string trimmed = value.Length > innerWidth ? value[..innerWidth] : value;

        string aligned = alignment switch
        {
            Alignment.Left => trimmed.PadRight(innerWidth),
            Alignment.Right => trimmed.PadLeft(innerWidth),
            _ => trimmed.PadLeft((innerWidth + trimmed.Length) / 2).PadRight(innerWidth)
        };

        return " " + aligned + " ";
    }
}
