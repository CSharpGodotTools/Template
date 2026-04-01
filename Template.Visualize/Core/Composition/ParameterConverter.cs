#if DEBUG
using System;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Converts UI-provided values into a method-invocation parameter array.
/// </summary>
internal static class ParameterConverter
{
    /// <summary>
    /// Converts edited parameter values to an invocation-ready argument array.
    /// </summary>
    /// <param name="paramInfos">Method parameter metadata.</param>
    /// <param name="providedValues">Values collected from parameter controls.</param>
    /// <returns>Converted argument array in parameter order.</returns>
    public static object[] ConvertParameterInfoToObjectArray(ParameterInfo[] paramInfos, object[] providedValues)
    {
        ValidateInput(paramInfos, providedValues);

        object[] parameters = new object[paramInfos.Length];

        for (int i = 0; i < paramInfos.Length; i++)
        {
            parameters[i] = ConvertParameter(paramInfos[i], providedValues[i]);
        }

        return parameters;
    }

    /// <summary>
    /// Validates input arrays used for argument conversion.
    /// </summary>
    /// <param name="paramInfos">Method parameter metadata.</param>
    /// <param name="providedValues">Values collected from controls.</param>
    private static void ValidateInput(ParameterInfo[] paramInfos, object[] providedValues)
    {
        ArgumentNullException.ThrowIfNull(paramInfos);
        ArgumentNullException.ThrowIfNull(providedValues);

        // Conversion requires a one-to-one value mapping for each method parameter.
        if (paramInfos.Length != providedValues.Length)
        {
            throw new ArgumentException("The number of provided values does not match the number of method parameters.");
        }
    }

    /// <summary>
    /// Converts a single provided value to a parameter-compatible value.
    /// </summary>
    /// <param name="paramInfo">Target parameter metadata.</param>
    /// <param name="providedValue">Provided value from controls.</param>
    /// <returns>Converted value suitable for method invocation.</returns>
    private static object ConvertParameter(ParameterInfo paramInfo, object providedValue)
    {
        // Null values map to type-specific defaults.
        if (providedValue == null)
        {
            return GetDefaultValue(paramInfo.ParameterType);
        }

        // Reject values that cannot be assigned to the parameter type.
        if (!paramInfo.ParameterType.IsAssignableFrom(providedValue.GetType()))
        {
            throw new InvalidOperationException($"The provided value for parameter '{paramInfo.Name}' is not assignable to the parameter type '{paramInfo.ParameterType}'.");
        }

        return providedValue;
    }

    /// <summary>
    /// Returns a default value for a parameter type when no value was provided.
    /// </summary>
    /// <param name="type">Parameter type.</param>
    /// <returns>Default value for the requested type.</returns>
    private static object GetDefaultValue(Type type)
    {
        // Preserve existing behavior where string defaults to null.
        if (type == typeof(string))
        {
            return null!;
        }

        // Value types default to a zero-initialized instance.
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type)!;
        }

        return null!;
    }
}
#endif
