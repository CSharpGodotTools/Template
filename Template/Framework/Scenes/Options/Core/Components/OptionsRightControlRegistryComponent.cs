using System;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Ui;

internal sealed class OptionsRightControlRegistryComponent
{
    private readonly Dictionary<string, int> _rightControlIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, RegisteredRightControl> _rightControls = [];
    private int _nextRightControlId;

    public event Action<RegisteredRightControl>? RightControlRegistered;

    public IEnumerable<RegisteredRightControl> GetRightControls()
    {
        return _rightControls.Values.OrderBy(control => control.Id);
    }

    public void AddRightControl(OptionRightControlDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        OptionValidator.ValidateTab(definition.Tab);
        OptionValidator.ValidateLabel(definition.TargetLabel, "RightControl target");
        OptionValidator.ValidateLabel(definition.Name, "RightControl name");

        string key = CreateRightControlKey(definition.Tab, definition.TargetLabel, definition.Name);
        if (!_rightControlIds.TryGetValue(key, out int id))
        {
            id = ++_nextRightControlId;
            _rightControlIds[key] = id;
        }

        RegisteredRightControl registered = new(id, definition);
        _rightControls[id] = registered;
        RightControlRegistered?.Invoke(registered);
    }

    private static string CreateRightControlKey(string tab, string targetLabel, string name)
    {
        return $"{tab.Trim()}::{targetLabel.Trim()}::{name.Trim()}";
    }
}
