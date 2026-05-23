using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace __TEMPLATE__.Debugging;

/// <summary>
/// Profiling helper that logs timings per call site.
/// </summary>
public sealed class Profiler : IProfiler
{
    private readonly Dictionary<string, Entry> _entries = [];
    
    public void Begin(string id = "",
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "")
    {
        string internalId = id + filePath + methodName;
        if (!_entries.TryGetValue(internalId, out Entry? entry))
        {
            Entry newEntry = new(filePath, methodName, id);
            _entries[internalId] = newEntry;
            newEntry.Restart();
            return;
        }
        entry.Restart();
    }
    
    public void End(string id = "",
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "")
    {
        string internalId = id + filePath + methodName;
        if (_entries.TryGetValue(internalId, out Entry? entry))
            entry.Stop();
    }
    
    public void Summary()
    {
        if (_entries.Count == 0) return;
        int totalFrames = (int)Engine.GetProcessFrames();
        double refreshRate = DisplayServer.ScreenGetRefreshRate();
        double targetFps = refreshRate > 0 ? refreshRate : 60;
        GD.Print(Formatter.BuildSummary(_entries.Values, totalFrames, targetFps));
    }

    /// <summary>
    /// Stores timing samples for a single call site in microseconds.
    /// </summary>
    private sealed class Entry(string filePath, string methodName, string id)
    {
        public string FileName { get; } = Path.GetFileName(filePath).Replace(".cs", "");
        public string MethodName { get; } = methodName;
        public string Id { get; } = id;

        private readonly Stopwatch _stopwatch = new();
        private readonly List<double> _times = [];

        public void Stop()
        {
            _stopwatch.Stop();
            _times.Add(_stopwatch.Elapsed.TotalMicroseconds);
        }
        public void Restart() => _stopwatch.Restart();
        public int GetCount() => _times.Count;
        public double GetAvg() => _times.Count > 0 ? _times.Average() : 0;
        public double GetMin() => _times.Count > 0 ? _times.Min() : 0;
        public double GetMax() => _times.Count > 0 ? _times.Max() : 0;
        public double GetTotal() => _times.Sum();
        public double GetCallsPerFrame(int totalFrames) => totalFrames > 0 ? (double)GetCount() / totalFrames : 0;
        public double GetPercentile(double percentile)
        {
            if (_times.Count == 0) return 0;
            if (percentile <= 0) return GetMin();
            if (percentile >= 100) return GetMax();

            List<double> sorted = [.. _times.OrderBy(x => x)];
            int index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
            index = Math.Clamp(index, 0, sorted.Count - 1);

            return sorted[index];
        }
        public double GetFramePercent(int totalFrames, double refreshRate)
        {
            if (totalFrames <= 0) return 0.0;

            double totalUs = GetTotal();
            double avgUsPerFrame = totalUs / totalFrames;
            double budgetUs = 1_000_000 / refreshRate; // 16,667 µs for 60 FPS

            return avgUsPerFrame / budgetUs * 100.0;
        }
    }

    /// <summary>
    /// Formats profiling results into a readable table.
    /// </summary>
    private sealed class Formatter
    {
        private const int MetricColumnWidth = 9;
        private const int MethodIndentSpaces = 4;
        private const int ExtraWidthOnLastColumn = 2;
        private const Metric LoggedMetric = Metric.Milliseconds;
        private static readonly Alignment CellAlignment = Alignment.Center;

        private enum Metric
        {
            Milliseconds,
            Microseconds,
            Nanoseconds
        }

        private enum Alignment
        {
            Left,
            Center,
            Right
        }

        private sealed class Column(
            string header,
            Func<Entry, double> valueGetter,
            Func<double, string> formatter,
            int minWidth)
        {
            public string Header { get; } = header;
            public Func<Entry, double> ValueGetter { get; } = valueGetter;
            public Func<double, string> Formatter { get; } = formatter;
            public int MinWidth { get; } = minWidth;
        }

