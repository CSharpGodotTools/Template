#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal sealed class VisualDictionaryControlProvider(
    IVisualDictionaryAdapterFactory adapterFactory,
    IVisualDictionaryKeyResolver keyResolver,
    IVisualDictionaryDisplayOrderTrackerFactory displayOrderTrackerFactory,
    Func<Type, VisualControlContext, VisualControlInfo> createControlForType,
    Func<Type, object> createDefaultValue,
    Action<Node, Action> cleanupOnTreeExited)
{
    private readonly IVisualDictionaryAdapterFactory _adapterFactory = adapterFactory;
    private readonly IVisualDictionaryKeyResolver _keyResolver = keyResolver;
    private readonly IVisualDictionaryDisplayOrderTrackerFactory _displayOrderTrackerFactory = displayOrderTrackerFactory;
    private readonly Func<Type, VisualControlContext, VisualControlInfo> _createControlForType = createControlForType;
    private readonly Func<Type, object> _createDefaultValue = createDefaultValue;
    private readonly Action<Node, Action> _cleanupOnTreeExited = cleanupOnTreeExited;

    public VisualControlInfo CreateControl(Type dictionaryType, VisualControlContext context)
    {
        object dictionaryObject = context.InitialValue ?? Activator.CreateInstance(dictionaryType)!;
        VisualDictionaryTypeInfo typeInfo = BuildTypeInfo(dictionaryType);
        IVisualDictionaryAdapter adapter = _adapterFactory.Create(dictionaryObject, dictionaryType);
        IVisualDictionaryDisplayOrderTracker displayOrderTracker = _displayOrderTrackerFactory.Create(typeInfo.UseStableDisplayOrder);

        VisualDictionaryControlComponent component = new(
            typeInfo,
            dictionaryObject,
            adapter,
            context,
            _adapterFactory,
            _keyResolver,
            displayOrderTracker,
            _createControlForType,
            _cleanupOnTreeExited);

        return component.Build();
    }

    private VisualDictionaryTypeInfo BuildTypeInfo(Type dictionaryType)
    {
        Type[] genericArguments = dictionaryType.GetGenericArguments();
        bool useStableDisplayOrder = genericArguments.Length == 2;
        Type keyType = useStableDisplayOrder ? genericArguments[0] : typeof(object);
        Type valueType = useStableDisplayOrder ? genericArguments[1] : typeof(object);

        object defaultKey = keyType == typeof(object) ? "Key" : _createDefaultValue(keyType);
        object defaultValue = valueType == typeof(object) ? string.Empty : _createDefaultValue(valueType);

        return new VisualDictionaryTypeInfo(
            dictionaryType,
            keyType,
            valueType,
            useStableDisplayOrder,
            defaultKey,
            defaultValue);
    }
}
#endif
