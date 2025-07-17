using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using static Godot.Control;

namespace __TEMPLATE__;

public static partial class VisualControlTypes
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
            object key = entry.Key;
            object value = entry.Value;

            VisualControlInfo valueControl = CreateControlForType(valueType, null, new VisualControlContext(context.SpinBoxes, value, v =>
            {
                dictionary[key] = v;
                context.ValueChanged(dictionary);
            }));

            VisualControlInfo keyControl = CreateControlForType(keyType, null, new VisualControlContext(context.SpinBoxes, key, v =>
            {
                if (dictionary.Contains(v))
                {
                    return;
                }

                if (v.GetType() != keyType)
                {
                    throw new ArgumentException($"[Visualize] Type mismatch: Expected {keyType}, got {v.GetType()}");
                }

                dictionary.Remove(key);
                dictionary[v] = value;
                key = v;
                context.ValueChanged(dictionary);
                SetControlValue(valueControl.VisualControl.Control, defaultValue);
            }));

            if (keyControl.VisualControl != null && valueControl.VisualControl != null)
            {
                SetControlValue(keyControl.VisualControl.Control, key);
                SetControlValue(valueControl.VisualControl.Control, value);

                Button removeKeyEntryButton = new() { Text = "-" };
                HBoxContainer hbox = new();

                removeKeyEntryButton.Pressed += () =>
                {
                    dictionaryVBox.RemoveChild(hbox);
                    dictionary.Remove(key);
                    context.ValueChanged(dictionary);
                };

                hbox.AddChild(keyControl.VisualControl.Control);
                hbox.AddChild(valueControl.VisualControl.Control);
                hbox.AddChild(removeKeyEntryButton);
                dictionaryVBox.AddChild(hbox);
            }
        }

        void AddNewEntryToDictionary()
        {
            if (dictionary.Contains(defaultKey))
            {
                return;
            }

            dictionary[defaultKey] = defaultValue;
            context.ValueChanged(dictionary);

            object oldKey = defaultKey;

            VisualControlInfo valueControl = CreateControlForType(valueType, null, new VisualControlContext(context.SpinBoxes, defaultValue, v =>
            {
                dictionary[oldKey] = v;
                context.ValueChanged(dictionary);
            }));

            VisualControlInfo keyControl = CreateControlForType(keyType, null, new VisualControlContext(context.SpinBoxes, defaultKey, v =>
            {
                if (dictionary.Contains(v))
                {
                    return;
                }

                if (v.GetType() != keyType)
                {
                    throw new ArgumentException($"[Visualize] Type mismatch: Expected {keyType}, got {v.GetType()}");
                }

                dictionary.Remove(oldKey);
                dictionary[v] = defaultValue;
                oldKey = v;
                context.ValueChanged(dictionary);
                SetControlValue(valueControl.VisualControl.Control, defaultValue);
            }));

            if (keyControl.VisualControl != null && valueControl.VisualControl != null)
            {
                Button removeKeyEntryButton = new() { Text = "-" };
                HBoxContainer hbox = new();

                removeKeyEntryButton.Pressed += () =>
                {
                    dictionaryVBox.RemoveChild(hbox);
                    dictionary.Remove(oldKey);
                    context.ValueChanged(dictionary);
                };

                hbox.AddChild(keyControl.VisualControl.Control);
                hbox.AddChild(valueControl.VisualControl.Control);
                hbox.AddChild(removeKeyEntryButton);
                dictionaryVBox.AddChild(hbox);
                dictionaryVBox.MoveChild(addButton, dictionaryVBox.GetChildCount() - 1);
            }
        }

        addButton.Pressed += AddNewEntryToDictionary;
        dictionaryVBox.AddChild(addButton);

        return new VisualControlInfo(new VBoxContainerControl(dictionaryVBox));
    }
}
