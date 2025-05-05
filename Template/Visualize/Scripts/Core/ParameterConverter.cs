using System;
using System.Reflection;

namespace Template;

/// <summary>
/// Utility class for converting provided values to method parameters
/// </summary>
public static class ParameterConverter
{
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

    private static void ValidateInput(ParameterInfo[] paramInfos, object[] providedValues)
    {
        ArgumentNullException.ThrowIfNull(paramInfos);
        ArgumentNullException.ThrowIfNull(providedValues);

        if (paramInfos.Length != providedValues.Length)
        {
            throw new ArgumentException("The number of provided values does not match the number of method parameters.");
        }
    }

    private static object ConvertParameter(ParameterInfo paramInfo, object providedValue)
    {
        if (providedValue == null)
        {
            return GetDefaultValue(paramInfo.ParameterType);
        }

        if (!paramInfo.ParameterType.IsAssignableFrom(providedValue.GetType()))
        {
            throw new InvalidOperationException($"The provided value for parameter '{paramInfo.Name}' is not assignable to the parameter type '{paramInfo.ParameterType}'.");
        }

        return providedValue;
    }

    private static object GetDefaultValue(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }
}