        private static List<Column> GetColumns(int totalFrames, double targetFps) =>
        [
            new Column("Calls", e => e.GetCount(), FormatCount, MetricColumnWidth),
            new Column("Calls/f", e => e.GetCallsPerFrame(totalFrames), v => $"{v:F1}", MetricColumnWidth),
            new Column("Avg", e => e.GetAvg(), FormatValue, MetricColumnWidth),
            new Column("Max", e => e.GetMax(), FormatValue, MetricColumnWidth),
            new Column("P99", e => e.GetPercentile(99), FormatValue, MetricColumnWidth),
            new Column("P99.9", e => e.GetPercentile(99.9), FormatValue, MetricColumnWidth),
            new Column("Frame%", e => e.GetFramePercent(totalFrames, targetFps), v => $"{v:F2} %", MetricColumnWidth),
        ];

        private sealed class SummaryGroup(string fileName, List<Entry> methods, double averageTime)
        {
            public string FileName { get; } = fileName;
            public List<Entry> Methods { get; } = methods;
            public double AverageTime { get; } = averageTime;
        }

        public static string BuildSummary(IEnumerable<Entry> entries, int totalFrames, double refreshRate)
        {
            List<Entry> entryList = [.. entries];
            List<Column> columns = GetColumns(totalFrames, refreshRate);

            List<SummaryGroup> groupedEntries = [.. entryList
                .GroupBy(e => e.FileName)
                .Select(g =>
                {
                    List<Entry> methods = [.. g.OrderByDescending(m => m.GetAvg())];
                    double totalTime = g.Sum(m => m.GetTotal());
                    int totalSamples = g.Sum(m => m.GetCount());
                    double averageTime = totalSamples > 0 ? totalTime / totalSamples : 0;
                    return new SummaryGroup(g.Key, methods, averageTime);
                })
                .OrderByDescending(g => g.AverageTime)];

            if (groupedEntries.Count == 0)
                return string.Empty;

            // Dynamically calculate best width for method column
            int methodColumnWidth = CalculateMethodColumnWidth(entryList);

            List<int> columnWidths = [.. columns.Select(c =>
                GetColumnWidth(c.Header, c.MinWidth, entryList, e => c.Formatter(c.ValueGetter(e))))];

            // Last column is slightly wider
            if (columnWidths.Count > 0)
                columnWidths[^1] += ExtraWidthOnLastColumn;

            StringBuilder sb = new();

            foreach (SummaryGroup group in groupedEntries)
            {
                // Header row
                List<string> headerCells =
                [
                    BuildSectionTitleCell(group.FileName, methodColumnWidth),
                    .. columnWidths.Select((w, i) => BuildHeaderCell(columns[i].Header, w)),
                ];
                AppendRow(sb, "┌", "┬", "┐", [.. headerCells]);

                // Data rows
                foreach (Entry m in group.Methods)
                {
                    List<string> valueCells =
                    [
                        BuildMethodCell(m, methodColumnWidth),
                        .. columnWidths.Select((w, i) => BuildValueCell(columns[i].Formatter(columns[i].ValueGetter(m)), w)),
                    ];
                    AppendRow(sb, "│", "│", "│", [.. valueCells]);
                }

                // Separator row
                List<string> separatorCells = [BuildSeparatorCell(methodColumnWidth), .. columnWidths.Select(BuildSeparatorCell)];
                AppendRow(sb, "└", "┴", "┘", [.. separatorCells]);

                if (!ReferenceEquals(group, groupedEntries[^1]))
                    sb.AppendLine();
            }

            sb.AppendLine();

            // Final Summary Table
            List<Entry> allEntries = [.. entryList
                .OrderByDescending(e => e.GetFramePercent(totalFrames, refreshRate))
                .ThenByDescending(e => e.GetMax())];

            if (allEntries.Count > 0)
            {
                // Determine required column widths from content
                const int rankWidth = 6; // " Rank "
                int frameWidth = GetColumnWidth("Frame%", MetricColumnWidth, allEntries,
                    e => $"{e.GetFramePercent(totalFrames, refreshRate):F2}%");
                int maxWidth = GetColumnWidth("Max Spike", MetricColumnWidth, allEntries,
                    e => FormatValue(e.GetMax()));

                // Build the full display name for every entry to measure method column width
                int methodWidth = " Method (File) ".Length;
                foreach (Entry e in allEntries)
                {
                    string suffix = string.IsNullOrWhiteSpace(e.Id) ? string.Empty : $" [{e.Id}]";
                    string display = $"{e.MethodName}{suffix} ({e.FileName}.cs)";
                    if (display.Length + 2 > methodWidth) // +2 for surrounding spaces
                        methodWidth = display.Length + 2;
                }
                // Add a bit of breathing room
                methodWidth += 4;

                // Header row
                List<string> summaryHeaderCells =
                [
                    BuildHeaderCell("Rank", rankWidth),
                    BuildHeaderCell("Method (File)", methodWidth),
                    BuildHeaderCell("Frame%", frameWidth),
                    BuildHeaderCell("Max Spike", maxWidth)
                ];
                AppendRow(sb, "┌", "┬", "┐", [.. summaryHeaderCells]);

                // Data rows
                for (int i = 0; i < allEntries.Count; i++)
                {
                    Entry e = allEntries[i];
                    string rank = $"{i + 1}";

                    string suffix = string.IsNullOrWhiteSpace(e.Id) ? string.Empty : $" [{e.Id}]";
                    string methodDisplay = $"{e.MethodName}{suffix} ({e.FileName})";

                    List<string> valueCells =
                    [
                        BuildValueCell(rank, rankWidth),
                        BuildLeftValueCell(methodDisplay, methodWidth), // left‑aligned
                        BuildValueCell($"{e.GetFramePercent(totalFrames, refreshRate):F2}%", frameWidth),
                        BuildValueCell(FormatValue(e.GetMax()), maxWidth)
                    ];
                    AppendRow(sb, "│", "│", "│", [.. valueCells]);
                }

                // Separator row
                List<string> summarySeparatorCells =
                [
                    BuildSeparatorCell(rankWidth),
                    BuildSeparatorCell(methodWidth),
                    BuildSeparatorCell(frameWidth),
                    BuildSeparatorCell(maxWidth)
                ];
                AppendRow(sb, "└", "┴", "┘", [.. summarySeparatorCells]);
            }

            sb.AppendLine();

            // Session Summary
            double totalCostUsPerFrame = allEntries.Sum(e => e.GetTotal()) / totalFrames;
            double totalCostMs = totalCostUsPerFrame / 1000.0;
            double frameBudgetMs = 1000.0 / refreshRate;
            double totalFramePercent = allEntries.Sum(e => e.GetFramePercent(totalFrames, refreshRate));

            Entry? worstEntry = allEntries.MaxBy(e => e.GetMax());
            string worstSpikeLine = worstEntry is not null
                ? $"{worstEntry.MethodName}{(string.IsNullOrWhiteSpace(worstEntry.Id) ? "" : $" [{worstEntry.Id}]")} ({worstEntry.FileName}.cs) – {FormatValue(worstEntry.GetMax())} ({worstEntry.GetCount()} occurrence)"
                : "None";

            string sessionInfoLine = $"{totalFrames} frames ({totalFrames / refreshRate:F1}s), Physics {Engine.PhysicsTicksPerSecond} Hz, Render {refreshRate:F0} Hz";
            string costLine = $"Total frame time: {totalFramePercent:F2}% ({totalCostMs:F2} ms / {frameBudgetMs:F2} ms)";

            List<string> footerContent = [sessionInfoLine, costLine];
            int maxContentWidth = footerContent.Max(l => l.Length);

            string title = " Session Summary ";
            int titleLength = title.Length;

            // Minimum box width to have "┌── Title ──┐" (4 for "┌── ", title, at least 2 dashes, 1 for "┐")
            int minBoxWidth = 7 + titleLength; // 4 + titleLength + 2 + 1
            int boxWidth = Math.Max(maxContentWidth + 4, minBoxWidth); // +4 for "│ " and " │"

            // Top border: "┌── Title ─────...─┐" exactly boxWidth chars
            int dashesAfterTitle = boxWidth - 5 - titleLength; // 4 for "┌── ", 1 for "┐"
            string topBorder = "┌── " + title + new string('─', dashesAfterTitle) + "┐";
            sb.AppendLine(topBorder);

            // Content rows
            foreach (string line in footerContent)
                sb.AppendLine("│ " + line.PadRight(boxWidth - 4) + " │");

            // Bottom border
            string bottomBorder = "└" + new string('─', boxWidth - 2) + "┘";
            sb.AppendLine(bottomBorder);

            return sb.ToString();
        }

