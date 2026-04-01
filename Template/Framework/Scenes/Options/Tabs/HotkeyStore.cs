using Godot;
using System.Collections.Generic;
using System.Linq;

namespace __TEMPLATE__.Ui;

public partial class OptionsInput
{
    /// <summary>
    /// Thin adapter over option-managed hotkey data and InputMap synchronization.
    /// </summary>
    public sealed class HotkeyStore
    {
        private readonly OptionsManager _options;

        /// <summary>
        /// Creates a store facade for reading and mutating configured hotkeys.
        /// </summary>
        /// <param name="options">Options manager that owns persistent hotkey settings.</param>
        public HotkeyStore(OptionsManager options)
        {
            _options = options;
        }

        /// <summary>
        /// Gets the current action-to-events map from configured hotkeys.
        /// </summary>
        public Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> Actions => _options.GetHotkeys().Actions;

        /// <summary>
        /// Gets all bound events for a specific input action.
        /// </summary>
        /// <param name="action">Input action to inspect.</param>
        /// <returns>Collection of events currently bound to the action.</returns>
        public Godot.Collections.Array<InputEvent> GetEvents(StringName action)
        {
            return Actions[action];
        }

        /// <summary>
        /// Returns action names sorted lexicographically for stable UI ordering.
        /// </summary>
        /// <returns>Sorted action names.</returns>
        public IEnumerable<StringName> GetOrderedActions()
        {
            return Actions.Keys.OrderBy(x => x.ToString());
        }

        /// <summary>
        /// Removes one event binding from an action.
        /// </summary>
        /// <param name="action">Action whose binding should be removed.</param>
        /// <param name="event">Bound event to remove.</param>
        public void RemoveEvent(StringName action, InputEvent @event)
        {
            // Ignore remove requests without a concrete event value.
            if (@event == null)
                return;

            Actions[action].Remove(@event);
        }

        /// <summary>
        /// Replaces one existing action binding with a new input event.
        /// </summary>
        /// <param name="action">Action whose binding should be replaced.</param>
        /// <param name="oldEvent">Existing event to remove.</param>
        /// <param name="newEvent">New event to add.</param>
        public void ReplaceEvent(StringName action, InputEvent oldEvent, InputEvent newEvent)
        {
            Actions[action].Remove(oldEvent);
            Actions[action].Add(newEvent);
        }

        /// <summary>
        /// Checks whether the candidate event already exists in the action's bindings.
        /// </summary>
        /// <param name="action">Action to search for duplicates.</param>
        /// <param name="candidate">Candidate event to compare.</param>
        /// <returns><see langword="true"/> when an equivalent binding already exists.</returns>
        public bool HasDuplicate(StringName action, InputEvent candidate)
        {
            Godot.Collections.Array<InputEvent> events = Actions[action];
            for (int i = 0; i < events.Count; i++)
            {
                // Return immediately when an equivalent binding already exists.
                if (EventsMatch(events[i], candidate))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Restores hotkey settings to project defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            _options.ResetHotkeys();
        }

        /// <summary>
        /// Temporarily clears all runtime InputMap bindings for an action.
        /// </summary>
        /// <param name="action">Action to clear from the active InputMap.</param>
        public static void SuppressAction(StringName action)
        {
            Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
            for (int i = 0; i < events.Count; i++)
            {
                InputMap.ActionEraseEvent(action, events[i]);
            }
        }

        /// <summary>
        /// Replaces runtime InputMap events for an action with the store's current bindings.
        /// </summary>
        /// <param name="action">Action to synchronize.</param>
        public void SyncAction(StringName action)
        {
            Godot.Collections.Array<InputEvent> existing = InputMap.ActionGetEvents(action);
            for (int i = 0; i < existing.Count; i++)
            {
                InputMap.ActionEraseEvent(action, existing[i]);
            }

            Godot.Collections.Array<InputEvent> stored = Actions[action];
            for (int i = 0; i < stored.Count; i++)
            {
                InputMap.ActionAddEvent(action, stored[i]);
            }
        }

        /// <summary>
        /// Compares two input events for hotkey-equivalent meaning.
        /// </summary>
        /// <param name="left">First event to compare.</param>
        /// <param name="right">Second event to compare.</param>
        /// <returns><see langword="true"/> when both events represent the same binding.</returns>
        private static bool EventsMatch(InputEvent left, InputEvent right)
        {
            // Treat identical references as an immediate match.
            if (ReferenceEquals(left, right))
                return true;

            // Null on either side cannot represent a valid match.
            if (left == null || right == null)
                return false;

            // Compare keyboard events using keycodes plus modifier state.
            if (left is InputEventKey leftKey && right is InputEventKey rightKey)
            {
                // Mismatched modifiers mean keys are not equivalent.
                if (!ModifiersMatch(leftKey, rightKey))
                    return false;

                return KeyMatch(leftKey.Keycode, rightKey.Keycode)
                    || KeyMatch(leftKey.PhysicalKeycode, rightKey.PhysicalKeycode)
                    || KeyMatch(leftKey.Keycode, rightKey.PhysicalKeycode)
                    || KeyMatch(leftKey.PhysicalKeycode, rightKey.Keycode);
            }

            // Compare mouse buttons using button index plus modifier state.
            if (left is InputEventMouseButton leftMouse && right is InputEventMouseButton rightMouse)
            {
                return leftMouse.ButtonIndex == rightMouse.ButtonIndex
                    && ModifiersMatch(leftMouse, rightMouse);
            }

            return false;
        }

        /// <summary>
        /// Compares modifier keys for two modifier-capable input events.
        /// </summary>
        /// <param name="left">First event modifiers.</param>
        /// <param name="right">Second event modifiers.</param>
        /// <returns><see langword="true"/> when all modifier states are equal.</returns>
        private static bool ModifiersMatch(InputEventWithModifiers left, InputEventWithModifiers right)
        {
            return left.ShiftPressed == right.ShiftPressed
                && left.AltPressed == right.AltPressed
                && left.CtrlPressed == right.CtrlPressed
                && left.MetaPressed == right.MetaPressed;
        }

        /// <summary>
        /// Compares two key values, treating zero as an unset key.
        /// </summary>
        /// <param name="left">First key value.</param>
        /// <param name="right">Second key value.</param>
        /// <returns><see langword="true"/> when both non-zero keys are equal.</returns>
        private static bool KeyMatch(Key left, Key right)
        {
            int leftValue = (int)left;
            int rightValue = (int)right;

            return leftValue != 0 && rightValue != 0 && leftValue == rightValue;
        }
    }
}
