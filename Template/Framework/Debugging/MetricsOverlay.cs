using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using Monitor = Godot.Performance.Monitor;

namespace __TEMPLATE__.Debugging;

/// <summary>
/// Debug overlay that displays runtime metrics and custom monitored values.
/// </summary>
public partial class MetricsOverlay : CanvasLayer, IMetricsOverlay
{
    // Config
    private const int BytesInMegabyte = 1024 * 1024;
    private const int BytesInKilobyte = 1024;
    private const int MaxFpsBuffer = 100;
    private const int PanelWidth = 280;
    private const int GraphHeight = 50;
    private const int Margin = 10;
    private const int MonitorValueDecimals = 2;
    private const string MetricsHeaderText = "METRICS";
    private const string VariablesHeaderText = "VARIABLES";

    // Theme Colors
    private static readonly Color _backgroundColor = new(0.11f, 0.11f, 0.13f, 0.92f);
    private static readonly Color _headerColor = new(0.16f, 0.16f, 0.18f, 1.0f);
    private static readonly Color _textColor = new(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Color _accentColor = new(0.26f, 0.59f, 0.98f, 1.0f);
    private static readonly Color _graphLineColor = new(0.26f, 0.59f, 0.98f, 0.8f);
    private static readonly Color _graphFillColor = new(0.26f, 0.59f, 0.98f, 0.3f);

    // Variables
    [Export]
    private string _toggleAction = "debug_overlay";

    private readonly MonitorRegistry _registry = new();
    private readonly Dictionary<string, Label> _metricLabels = new();
    private readonly List<(Label label, string displayName, Func<object> provider)> _variableLabels = new();
    private readonly Dictionary<string, Func<float, string>> _currentMetrics = [];
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
    private bool _metricsExpanded = true;
    private bool _variablesExpanded = true;

    /// <summary>
    /// Initializes default metric providers.
    /// </summary>
    public MetricsOverlay()
    {
        Dictionary<string, (bool Enabled, Func<float, string> ValueProvider)> metrics = new()
        {
            { "FPS", (true, fps => $"{fps:F0}") },
            { "Process", (false, _ => $"{Retrieve(Monitor.TimeProcess) * 1000:0.00} ms") },
            { "Physics Process", (false, _ => $"{Retrieve(Monitor.TimePhysicsProcess) * 1000:0.00} ms") },
            { "Navigation Process", (false, _ => $"{Retrieve(Monitor.TimeNavigationProcess) * 1000:0.00} ms") },
            { "Static Memory", (true, _ => $"{Retrieve(Monitor.MemoryStatic) / BytesInMegabyte:0.0} MiB") },
            { "Static Memory Max", (false, _ => $"{Retrieve(Monitor.MemoryStaticMax) / BytesInMegabyte:0.0} MiB") },
            { "Video Memory", (true, _ => $"{Retrieve(Monitor.RenderVideoMemUsed) / BytesInMegabyte:0.0} MiB") },
            { "Texture Memory", (false, _ => $"{Retrieve(Monitor.RenderTextureMemUsed) / BytesInMegabyte:0.0} MiB") },
            { "Buffer Memory", (false, _ => $"{Retrieve(Monitor.RenderBufferMemUsed) / BytesInMegabyte:0.0} MiB") },
            { "Message Buffer Max", (false, _ => $"{Retrieve(Monitor.MemoryMessageBufferMax) / BytesInKilobyte:0.0} KiB") },
            { "Resource Count", (false, _ => $"{Retrieve(Monitor.ObjectResourceCount)}") },
            { "Node Count", (true, _ => $"{Retrieve(Monitor.ObjectNodeCount)}") },
            { "Orphan Node Count", (true, _ => $"{Retrieve(Monitor.ObjectOrphanNodeCount)}") },
            { "Object Count", (true, _ => $"{Retrieve(Monitor.ObjectCount)}") },
            { "Total Objects Drawn", (false, _ => $"{Retrieve(Monitor.RenderTotalObjectsInFrame)}") },
            { "Total Primitives Drawn", (false, _ => $"{Retrieve(Monitor.RenderTotalPrimitivesInFrame)}") },
            { "Total Draw Calls", (false, _ => $"{Retrieve(Monitor.RenderTotalDrawCallsInFrame)}") },
        };

        foreach (KeyValuePair<string, (bool Enabled, Func<float, string> ValueProvider)> metric in metrics)
        {
            // Seed overlay with metrics marked as enabled by default.
            if (metric.Value.Enabled)
                _currentMetrics.Add(metric.Key, metric.Value.ValueProvider);
        }

    }

    // Godot Overrides
    public override void _Ready()
    {
        BuildUi();

        GetViewport().SizeChanged += () =>
        {
            if (_panel != null && IsInstanceValid(_panel))
                PositionPanel();
        };

        Layer = 100;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        // Toggle overlay visibility on debug shortcut press.
        if (Input.IsActionJustPressed(_toggleAction))
            Visible = !Visible;

        // Refresh values only when the overlay is visible.
        if (Visible)
            UpdateMetrics();
    }

    // API
    /// <summary>
    /// Registers a custom monitor callback and returns a disposable handle.
    /// </summary>
    /// <param name="key">Display label for the monitored value.</param>
    /// <param name="function">Callback that returns current monitor value.</param>
    /// <returns>Disposable handle that unregisters the monitor when disposed.</returns>
    public IDisposable StartMonitoring(string key, Func<object> function)
    {
        ArgumentNullException.ThrowIfNull(function);

        IDisposable handle = _registry.StartMonitoring(key, function);
        UpdateVariablesSection();
        return handle;
    }

    /// <summary>
    /// Forces a refresh of the custom variables UI.
    /// </summary>
    public void RefreshVariablesUI()
    {
        UpdateVariablesSection();
    }

    // Private Methods
    /// <summary>
    /// Builds root overlay controls and both sections.
    /// </summary>
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

    /// <summary>
    /// Positions panel near the top-right corner of the viewport.
    /// </summary>
    private void PositionPanel()
    {
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        _panel.Position = new Vector2(viewportSize.X - PanelWidth - Margin, Margin);
    }

    /// <summary>
    /// Builds the built-in metrics section and FPS graph.
    /// </summary>
    private void BuildMetricsSection()
    {
        _metricsHeader = CreateSectionHeader(MetricsHeaderText);
        _metricsHeader.Pressed += () => ToggleSection(ref _metricsExpanded, _metricsContainer);
        _mainContainer.AddChild(_metricsHeader);

        _metricsContainer = new VBoxContainer { Name = "MetricsContainer" };
        _mainContainer.AddChild(_metricsContainer);

        _fpsGraph = new FpsGraph { CustomMinimumSize = new Vector2(0, GraphHeight) };
        _metricsContainer.AddChild(_fpsGraph);

        UpdateMetricsSection();
    }

    /// <summary>
    /// Builds the custom variables section.
    /// </summary>
    private void BuildVariablesSection()
    {
        _variablesHeader = CreateSectionHeader(VariablesHeaderText);
        _variablesHeader.Pressed += () => ToggleSection(ref _variablesExpanded, _variablesContainer);
        _variablesHeader.Visible = false;
        _mainContainer.AddChild(_variablesHeader);

        _variablesContainer = new VBoxContainer { Name = "VariablesContainer", Visible = false };
        _mainContainer.AddChild(_variablesContainer);
    }

    /// <summary>
    /// Creates a styled section header button.
    /// </summary>
    /// <param name="text">Header label text.</param>
    /// <returns>Configured header button.</returns>
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
        button.AddThemeColorOverride("font_color", _accentColor);

        return button;
    }

