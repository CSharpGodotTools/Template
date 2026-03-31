using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Monitor = Godot.Performance.Monitor;

namespace __TEMPLATE__.Debugging;

public partial class MetricsOverlay : CanvasLayer, IMetricsOverlay
{
    // Config
    private const int BytesInMegabyte = 1048576;
    private const int BytesInKilobyte = 1024;
    private const int MaxFpsBuffer = 100;
    private const int PanelWidth = 280;
    private const int GraphHeight = 50;
    private const int Margin = 10;
    private const int MonitorValueDecimals = 2;

    // Theme Colors
    private static readonly Color _backgroundColor = new(0.11f, 0.11f, 0.13f, 0.92f);
    private static readonly Color _headerColor = new(0.16f, 0.16f, 0.18f, 1.0f);
    private static readonly Color _textColor = new(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Color _accentColor = new(0.26f, 0.59f, 0.98f, 1.0f);
    private static readonly Color _graphLineColor = new(0.26f, 0.59f, 0.98f, 0.8f);
    private static readonly Color _graphFillColor = new(0.26f, 0.59f, 0.98f, 0.3f);

    // Variables
    private readonly Dictionary<int, (string DisplayName, Func<object> Provider)> _monitors = [];
    private readonly Dictionary<string, Func<string>> _currentMetrics = [];
    private readonly float[] _fpsBuffer = new float[MaxFpsBuffer];

    private PanelContainer _panel = null!;
    private VBoxContainer _mainContainer = null!;
    private VBoxContainer _metricsContainer = null!;
    private VBoxContainer _variablesContainer = null!;
    private FpsGraph _fpsGraph = null!;
    private Button _metricsHeader = null!;
    private Button _variablesHeader = null!;

    private float _cachedFps;
    private int _fpsIndex;
    private int _nextMonitorId;
    private bool _metricsExpanded = true;
    private bool _variablesExpanded = true;

    public MetricsOverlay()
    {
        Dictionary<string, (bool Enabled, Func<string> ValueProvider)> metrics = new()
        {
            { "FPS", (true, () => $"{_cachedFps:F0}") },
            { "Process", (false, () => $"{Retrieve(Monitor.TimeProcess) * 1000:0.00} ms") },
            { "Physics Process", (false, () => $"{Retrieve(Monitor.TimePhysicsProcess) * 1000:0.00} ms") },
            { "Navigation Process", (false, () => $"{Retrieve(Monitor.TimeNavigationProcess) * 1000:0.00} ms") },
            { "Static Memory", (true, () => $"{Retrieve(Monitor.MemoryStatic) / BytesInMegabyte:0.0} MiB") },
            { "Static Memory Max", (false, () => $"{Retrieve(Monitor.MemoryStaticMax) / BytesInMegabyte:0.0} MiB") },
            { "Video Memory", (true, () => $"{Retrieve(Monitor.RenderVideoMemUsed) / BytesInMegabyte:0.0} MiB") },
            { "Texture Memory", (false, () => $"{Retrieve(Monitor.RenderTextureMemUsed) / BytesInMegabyte:0.0} MiB") },
            { "Buffer Memory", (false, () => $"{Retrieve(Monitor.RenderBufferMemUsed) / BytesInMegabyte:0.0} MiB") },
            { "Message Buffer Max", (false, () => $"{Retrieve(Monitor.MemoryMessageBufferMax) / BytesInKilobyte:0.0} KiB") },
            { "Resource Count", (false, () => $"{Retrieve(Monitor.ObjectResourceCount)}") },
            { "Node Count", (true, () => $"{Retrieve(Monitor.ObjectNodeCount)}") },
            { "Orphan Node Count", (true, () => $"{Retrieve(Monitor.ObjectOrphanNodeCount)}") },
            { "Object Count", (true, () => $"{Retrieve(Monitor.ObjectCount)}") },
            { "Total Objects Drawn", (false, () => $"{Retrieve(Monitor.RenderTotalObjectsInFrame)}") },
            { "Total Primitives Drawn", (false, () => $"{Retrieve(Monitor.RenderTotalPrimitivesInFrame)}") },
            { "Total Draw Calls", (false, () => $"{Retrieve(Monitor.RenderTotalDrawCallsInFrame)}") },
        };

        foreach (KeyValuePair<string, (bool Enabled, Func<string> ValueProvider)> metric in metrics)
        {
            if (metric.Value.Enabled)
            {
                _currentMetrics.Add(metric.Key, metric.Value.ValueProvider);
            }
        }

        BuildUi();
    }

    // Godot Overrides
    public override void _Ready()
    {
        Layer = 100;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.DebugOverlay))
        {
            Visible = !Visible;
        }

