#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Builds dictionary controls using adapter-based access for typed and untyped dictionary variants.
/// </summary>
/// <param name="adapterFactory">Factory that creates dictionary adapters for runtime dictionary instances.</param>
/// <param name="keyResolver">Resolver used to parse and validate key input from UI controls.</param>
/// <param name="displayOrderTrackerFactory">Factory for stable dictionary entry display-order tracking.</param>
/// <param name="createControlForType">Factory that builds value controls for runtime types.</param>
/// <param name="createDefaultValue">Factory that creates default key/value instances for add-entry flows.</param>
/// <param name="cleanupOnTreeExited">Callback used to register cleanup when controls leave the scene tree.</param>
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

    /// <summary>
    /// Creates a dictionary control for the provided runtime dictionary type.
    /// </summary>
    /// <param name="dictionaryType">Dictionary runtime type.</param>
    /// <param name="context">Initial dictionary value and change callback context.</param>
    /// <returns>Created visual-control info.</returns>
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

    /// <summary>
    /// Resolves key/value type metadata and defaults for dictionary rendering.
    /// </summary>
    /// <param name="dictionaryType">Dictionary runtime type.</param>
    /// <returns>Dictionary type-info payload used by control component.</returns>
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