    /// <summary>
    /// Toggles expanded state and visibility of a section container.
    /// </summary>
    /// <param name="expanded">Reference to expanded-state flag.</param>
    /// <param name="container">Section container to show or hide.</param>
    private static void ToggleSection(ref bool expanded, Control container)
    {
        expanded = !expanded;
        container.Visible = expanded;
    }

    /// <summary>
    /// Refreshes FPS sample data and updates visible sections.
    /// </summary>
    private void UpdateMetrics()
    {
        _cachedFps = (float)Retrieve(Monitor.TimeFps);
        _fpsBuffer[_fpsIndex] = _cachedFps;
        _fpsIndex = (_fpsIndex + 1) % _fpsBuffer.Length;

        _fpsGraph.UpdateData(_fpsBuffer, _fpsIndex);
        UpdateMetricsSection();

        // Refresh custom variables section only when monitors are registered.
        if (_registry.Count > 0)
            UpdateVariablesSection();
    }

    /// <summary>
    /// Rebuilds labels for built-in metric values.
    /// </summary>
    private void UpdateMetricsSection()
    {
        foreach ((string key, Func<float, string> valueProvider) in _currentMetrics)
        {
            string text = $"{key}: {valueProvider(_cachedFps)}";
            if (_metricLabels.TryGetValue(key, out Label? label))
            {
                label.Text = text;
                continue;
            }

            Label newLabel = CreateLabel(text);
            _metricLabels[key] = newLabel;
            _metricsContainer.AddChild(newLabel);
        }
    }

