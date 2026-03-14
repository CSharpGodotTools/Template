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
    /// Collects all registered options, sorts by tab then order, and builds UI controls.
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
    }

    // Event handlers for runtime registration
    public static void OnSliderRegistered(OptionsCustom c, RegisteredSliderOption s) => AddOrReplace(c, new CustomOptionDescriptor(s));
    public static void OnDropdownRegistered(OptionsCustom c, RegisteredDropdownOption d) => AddOrReplace(c, new CustomOptionDescriptor(d));
    public static void OnLineEditRegistered(OptionsCustom c, RegisteredLineEditOption l) => AddOrReplace(c, new CustomOptionDescriptor(l));
    public static void OnToggleRegistered(OptionsCustom c, RegisteredToggleOption t) => AddOrReplace(c, new CustomOptionDescriptor(t));

    private static void AddOrReplace(OptionsCustom custom, CustomOptionDescriptor option)
    {
        if (!custom.Nav.TryGetTabContainer(option.Tab, out VBoxContainer tabContainer))
            return;

        Button navButton = GetNavButton(option.Tab, custom.Nav);
        if (navButton == null)
            return;

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
    }

    private static int CompareByTabThenOrder(CustomOptionDescriptor left, CustomOptionDescriptor right)
    {
        int tab = left.Tab.CompareTo(right.Tab);
        if (tab != 0) return tab;
        int order = left.Order.CompareTo(right.Order);
        return order != 0 ? order : left.Id.CompareTo(right.Id);
    }

    internal static Button GetNavButton(OptionsTab tab, OptionsNav nav)
    {
        Button? result = tab switch
        {
            OptionsTab.General => nav.GeneralButton,
            OptionsTab.Gameplay => nav.GameplayButton,
            OptionsTab.Display => nav.DisplayButton,
            OptionsTab.Graphics => nav.GraphicsButton,
            OptionsTab.Audio => nav.AudioButton,
            OptionsTab.Input => nav.InputButton,
            _ => null
        };

        if (result == null)
            throw new NotSupportedException($"Unsupported OptionsTab: {tab}");

        return result;
    }
}
