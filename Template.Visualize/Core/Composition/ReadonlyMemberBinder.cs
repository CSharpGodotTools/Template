#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace GodotUtils.Debugging;

/// <summary>
/// Builds readonly visualization controls and update actions for members listed by name.
/// </summary>
internal sealed class ReadonlyMemberBinder
{
    private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    private const int PollDelayMilliseconds = 1000;
    private const int ClassReadonlyRowSeparation = 4;
    private static readonly Color _hiddenGhostTitleColor = new(1, 1, 1, 0);

    private readonly List<Action> _updateActions = [];

    /// <summary>
    /// Per-frame actions that refresh readonly control values.
    /// </summary>
    public IReadOnlyList<Action> UpdateActions => _updateActions;

    /// <summary>
    /// Adds readonly visualization controls for the requested member names.
    /// </summary>
    /// <param name="visualizeMembers">Member names to bind.</param>
    /// <param name="target">Target object that owns the members.</param>
    /// <param name="readonlyMembers">Readonly members container.</param>
    /// <param name="displayName">Display name used in diagnostics.</param>
    public void AddReadonlyControls(string[] visualizeMembers, object target, Control readonlyMembers, string displayName)
    {
        // Null member lists are treated as invalid input and logged for diagnostics.
        if (visualizeMembers == null)
        {
            GD.PrintErr($"[Visualize] AddReadonlyControls called with null members on node '{displayName}'");
            return;
        }

        foreach (string visualMember in visualizeMembers)
        {
            // Skip names that do not resolve to readable fields/properties.
            if (!TryCreateMemberAccessor(target, visualMember, out ReadonlyMemberAccessor? accessor))
            {
                continue;
            }

            object? initialValue = accessor.GetValue(target);

            // Add immediate controls for members with available initial values.
            if (initialValue != null)
            {
                AddReadonlyControl(accessor, readonlyMembers, target, initialValue);
            }
            // Delay control creation for members that become available later.
            else
            {
                _ = TryAddReadonlyControlAsync(accessor, readonlyMembers, target, displayName);
            }
        }
    }

    /// <summary>
    /// Attempts to create a reflection accessor for a named readable property or field.
    /// </summary>
    /// <param name="target">Target object providing member metadata.</param>
    /// <param name="visualMember">Member name to resolve.</param>
    /// <param name="accessor">Created accessor when resolution succeeds.</param>
    /// <returns><see langword="true"/> when member resolution succeeds.</returns>
    private static bool TryCreateMemberAccessor(object target, string visualMember, [NotNullWhen(true)] out ReadonlyMemberAccessor? accessor)
    {
        Type targetType = target.GetType();

        PropertyInfo? property = targetType.GetProperty(visualMember, MemberBindingFlags);
        // Prefer properties with getters when both property and field names overlap.
        if (property != null && property.GetGetMethod(true) != null)
        {
            accessor = new ReadonlyMemberAccessor(visualMember, property, property.PropertyType);
            return true;
        }

        FieldInfo? field = targetType.GetField(visualMember, MemberBindingFlags);
        // Fall back to fields when no readable property exists.
        if (field != null)
        {
            accessor = new ReadonlyMemberAccessor(visualMember, field, field.FieldType);
            return true;
        }

        accessor = null;
        return false;
    }

    /// <summary>
    /// Polls for an initial member value before creating its readonly control.
    /// </summary>
    /// <param name="accessor">Accessor used to read member values.</param>
    /// <param name="readonlyMembers">Readonly members container.</param>
    /// <param name="target">Target object owning the member.</param>
    /// <param name="displayName">Display name used for status logging.</param>
    /// <returns>Task that completes once polling finishes or times out.</returns>
    private async Task TryAddReadonlyControlAsync(ReadonlyMemberAccessor accessor, Control readonlyMembers, object target, string displayName)
    {
        int elapsedSeconds = 0;

        while (true)
        {
            object? value = accessor.GetValue(target);

            // Create the control as soon as the member produces a concrete value.
            if (value != null)
            {
                AddReadonlyControl(accessor, readonlyMembers, target, value);
                break;
            }

            await Task.Delay(PollDelayMilliseconds);
            elapsedSeconds++;

            // Stop polling after timeout and keep tracking through update actions only.
            if (elapsedSeconds == VisualUiLayout.MaxSecondsToWaitForInitialValues)
            {
                GD.PrintRich($"[color=orange][Visualize] Tracking '{displayName}' to see if '{accessor.Name}' value changes[/color]");
                break;
            }
        }
    }

