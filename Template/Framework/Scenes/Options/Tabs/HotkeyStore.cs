using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Framework.UI;

public partial class OptionsInput
{
    public sealed class HotkeyStore
    {
        private readonly OptionsManager _options;

        public HotkeyStore(OptionsManager options)
        {
            _options = options;
        }

        public Godot.Collections.Dictionary<StringName, Godot.Collections.Array<InputEvent>> Actions => _options.GetHotkeys().Actions;

        public Godot.Collections.Array<InputEvent> GetEvents(StringName action)
        {
            return Actions[action];
        }

        public IEnumerable<StringName> GetOrderedActions()
        {
            return Actions.Keys.OrderBy(x => x.ToString());
        }

        public void RemoveEvent(StringName action, InputEvent @event)
        {
            if (@event == null)
                return;

            Actions[action].Remove(@event);
        }

        public void ReplaceEvent(StringName action, InputEvent oldEvent, InputEvent newEvent)
        {
            Actions[action].Remove(oldEvent);
            Actions[action].Add(newEvent);
        }

        public bool HasDuplicate(StringName action, InputEvent candidate)
        {
            Godot.Collections.Array<InputEvent> events = Actions[action];
            for (int i = 0; i < events.Count; i++)
            {
                if (EventsMatch(events[i], candidate))
                    return true;
            }

            return false;
        }

        public void ResetToDefaults()
        {
            _options.ResetHotkeys();
        }

        public static void SuppressAction(StringName action)
        {
            Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
            for (int i = 0; i < events.Count; i++)
            {
                InputMap.ActionEraseEvent(action, events[i]);
            }
        }

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

        private static bool EventsMatch(InputEvent left, InputEvent right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            if (left is InputEventKey leftKey && right is InputEventKey rightKey)
            {
                if (!ModifiersMatch(leftKey, rightKey))
                    return false;

                return KeyMatch(leftKey.Keycode, rightKey.Keycode)
                    || KeyMatch(leftKey.PhysicalKeycode, rightKey.PhysicalKeycode)
                    || KeyMatch(leftKey.Keycode, rightKey.PhysicalKeycode)
                    || KeyMatch(leftKey.PhysicalKeycode, rightKey.Keycode);
            }

            if (left is InputEventMouseButton leftMouse && right is InputEventMouseButton rightMouse)
            {
                return leftMouse.ButtonIndex == rightMouse.ButtonIndex
                    && ModifiersMatch(leftMouse, rightMouse);
            }

            return false;
        }

        private static bool ModifiersMatch(InputEventWithModifiers left, InputEventWithModifiers right)
        {
            return left.ShiftPressed == right.ShiftPressed
                && left.AltPressed == right.AltPressed
                && left.CtrlPressed == right.CtrlPressed
                && left.MetaPressed == right.MetaPressed;
        }

        private static bool KeyMatch(Key left, Key right)
        {
            int leftValue = (int)left;
            int rightValue = (int)right;

            return leftValue != 0 && rightValue != 0 && leftValue == rightValue;
        }
    }
}