    /// <summary>
    /// Rebuilds labels for custom monitored values.
    /// </summary>
    private void UpdateVariablesSection()
    {
        _registry.RemoveInvalidMonitors();
        bool rebuild = _registry.ConsumeChanges();
        bool hasVariables = _registry.Count > 0;
        _variablesHeader.Visible = hasVariables;
        _variablesContainer.Visible = hasVariables && _variablesExpanded;

        // Skip rebuild work when there are no custom variables to show.
        if (!hasVariables)
        {
            if (rebuild && _variableLabels.Count > 0)
                ClearVariableLabels();

            return;
        }

        if (rebuild)
        {
            RebuildVariableLabels();
            return;
        }

        foreach ((Label label, string displayName, Func<object> provider) in _variableLabels)
            label.Text = $"{displayName}: {FormatMonitorValue(provider())}";
    }

    /// <summary>
    /// Formats monitor values into display text.
    /// </summary>
    /// <param name="value">Value returned by monitor callback.</param>
    /// <returns>Formatted display text.</returns>
    private static string FormatMonitorValue(object? value)
    {
        // Render explicit null values as the string literal "null".
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
            _ => value.ToString() ?? ""
        };
    }

    /// <summary>
    /// Formats numeric values with fixed decimal precision.
    /// </summary>
    /// <param name="value">Numeric value to format.</param>
    /// <returns>Invariant-culture formatted number.</returns>
    private static string FormatDecimal(double value)
    {
        return value.ToString($"F{MonitorValueDecimals}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Removes all custom variable labels and clears the cache.
    /// </summary>
    private void ClearVariableLabels()
    {
        foreach (Node child in _variablesContainer.GetChildren())
        {
            _variablesContainer.RemoveChild(child);
            child.QueueFree();
        }

        _variableLabels.Clear();
    }

    /// <summary>
    /// Rebuilds custom variable labels from current monitors.
    /// </summary>
    private void RebuildVariableLabels()
    {
        ClearVariableLabels();

        foreach ((string displayName, Func<object> provider, _) in _registry.Monitors.Values)
        {
            Label label = CreateLabel($"{displayName}: {FormatMonitorValue(provider())}");
            _variablesContainer.AddChild(label);
            _variableLabels.Add((label, displayName, provider));
        }
    }

    /// <summary>
    /// Creates a styled metric label control.
    /// </summary>
    /// <param name="text">Label text.</param>
    /// <returns>Configured label.</returns>
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

    /// <summary>
    /// Reads a Godot performance monitor value.
    /// </summary>
    /// <param name="monitor">Monitor enum to query.</param>
    /// <returns>Monitor value as double.</returns>
    private static double Retrieve(Monitor monitor) => Performance.GetMonitor(monitor);

    // Nested Class: FPS Graph
    /// <summary>
    /// Simple line/area graph used to render recent FPS samples.
    /// </summary>
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

        /// <summary>
        /// Updates graph sample buffer and schedules redraw.
        /// </summary>
        /// <param name="data">Circular sample buffer.</param>
        /// <param name="index">Current insertion index.</param>
        public void UpdateData(float[] data, int index)
        {
            _data = data;
            _dataIndex = index;
            QueueRedraw();
        }

        public override void _Draw()
        {
            // Skip drawing when no samples are available yet.
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
                fillPoints[i + 1] = points[i];
            fillPoints[bufferLength + 1] = new Vector2(graphSize.X, graphSize.Y);
            DrawColoredPolygon(fillPoints, _graphFillColor);

            // Draw line
            for (int i = 0; i < bufferLength - 1; i++)
                DrawLine(points[i], points[i + 1], _graphLineColor, 2f);
        }
    }
}