    /// <summary>
    /// Creates and adds a readonly control for a resolved member accessor.
    /// </summary>
    /// <param name="accessor">Accessor describing the readonly member.</param>
    /// <param name="readonlyMembers">Readonly members container.</param>
    /// <param name="target">Target object owning the member.</param>
    /// <param name="initialValue">Initial member value.</param>
    private void AddReadonlyControl(ReadonlyMemberAccessor accessor, Control readonlyMembers, object target, object initialValue)
    {
        VisualControlContext context = new(initialValue, _ =>
        {
            // Do nothing
        });

        VisualControlInfo visualControlInfo = VisualControlTypes.CreateControlForType(accessor.MemberType, accessor.Member, context);

        // Unsupported member types are skipped to avoid adding unusable controls.
        if (visualControlInfo.VisualControl == null)
        {
            return;
        }

        IVisualControl visualControl = visualControlInfo.VisualControl;

        // Readonly column should never accept user edits.
        visualControl.SetEditable(false);

        // Nested class controls hide internal labels in readonly column layout.
        if (visualControl is ClassControl)
        {
            SetNestedLabelsVisible(visualControl.Control, false);
        }

        _updateActions.Add(() =>
        {
            object? current = accessor.GetValue(target);

            // Push updates only when current value is available.
            if (current is not null)
            {
                visualControl.SetValue(current);
            }
        });

        HBoxContainer hbox = new()
        {
            Name = accessor.Name,
            Alignment = BoxContainer.AlignmentMode.Begin
        };

        Label label = new()
        {
            Text = VisualText.ToDisplayName(accessor.Name),
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(VisualUiLayout.MemberLabelMinWidth, 0),
            Visible = false
        };

        hbox.AddChild(label);

        hbox.AddChild(visualControl.Control);

        // Class controls use a two-row structure with a hidden ghost title for spacing alignment.
        if (visualControl is ClassControl)
        {
            Label ghostTitle = new()
            {
                Name = "ClassTitleGhost",
                Text = VisualText.ToDisplayName(accessor.Name),
                HorizontalAlignment = HorizontalAlignment.Right,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SelfModulate = _hiddenGhostTitleColor,
                LabelSettings = new LabelSettings
                {
                    FontSize = VisualUiLayout.MemberFontSize,
                    FontColor = Colors.White,
                    OutlineSize = VisualUiLayout.FontOutlineSize,
                    OutlineColor = Colors.Black,
                }
            };

            VBoxContainer classReadonlyRow = new()
            {
                Name = accessor.Name,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            classReadonlyRow.AddThemeConstantOverride("separation", ClassReadonlyRowSeparation);
            classReadonlyRow.AddChild(ghostTitle);
            classReadonlyRow.AddChild(hbox);
            readonlyMembers.AddChild(classReadonlyRow);
            return;
        }

        readonlyMembers.AddChild(hbox);
    }

    /// <summary>
    /// Recursively toggles label visibility for nested readonly controls.
    /// </summary>
    /// <param name="root">Root control to traverse.</param>
    /// <param name="visible">Desired label visibility.</param>
    private static void SetNestedLabelsVisible(Control root, bool visible)
    {
        foreach (Node child in root.GetChildren())
        {
            // Apply visibility toggle to labels found at this depth.
            if (child is Label label)
            {
                label.Visible = visible;
            }

            // Continue traversal through nested controls.
            if (child is Control childControl)
            {
                SetNestedLabelsVisible(childControl, visible);
            }
        }
    }

}
#endif