        if (Visible)
        {
            UpdateMetrics();
        }
    }

    // API
    public IDisposable StartMonitoring(string key, Func<object> function)
    {
        ArgumentNullException.ThrowIfNull(function);

        string displayName = string.IsNullOrWhiteSpace(key) ? "Monitor" : key;
        int monitorId = ++_nextMonitorId;

        Visible = true;
        _monitors[monitorId] = (displayName, function);
        UpdateVariablesSection();

        return new MonitorHandle(this, monitorId);
    }

    private void StopMonitoring(int monitorId)
    {
        if (!_monitors.Remove(monitorId))
            return;

        UpdateVariablesSection();
    }

    // Private Methods
    private void BuildUi()
    {
        _panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(PanelWidth, 0)
        };
        AddChild(_panel);

        StyleBox panelStyle = new StyleBoxFlat
        {
            BgColor = _backgroundColor,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = Margin,
            ContentMarginTop = Margin,
            ContentMarginRight = Margin,
            ContentMarginBottom = Margin
        };
        _panel.AddThemeStyleboxOverride("panel", panelStyle);

        _mainContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        _panel.AddChild(_mainContainer);

        BuildMetricsSection();
        BuildVariablesSection();

        CallDeferred(MethodName.PositionPanel);
    }

    private void PositionPanel()
    {
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        _panel.Position = new Vector2(viewportSize.X - PanelWidth - Margin, Margin);
    }

    private void BuildMetricsSection()
    {
        _metricsHeader = CreateSectionHeader("METRICS");
        _metricsHeader.Pressed += () => ToggleSection(ref _metricsExpanded, _metricsContainer);
        _mainContainer.AddChild(_metricsHeader);

        _metricsContainer = new VBoxContainer { Name = "MetricsContainer" };
        _mainContainer.AddChild(_metricsContainer);

        _fpsGraph = new FpsGraph { CustomMinimumSize = new Vector2(0, GraphHeight) };
        _metricsContainer.AddChild(_fpsGraph);

        UpdateMetricsSection();
    }

    private void BuildVariablesSection()
    {
        _variablesHeader = CreateSectionHeader("VARIABLES");
        _variablesHeader.Pressed += () => ToggleSection(ref _variablesExpanded, _variablesContainer);
        _variablesHeader.Visible = false;
        _mainContainer.AddChild(_variablesHeader);

        _variablesContainer = new VBoxContainer { Name = "VariablesContainer", Visible = false };
        _mainContainer.AddChild(_variablesContainer);
    }

    private static Button CreateSectionHeader(string text)
    {
        Button button = new()
        {
            Text = text,
            Flat = true,
            Alignment = HorizontalAlignment.Left
        };

        StyleBox normalStyle = new StyleBoxFlat
        {
            BgColor = _headerColor,
            ContentMarginLeft = 5,
            ContentMarginTop = 3,
            ContentMarginRight = 5,
            ContentMarginBottom = 3
        };
        button.AddThemeStyleboxOverride("normal", normalStyle);
        button.AddThemeStyleboxOverride("hover", normalStyle);
        button.AddThemeColorOverride("font_color", _accentColor);

        return button;
    }

    private static void ToggleSection(ref bool expanded, Control container)
    {
        expanded = !expanded;
        container.Visible = expanded;
    }

    private void UpdateMetrics()
    {
        _cachedFps = (float)Retrieve(Monitor.TimeFps);
        _fpsBuffer[_fpsIndex] = _cachedFps;
        _fpsIndex = (_fpsIndex + 1) % _fpsBuffer.Length;

        _fpsGraph.UpdateData(_fpsBuffer, _fpsIndex);
        UpdateMetricsSection();

        if (_monitors.Count > 0)
        {
            UpdateVariablesSection();
        }
    }

    private void UpdateMetricsSection()
    {
        ClearLabels(_metricsContainer, skipGraph: true);

        foreach ((string key, Func<string> valueProvider) in _currentMetrics)
        {
            Label label = CreateLabel($"{key}: {valueProvider()}");
            _metricsContainer.AddChild(label);
        }
    }

    private void UpdateVariablesSection()
    {
        bool hasVariables = _monitors.Count > 0;
        _variablesHeader.Visible = hasVariables;
        _variablesContainer.Visible = hasVariables && _variablesExpanded;

        ClearLabels(_variablesContainer, skipGraph: false);

        if (!hasVariables)
        {
            return;
        }

        foreach ((string displayName, Func<object> provider) in _monitors.Values)
        {
            Label label = CreateLabel($"{displayName}: {FormatMonitorValue(provider())}");
            _variablesContainer.AddChild(label);
        }
    }

    private static string FormatMonitorValue(object? value)
    {
        if (value is null)
            return "null";

        return value switch
        {
            float number => FormatDecimal(number),
            double number => FormatDecimal(number),
            decimal number => FormatDecimal((double)number),
            Vector2 vector => $"({FormatDecimal(vector.X)}, {FormatDecimal(vector.Y)})",
            Vector2I vector => $"({FormatDecimal(vector.X)}, {FormatDecimal(vector.Y)})",
            Vector3 vector => $"({FormatDecimal(vector.X)}, {FormatDecimal(vector.Y)}, {FormatDecimal(vector.Z)})",
            Vector3I vector => $"({FormatDecimal(vector.X)}, {FormatDecimal(vector.Y)}, {FormatDecimal(vector.Z)})",
            Vector4 vector => $"({FormatDecimal(vector.X)}, {FormatDecimal(vector.Y)}, {FormatDecimal(vector.Z)}, {FormatDecimal(vector.W)})",
            Quaternion quaternion => $"({FormatDecimal(quaternion.X)}, {FormatDecimal(quaternion.Y)}, {FormatDecimal(quaternion.Z)}, {FormatDecimal(quaternion.W)})",
            Color color => $"({FormatDecimal(color.R)}, {FormatDecimal(color.G)}, {FormatDecimal(color.B)}, {FormatDecimal(color.A)})",
            _ => RoundDecimalsInText(value.ToString() ?? string.Empty)
        };
    }

    private static string FormatDecimal(double value)
    {
        return value.ToString($"F{MonitorValueDecimals}", CultureInfo.InvariantCulture);
    }

    private static string RoundDecimalsInText(string text)
    {
        return DecimalNumberRegex().Replace(text, match =>
        {
            if (double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            {
                return FormatDecimal(number);
            }

            return match.Value;
        });
    }

    [GeneratedRegex(@"-?\d+\.\d+")]
    private static partial Regex DecimalNumberRegex();

    private static void ClearLabels(Node container, bool skipGraph)
    {
        foreach (Node child in container.GetChildren())
        {
            if (skipGraph && child is FpsGraph)
                continue;

            container.RemoveChild(child);
            child.QueueFree();
        }
    }

    private static Label CreateLabel(string text)
    {
        Label label = new()
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.Arbitrary,
            SizeFlagsHorizontal = Control.SizeFlags.Fill,
            CustomMinimumSize = new Vector2(PanelWidth - (Margin * 2), 0)
        };
        label.AddThemeColorOverride("font_color", _textColor);
        return label;
    }

    private static double Retrieve(Monitor monitor) => Performance.GetMonitor(monitor);

    private sealed class MonitorHandle(MetricsOverlay owner, int monitorId) : IDisposable
    {
        private MetricsOverlay? _owner = owner;
        private readonly int _monitorId = monitorId;

        public void Dispose()
        {
            if (_owner == null)
                return;

            if (GodotObject.IsInstanceValid(_owner))
                _owner.StopMonitoring(_monitorId);

            _owner = null;
        }
    }

    // Nested Class: FPS Graph
    private partial class FpsGraph : Control
    {
        private float[] _data = [];
        private int _dataIndex;
        private float _maxRefreshRate;

        public override void _Ready()
        {
            float refreshRate = DisplayServer.ScreenGetRefreshRate();
            _maxRefreshRate = refreshRate > 0 ? refreshRate : 60f;
        }

        public void UpdateData(float[] data, int index)
        {
            _data = data;
            _dataIndex = index;
            QueueRedraw();
        }

        public override void _Draw()
        {
            if (_data.Length == 0)
                return;

            Vector2 graphSize = Size;
            int bufferLength = _data.Length;
            float xStep = graphSize.X / (bufferLength - 1);

            Vector2[] points = new Vector2[bufferLength];
            for (int i = 0; i < bufferLength; i++)
            {
                int dataIdx = (_dataIndex + i) % bufferLength;
                float normalizedValue = Mathf.Clamp(_data[dataIdx] / _maxRefreshRate, 0f, 1f);
                float x = i * xStep;
                float y = graphSize.Y - (normalizedValue * graphSize.Y);
                points[i] = new Vector2(x, y);
            }

            // Draw filled area
            Vector2[] fillPoints = new Vector2[bufferLength + 2];
            fillPoints[0] = new Vector2(0, graphSize.Y);
            for (int i = 0; i < bufferLength; i++)
            {
                fillPoints[i + 1] = points[i];
            }
            fillPoints[bufferLength + 1] = new Vector2(graphSize.X, graphSize.Y);
            DrawColoredPolygon(fillPoints, _graphFillColor);

            // Draw line
            for (int i = 0; i < bufferLength - 1; i++)
            {
                DrawLine(points[i], points[i + 1], _graphLineColor, 2f);
            }
        }
    }
}