        private static string BuildLeftValueCell(string value, int width)
        {
            if (width <= 0) return string.Empty;
            int innerWidth = Math.Max(0, width - 2);
            string trimmed = value.Length > innerWidth ? value[..innerWidth] : value;
            return " " + trimmed.PadRight(innerWidth) + " ";
        }

        private static int CalculateMethodColumnWidth(IEnumerable<Entry> entries)
        {
            int maxWidth = 20; // Reasonable minimum

            foreach (Entry e in entries)
            {
                string suffix = string.IsNullOrWhiteSpace(e.Id) ? string.Empty : $" [{e.Id}]";
                string fullName = e.MethodName + suffix;
                if (fullName.Length > maxWidth)
                    maxWidth = fullName.Length;
            }

            // Add some padding for indentation and readability
            return maxWidth + MethodIndentSpaces + 4;
        }

        private static void AppendRow(StringBuilder sb, string left, string middle, string right, params string[] cells)
        {
            sb.Append(left);
            for (int i = 0; i < cells.Length; i++)
            {
                sb.Append(cells[i]);
                sb.Append(i == cells.Length - 1 ? right : middle);
            }
            sb.AppendLine();
        }

        private static string BuildSectionTitleCell(string title, int width)
        {
            if (width <= 0) return string.Empty;
            string prefix = $"── {title} ";
            if (prefix.Length >= width) return prefix.Length > width ? prefix[..width] : prefix;
            return prefix + new string('─', width - prefix.Length);
        }

