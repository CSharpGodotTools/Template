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
    /// <param name="custom">Custom options UI owner.</param>
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

        List<RegisteredRightControl> rightControls = [.. custom.OptionsManager.GetRightControls()];
        rightControls.Sort(static (left, right) => left.Id.CompareTo(right.Id));

        foreach (RegisteredRightControl rightControl in rightControls)
            AddOrReplaceRightControl(custom, rightControl);
    }

    // Event handlers for runtime registration
    /// <summary>
    /// Handles slider registration after runtime option additions.
    /// </summary>
    /// <param name="c">Custom options UI owner.</param>
    /// <param name="s">Registered slider option.</param>
    public static void OnSliderRegistered(OptionsCustom c, RegisteredSliderOption s) => AddOrReplace(c, new CustomOptionDescriptor(s));

    /// <summary>
    /// Handles dropdown registration after runtime option additions.
    /// </summary>
    /// <param name="c">Custom options UI owner.</param>
    /// <param name="d">Registered dropdown option.</param>
    public static void OnDropdownRegistered(OptionsCustom c, RegisteredDropdownOption d) => AddOrReplace(c, new CustomOptionDescriptor(d));

    /// <summary>
    /// Handles line-edit registration after runtime option additions.
    /// </summary>
    /// <param name="c">Custom options UI owner.</param>
    /// <param name="l">Registered line-edit option.</param>
    public static void OnLineEditRegistered(OptionsCustom c, RegisteredLineEditOption l) => AddOrReplace(c, new CustomOptionDescriptor(l));

    /// <summary>
    /// Handles toggle registration after runtime option additions.
    /// </summary>
    /// <param name="c">Custom options UI owner.</param>
    /// <param name="t">Registered toggle option.</param>
    public static void OnToggleRegistered(OptionsCustom c, RegisteredToggleOption t) => AddOrReplace(c, new CustomOptionDescriptor(t));

    /// <summary>
    /// Handles right-control registration after runtime additions.
    /// </summary>
    /// <param name="c">Custom options UI owner.</param>
    /// <param name="r">Registered right-control definition.</param>
    public static void OnRightControlRegistered(OptionsCustom c, RegisteredRightControl r) => AddOrReplaceRightControl(c, r);

    /// <summary>
    /// Adds or replaces a custom option row binding.
    /// </summary>
    /// <param name="custom">Custom options UI owner.</param>
    /// <param name="option">Descriptor for option to bind.</param>
    private static void AddOrReplace(OptionsCustom custom, CustomOptionDescriptor option)
    {
        // Skip registration when target tab/button cannot be resolved.
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

        // Store binding only when UI creation succeeded.
        if (binding != null)
            custom.Bindings.Add(option.Id, binding);

        foreach (RegisteredRightControl rightControl in custom.OptionsManager.GetRightControls())
            AddOrReplaceRightControl(custom, rightControl);

        custom.Nav.EnsureCurrentTabSelection();
    }

    /// <summary>
    /// Adds or replaces a right-side control binding for an option row.
    /// </summary>
    /// <param name="custom">Custom options UI owner.</param>
    /// <param name="rightControl">Registered right-control definition.</param>
    private static void AddOrReplaceRightControl(OptionsCustom custom, RegisteredRightControl rightControl)
    {
        OptionRightControlDefinition definition = rightControl.Definition;

        // Skip when target tab container does not exist.
        if (!custom.Nav.TryGetTabContainer(definition.Tab, out VBoxContainer tabContainer))
            return;

        // Skip when target row label cannot be found.
        if (!TryFindRowByLabel(tabContainer, definition.TargetLabel, out HBoxContainer row))
            return;

        // Skip when row does not expose a primary control anchor.
        if (!OptionRowFactory.TryGetPrimaryControl(row, out Control anchorControl))
            return;

        // Skip when row does not expose a controls container.
        if (!OptionRowFactory.TryGetControlsContainer(row, out HBoxContainer controlsContainer))
            return;

        // Dispose existing right-control binding before replacement.
        if (custom.RightControlBindings.TryGetValue(rightControl.Id, out IDisposable? existing))
        {
            existing.Dispose();
            custom.RightControlBindings.Remove(rightControl.Id);
        }

        RightControlBinding binding = RightControlBinding.Create(controlsContainer, anchorControl, rightControl);
        custom.RightControlBindings.Add(rightControl.Id, binding);
    }

    /// <summary>
    /// Finds a row by its label text in a tab container.
    /// </summary>
    /// <param name="tabContainer">Tab container to scan.</param>
    /// <param name="targetLabel">Label text to match.</param>
    /// <param name="row">Matched row when found.</param>
    /// <returns><see langword="true"/> when a matching row is found.</returns>
    private static bool TryFindRowByLabel(VBoxContainer tabContainer, string targetLabel, out HBoxContainer row)
    {
        foreach (Node child in tabContainer.GetChildren())
        {
            // Ignore non-row controls in the tab container.
            if (child is not HBoxContainer candidate)
                continue;

            // Ignore rows that do not start with a label control.
            if (candidate.GetChildCount() == 0 || candidate.GetChild(0) is not Label label)
                continue;

            // Ignore rows whose label does not match target text.
            if (!string.Equals(label.Text, targetLabel, StringComparison.OrdinalIgnoreCase))
                continue;

            row = candidate;
            return true;
        }

        row = null!;
        return false;
    }

    /// <summary>
    /// Sorts descriptors by tab name and then stable registration id.
    /// </summary>
    /// <param name="left">Left descriptor.</param>
    /// <param name="right">Right descriptor.</param>
    /// <returns>Comparison result for sort ordering.</returns>
    private static int CompareByTabThenOrder(CustomOptionDescriptor left, CustomOptionDescriptor right)
    {
        int tab = string.Compare(left.Tab, right.Tab, StringComparison.OrdinalIgnoreCase);
        return tab != 0 ? tab : left.Id.CompareTo(right.Id);
    }
}
