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
        private const int RankColumnWidth = 6;
        private const Metric LoggedMetric = Metric.Milliseconds;

        private enum Metric
        {
            Milliseconds,
            Microseconds,
            Nanoseconds
        }

        private sealed class MetricColumn(
            string header,
            Func<Entry, double> valueGetter,
            Func<double, string> formatter)
        {
            public string Header { get; } = header;
            public Func<Entry, double> ValueGetter { get; } = valueGetter;
            public Func<double, string> Formatter { get; } = formatter;
        }

        private static List<MetricColumn> GetColumns(int totalFrames, double targetFps) =>
        [
            new MetricColumn("Calls", e => e.GetCount(), FormatCount),
            new MetricColumn("Calls/f", e => e.GetCallsPerFrame(totalFrames), v => $"{v:F1}"),
            new MetricColumn("Avg", e => e.GetAvg(), FormatValue),
            new MetricColumn("Max", e => e.GetMax(), FormatValue),
            new MetricColumn("P99", e => e.GetPercentile(99), FormatValue),
            new MetricColumn("P99.9", e => e.GetPercentile(99.9), FormatValue),
            new MetricColumn("Frame%", e => e.GetFramePercent(totalFrames, targetFps), v => $"{v:F2} %"),
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
            List<MetricColumn> columns = GetColumns(totalFrames, refreshRate);

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

            List<List<string>> metricValues = [.. columns
                .Select(c => entryList.Select(e => c.Formatter(c.ValueGetter(e))).ToList())];
            int[] metricExtras = metricValues
                .Select((values, i) => CalculateExtraWidth(columns[i].Header, MetricColumnWidth, values))
                .ToArray();

            StringBuilder sb = new();

            TextTableLayout layout = new();
            layout.AddColumn(string.Empty, Alignment.Left);
            for (int i = 0; i < columns.Count; i++)
                layout.AddColumn(columns[i].Header, Alignment.Center, metricExtras[i]);

            List<TextTable> groupTables = new(groupedEntries.Count);

            foreach (SummaryGroup group in groupedEntries)
            {
                TextTable groupTable = layout.CreateTable();
                groupTable.SetColumnHeader(0, group.FileName);

                foreach (Entry method in group.Methods)
                {
                    string[] row = new string[columns.Count + 1];
                    row[0] = BuildMethodDisplay(method);
                    for (int i = 0; i < columns.Count; i++)
                        row[i + 1] = columns[i].Formatter(columns[i].ValueGetter(method));
                    groupTable.AddRow(row);
                }

                groupTables.Add(groupTable);
            }

            for (int i = 0; i < groupTables.Count; i++)
            {
                sb.Append(groupTables[i].Render());
                if (i < groupTables.Count - 1)
                    sb.AppendLine();
            }

            sb.AppendLine();

            // Final Summary Table
            List<Entry> allEntries = [.. entryList
                .OrderByDescending(e => e.GetFramePercent(totalFrames, refreshRate))
                .ThenByDescending(e => e.GetMax())];

            if (allEntries.Count > 0)
            {
                List<string> rankValues = new(allEntries.Count);
                List<string> methodValues = new(allEntries.Count);
                List<string> frameValues = new(allEntries.Count);
                List<string> maxValues = new(allEntries.Count);

                for (int i = 0; i < allEntries.Count; i++)
                {
                    Entry e = allEntries[i];
                    rankValues.Add($"{i + 1}");
                    methodValues.Add(BuildSummaryMethodDisplay(e));
                    frameValues.Add($"{e.GetFramePercent(totalFrames, refreshRate):F2}%");
                    maxValues.Add(FormatValue(e.GetMax()));
                }

                TextTable summaryTable = new();
                int rankExtra = CalculateExtraWidth("Rank", RankColumnWidth, rankValues);
                int frameExtra = CalculateExtraWidth("Frame%", MetricColumnWidth, frameValues);
                int maxExtra = CalculateExtraWidth("Max Spike", MetricColumnWidth, maxValues);

                summaryTable.AddColumn("Rank", Alignment.Center, rankExtra);
                summaryTable.AddColumn("Method (File)", Alignment.Left, extraWidth: 2);
                summaryTable.AddColumn("Frame%", Alignment.Center, frameExtra);
                summaryTable.AddColumn("Max Spike", Alignment.Center, maxExtra);

                for (int i = 0; i < allEntries.Count; i++)
                    summaryTable.AddRow(rankValues[i], " " + methodValues[i], frameValues[i], maxValues[i]);

                sb.Append(summaryTable.Render());
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

            TextTable sessionSummary = new();
            sessionSummary.AddColumn("Session Summary", Alignment.Left);
            sessionSummary.AddRow(sessionInfoLine);
            sessionSummary.AddRow(costLine);
            sb.Append(sessionSummary.Render());

            return sb.ToString();
        }

        private static string BuildMethodDisplay(Entry entry)
        {
            string suffix = string.IsNullOrWhiteSpace(entry.Id) ? string.Empty : $" [{entry.Id}]";
            return " " + entry.MethodName + suffix + "  ";
        }

        private static string BuildSummaryMethodDisplay(Entry entry)
        {
            string suffix = string.IsNullOrWhiteSpace(entry.Id) ? string.Empty : $" [{entry.Id}]";
            return $"{entry.MethodName}{suffix} ({entry.FileName})";
        }

        private static int CalculateExtraWidth(string header, int minWidth, IEnumerable<string> values)
        {
            int headerWidth = (" " + header + " ").Length;
            int maxValueWidth = 0;

            foreach (string value in values)
            {
                int width = (value?.Length ?? 0) + 2;
                if (width > maxValueWidth)
                    maxValueWidth = width;
            }

            int baseWidth = Math.Max(headerWidth, maxValueWidth);
            return Math.Max(0, minWidth - baseWidth);
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