        private static string BuildHeaderCell(string title, int width)
        {
            if (width <= 0) return string.Empty;
            string label = $" {title} ";
            if (label.Length >= width) return label.Length > width ? label[..width] : label;
            int dashCount = width - label.Length;
            int left = dashCount / 2;
            int right = dashCount - left;
            return new string('─', left) + label + new string('─', right);
        }

        private static string BuildSeparatorCell(int width) => width > 0 ? new string('─', width) : string.Empty;

        private static string BuildMethodCell(Entry entry, int methodColumnWidth)
        {
            int indent = Math.Min(MethodIndentSpaces, methodColumnWidth);
            int nameWidth = Math.Max(0, methodColumnWidth - indent);
            string suffix = string.IsNullOrWhiteSpace(entry.Id) ? string.Empty : $" [{entry.Id}]";
            string displayName = entry.MethodName + suffix;
            string name = displayName.Length > nameWidth ? displayName[..nameWidth] : displayName;
            return new string(' ', indent) + name.PadRight(nameWidth);
        }

        private static int GetColumnWidth(string header, int minWidth, IEnumerable<Entry> methods, Func<Entry, string> formatter)
        {
            int width = Math.Max(minWidth, $" {header} ".Length);
            foreach (Entry method in methods)
            {
                string value = formatter(method);
                int required = value.Length + 2;
                if (required > width)
                    width = required;
            }
            return width;
        }

        private static string BuildValueCell(string value, int width)
        {
            if (width <= 0)
                return string.Empty;

            int innerWidth = Math.Max(0, width - 2);
            string trimmed = value.Length > innerWidth ? value[..innerWidth] : value;

            string aligned = CellAlignment switch
            {
                Alignment.Left => trimmed.PadRight(innerWidth),
                Alignment.Right => trimmed.PadLeft(innerWidth),
                _ => trimmed.PadLeft((innerWidth + trimmed.Length) / 2).PadRight(innerWidth) // Center
            };

            return " " + aligned + " ";
        }

        private static string FormatCount(double count)
        {
            return ((int)count).ToString("N0");
        }

        private static string FormatValue(double us)
        {
            return LoggedMetric switch
            {
                Metric.Milliseconds => $"{us / 1000.0:F3} ms",
                Metric.Microseconds => $"{us:F1} µs",
                Metric.Nanoseconds => $"{us * 1000.0:F0} ns",
                _ => $"{us:F1} µs"
            };
        }
    }
}
