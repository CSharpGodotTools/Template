#if DEBUG
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using static Godot.Control;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualDictionary(Type type, VisualControlContext context)
    {
        VBoxContainer dictionaryVBox = new() { SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand };
        Button addButton = new() { Text = "+" };

        Type[] genericArguments = type.GetGenericArguments();
        Type keyType = genericArguments[0];
        Type valueType = genericArguments[1];

        object defaultKey = VisualMethods.CreateDefaultValue(keyType);
        object defaultValue = VisualMethods.CreateDefaultValue(valueType);

        IDictionary dictionary = context.InitialValue as IDictionary ?? (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));

        foreach (DictionaryEntry entry in dictionary)
        {
            AddEntry(entry.Key, entry.Value);
        }

        addButton.Pressed += () =>
        {
            if (dictionary.Contains(defaultKey))
            {
                return;
            }

            dictionary[defaultKey] = defaultValue;
            context.ValueChanged(dictionary);
            AddEntry(defaultKey, defaultValue);
            dictionaryVBox.MoveChild(addButton, dictionaryVBox.GetChildCount() - 1);
        };
        dictionaryVBox.AddChild(addButton);

        return new VisualControlInfo(new VBoxContainerControl(dictionaryVBox));

        void AddEntry(object key, object value)
        {
            object currentKey = key;

            VisualControlInfo valueControl = CreateControlForType(valueType, null, new VisualControlContext(value, v =>
            {
                dictionary[currentKey] = v;
                context.ValueChanged(dictionary);
            }));

            VisualControlInfo keyControl = CreateControlForType(keyType, null, new VisualControlContext(currentKey, v =>
            {
                if (v == null || dictionary.Contains(v))
                {
                    return;
                }

                if (v.GetType() != keyType)
                {
                    throw new ArgumentException($"[Visualize] Type mismatch: Expected {keyType}, got {v.GetType()}");
                }

                object currentValue = dictionary[currentKey];
                dictionary.Remove(currentKey);
                dictionary[v] = currentValue;
                currentKey = v;
                context.ValueChanged(dictionary);
                valueControl.VisualControl.SetValue(defaultValue);
            }));

            if (keyControl.VisualControl == null || valueControl.VisualControl == null)
            {
                return;
            }

            keyControl.VisualControl.SetValue(currentKey);
            valueControl.VisualControl.SetValue(value);

            Button removeKeyEntryButton = new() { Text = "-" };
            HBoxContainer hbox = new();

            removeKeyEntryButton.Pressed += () =>
            {
                dictionaryVBox.RemoveChild(hbox);
                dictionary.Remove(currentKey);
                context.ValueChanged(dictionary);
            };

            hbox.AddChild(keyControl.VisualControl.Control);
            hbox.AddChild(valueControl.VisualControl.Control);
            hbox.AddChild(removeKeyEntryButton);
            dictionaryVBox.AddChild(hbox);
        }
    }
}
#endif
