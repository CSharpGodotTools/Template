using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Template.Setup.Testing;

public static class PacketDiff
{
    public static string FindFirstDiff(object expected, object actual, string path = "")
    {
        if (ReferenceEquals(expected, actual))
        {
            return null;
        }

        if (expected is null || actual is null)
        {
            return $"{FormatPath(path)}: one is null (expected={FormatValue(expected)}, actual={FormatValue(actual)})";
        }

        Type expectedType = expected.GetType();
        Type actualType = actual.GetType();

        if (expectedType != actualType)
        {
            return $"{FormatPath(path)}: type mismatch expected={expectedType.FullName} actual={actualType.FullName}";
        }

        if (IsSimple(expectedType))
        {
            if (!Equals(expected, actual))
            {
                return $"{FormatPath(path)}: expected {FormatValue(expected)} actual {FormatValue(actual)}";
            }

            return null;
        }

        if (expected is IList expectedList && actual is IList actualList)
        {
            if (expectedList.Count != actualList.Count)
            {
                return $"{FormatPath(path)}: count mismatch expected={expectedList.Count} actual={actualList.Count}";
            }

            for (int i = 0; i < expectedList.Count; i++)
            {
                string diff = FindFirstDiff(expectedList[i], actualList[i], AppendIndex(path, i));
                if (diff != null)
                {
                    return diff;
                }
            }

            return null;
        }

        if (expected is IEnumerable expectedEnumerable && actual is IEnumerable actualEnumerable)
        {
            List<object> expectedItems = [.. expectedEnumerable.Cast<object>()];
            List<object> actualItems = [.. actualEnumerable.Cast<object>()];

            if (expectedItems.Count != actualItems.Count)
            {
                return $"{FormatPath(path)}: count mismatch expected={expectedItems.Count} actual={actualItems.Count}";
            }

            for (int i = 0; i < expectedItems.Count; i++)
            {
                string diff = FindFirstDiff(expectedItems[i], actualItems[i], AppendIndex(path, i));
                if (diff != null)
                {
                    return diff;
                }
            }

            return null;
        }

        PropertyInfo[] properties = [.. expectedType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .OrderBy(p => p.Name, StringComparer.Ordinal)];

        foreach (PropertyInfo property in properties)
        {
            object expectedValue = property.GetValue(expected);
            object actualValue = property.GetValue(actual);
            string diff = FindFirstDiff(expectedValue, actualValue, AppendPath(path, property.Name));
            if (diff != null)
            {
                return diff;
            }
        }

        return null;
    }

    private static bool IsSimple(Type type)
    {
        if (type.IsPrimitive || type.IsEnum)
        {
            return true;
        }

        return type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(Guid);
    }

    private static string AppendPath(string path, string name)
    {
        if (string.IsNullOrEmpty(path))
        {
            return name;
        }

        return path + "." + name;
    }

    private static string AppendIndex(string path, int index)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "[" + index.ToString(CultureInfo.InvariantCulture) + "]";
        }

        return path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]";
    }

    private static string FormatPath(string path)
    {
        return string.IsNullOrEmpty(path) ? "<root>" : path;
    }

    private static string FormatValue(object value)
    {
        if (value is null)
        {
            return "null";
        }

        if (value is string str)
        {
            return "\"" + str + "\"";
        }

        if (value is char ch)
        {
            return "'" + ch.ToString(CultureInfo.InvariantCulture) + "'";
        }

        if (value is DateTime dt)
        {
            return dt.ToString("O", CultureInfo.InvariantCulture);
        }

        if (value is IFormattable formattable)
        {
            return formattable.ToString(null, CultureInfo.InvariantCulture);
        }

        return value.ToString();
    }
}
