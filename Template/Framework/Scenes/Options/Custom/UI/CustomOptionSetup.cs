using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Handles initial population and runtime registration of custom option
/// UI controls inside the options screen.
/// </summary>
internal static class CustomOptionSetup
{
    /// <summary>
    /// Collects all registered options, sorts by tab then registration order, and builds UI controls.
    /// </summary>
    public static void Setup(OptionsCustom custom)
    {
        List<CustomOptionDescriptor> options = [];
        options.AddRange(custom.OptionsManager.GetSliderOptions().Select(s => new CustomOptionDescriptor(s)));
        options.AddRange(custom.OptionsManager.GetDropdownOptions().Select(d => new CustomOptionDescriptor(d)));
        options.AddRange(custom.OptionsManager.GetLineEditOptions().Select(l => new CustomOptionDescriptor(l)));
        options.AddRange(custom.OptionsManager.GetToggleOptions().Select(t => new CustomOptionDescriptor(t)));
        options.Sort(CompareByTabThenOrder);

        foreach (CustomOptionDescriptor option in options)
            AddOrReplace(custom, option);

        List<RegisteredRightControl> rightControls = custom.OptionsManager.GetRightControls().ToList();
        rightControls.Sort(static (left, right) => left.Id.CompareTo(right.Id));

        foreach (RegisteredRightControl rightControl in rightControls)
            AddOrReplaceRightControl(custom, rightControl);
    }

    // Event handlers for runtime registration
    public static void OnSliderRegistered(OptionsCustom c, RegisteredSliderOption s) => AddOrReplace(c, new CustomOptionDescriptor(s));
    public static void OnDropdownRegistered(OptionsCustom c, RegisteredDropdownOption d) => AddOrReplace(c, new CustomOptionDescriptor(d));
    public static void OnLineEditRegistered(OptionsCustom c, RegisteredLineEditOption l) => AddOrReplace(c, new CustomOptionDescriptor(l));
    public static void OnToggleRegistered(OptionsCustom c, RegisteredToggleOption t) => AddOrReplace(c, new CustomOptionDescriptor(t));
    public static void OnRightControlRegistered(OptionsCustom c, RegisteredRightControl r) => AddOrReplaceRightControl(c, r);

    private static void AddOrReplace(OptionsCustom custom, CustomOptionDescriptor option)
    {
        if (!custom.Nav.TryGetTab(option.Tab, out VBoxContainer tabContainer, out Button navButton))
            return;

        custom.Nav.SetTabEnabled(option.Tab, true, ensureSelection: false);

        // Dispose the existing binding when re‑registering an option
        if (custom.Bindings.TryGetValue(option.Id, out IDisposable? existing))
        {
            existing.Dispose();
            custom.Bindings.Remove(option.Id);
        }

        // Delegate UI creation to the appropriate binding type
        IDisposable? binding = option.Type switch
        {
            CustomOptionType.Slider => SliderBinding.Create(tabContainer, navButton, option.Slider!),
            CustomOptionType.Dropdown => DropdownBinding.Create(tabContainer, navButton, option.Dropdown!),
            CustomOptionType.LineEdit => LineEditBinding.Create(tabContainer, navButton, option.LineEdit!),
            CustomOptionType.Toggle => ToggleBinding.Create(tabContainer, navButton, option.Toggle!),
            _ => null
        };

        if (binding != null)
            custom.Bindings.Add(option.Id, binding);

        foreach (RegisteredRightControl rightControl in custom.OptionsManager.GetRightControls())
            AddOrReplaceRightControl(custom, rightControl);

        custom.Nav.EnsureCurrentTabSelection();
    }

    private static void AddOrReplaceRightControl(OptionsCustom custom, RegisteredRightControl rightControl)
    {
        OptionRightControlDefinition definition = rightControl.Definition;

        if (!custom.Nav.TryGetTabContainer(definition.Tab, out VBoxContainer tabContainer))
            return;

        if (!TryFindRowByLabel(tabContainer, definition.TargetLabel, out HBoxContainer row))
            return;

        if (!OptionRowFactory.TryGetPrimaryControl(row, out Control anchorControl))
            return;

        if (!OptionRowFactory.TryGetControlsContainer(row, out HBoxContainer controlsContainer))
            return;

        if (custom.RightControlBindings.TryGetValue(rightControl.Id, out IDisposable? existing))
        {
            existing.Dispose();
            custom.RightControlBindings.Remove(rightControl.Id);
        }

        RightControlBinding binding = RightControlBinding.Create(controlsContainer, anchorControl, rightControl);
        custom.RightControlBindings.Add(rightControl.Id, binding);
    }

    private static bool TryFindRowByLabel(VBoxContainer tabContainer, string targetLabel, out HBoxContainer row)
    {
        foreach (Node child in tabContainer.GetChildren())
        {
            if (child is not HBoxContainer candidate)
                continue;

            if (candidate.GetChildCount() == 0 || candidate.GetChild(0) is not Label label)
                continue;

            if (!string.Equals(label.Text, targetLabel, StringComparison.OrdinalIgnoreCase))
                continue;

            row = candidate;
            return true;
        }

        row = null!;
        return false;
    }

    private static int CompareByTabThenOrder(CustomOptionDescriptor left, CustomOptionDescriptor right)
    {
        int tab = string.Compare(left.Tab, right.Tab, StringComparison.OrdinalIgnoreCase);
        return tab != 0 ? tab : left.Id.CompareTo(right.Id);
    }
}
