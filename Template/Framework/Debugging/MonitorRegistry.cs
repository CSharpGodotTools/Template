using Godot;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Debugging;

internal sealed class MonitorRegistry
{
    private readonly Dictionary<int, (string DisplayName, Func<object> Provider, GodotObject? Owner)> _monitors = [];
    private readonly Dictionary<string, int> _monitorIdsByKey = [];
    private readonly Dictionary<string, MonitorHandle> _monitorHandlesByKey = [];
    private int _nextMonitorId;
    private bool _hasChanges;

    public int Count => _monitors.Count;

    public IReadOnlyDictionary<int, (string DisplayName, Func<object> Provider, GodotObject? Owner)> Monitors => _monitors;

    public IDisposable StartMonitoring(string key, Func<object> function)
    {
        ArgumentNullException.ThrowIfNull(function);

        string displayName = string.IsNullOrWhiteSpace(key) ? "Monitor" : key;
        GodotObject? owner = function.Target as GodotObject;

        if (_monitorIdsByKey.TryGetValue(displayName, out int existingId))
        {
            _monitors[existingId] = (displayName, function, owner);

            if (!_monitorHandlesByKey.TryGetValue(displayName, out MonitorHandle? existingHandle))
            {
                existingHandle = new MonitorHandle(this, existingId);
                _monitorHandlesByKey[displayName] = existingHandle;
            }

            NotifyChanged();
            return existingHandle;
        }

        int monitorId = ++_nextMonitorId;
        _monitors[monitorId] = (displayName, function, owner);
        _monitorIdsByKey[displayName] = monitorId;

        MonitorHandle handle = new(this, monitorId);
        _monitorHandlesByKey[displayName] = handle;

        NotifyChanged();
        return handle;
    }

    public bool RemoveInvalidMonitors()
    {
        if (_monitors.Count == 0)
            return false;

        List<int>? invalidIds = null;
        foreach ((int monitorId, (string DisplayName, Func<object> Provider, GodotObject? Owner) monitor) in _monitors)
        {
            if (monitor.Owner == null)
                continue;

            if (!GodotObject.IsInstanceValid(monitor.Owner))
            {
                invalidIds ??= [];
                invalidIds.Add(monitorId);
            }
        }

        if (invalidIds == null)
            return false;

        foreach (int monitorId in invalidIds)
            RemoveMonitor(monitorId);

        NotifyChanged();
        return true;
    }

    public bool ConsumeChanges()
    {
        if (!_hasChanges)
            return false;

        _hasChanges = false;
        return true;
    }

    private void NotifyChanged()
    {
        _hasChanges = true;
    }

    private void StopMonitoring(int monitorId)
    {
        if (!RemoveMonitor(monitorId))
            return;

        NotifyChanged();
    }

    private bool RemoveMonitor(int monitorId)
    {
        if (!_monitors.Remove(monitorId, out (string DisplayName, Func<object> Provider, GodotObject? Owner) monitor))
            return false;

        if (_monitorIdsByKey.TryGetValue(monitor.DisplayName, out int mappedId) && mappedId == monitorId)
        {
            _monitorIdsByKey.Remove(monitor.DisplayName);
            _monitorHandlesByKey.Remove(monitor.DisplayName);
        }

        return true;
    }

    private sealed class MonitorHandle(MonitorRegistry owner, int monitorId) : IDisposable
    {
        private MonitorRegistry? _owner = owner;
        private readonly int _monitorId = monitorId;

        public void Dispose()
        {
            if (_owner == null)
                return;

            _owner.StopMonitoring(_monitorId);
            _owner = null;
            GC.SuppressFinalize(this);
        }
    }
}
