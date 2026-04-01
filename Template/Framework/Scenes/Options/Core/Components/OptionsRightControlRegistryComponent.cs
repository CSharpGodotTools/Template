using System;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Registers custom right-side controls for options UI rows.
/// </summary>
internal sealed class OptionsRightControlRegistryComponent
{
    private readonly Dictionary<string, int> _rightControlIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, RegisteredRightControl> _rightControls = [];
    private int _nextRightControlId;

    /// <summary>
    /// Raised when a right control definition is registered.
    /// </summary>
    public event Action<RegisteredRightControl>? RightControlRegistered;

    /// <summary>
    /// Gets registered right controls ordered by deterministic id.
    /// </summary>
    /// <returns>Ordered registered right controls.</returns>
    public IEnumerable<RegisteredRightControl> GetRightControls()
    {
        return _rightControls.Values.OrderBy(control => control.Id);
    }

    /// <summary>
    /// Registers or updates a right control entry keyed by tab/target/name.
    /// </summary>
    /// <param name="definition">Right control definition to register.</param>
    public void AddRightControl(OptionRightControlDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        OptionValidator.ValidateTab(definition.Tab);
        OptionValidator.ValidateLabel(definition.TargetLabel, "RightControl target");
        OptionValidator.ValidateLabel(definition.Name, "RightControl name");

        string key = CreateRightControlKey(definition.Tab, definition.TargetLabel, definition.Name);
        // Allocate a new stable id only when this right-control key is new.
        if (!_rightControlIds.TryGetValue(key, out int id))
        {
            id = ++_nextRightControlId;
            _rightControlIds[key] = id;
        }

        RegisteredRightControl registered = new(id, definition);
        _rightControls[id] = registered;
        RightControlRegistered?.Invoke(registered);
    }

    /// <summary>
    /// Builds a stable key used to deduplicate right control registrations.
    /// </summary>
    /// <param name="tab">Tab identifier.</param>
    /// <param name="targetLabel">Target option label.</param>
    /// <param name="name">Right control name.</param>
    /// <returns>Normalized composite key.</returns>
    private static string CreateRightControlKey(string tab, string targetLabel, string name)
    {
        return $"{tab.Trim()}::{targetLabel.Trim()}::{name.Trim()}";
    }
}
