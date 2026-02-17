using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace PacketGen.Tests;

internal static class DeepAssert
{
    public static void AreEqual(object? expected, object? actual, string path)
    {
        if (ReferenceEquals(expected, actual))
        {
            return;
        }

        if (expected is null || actual is null)
        {
            Assert.That(actual, Is.EqualTo(expected), $"{path} expected <{Format(expected)}> but was <{Format(actual)}>.");
            return;
        }

        if (expected is string || expected.GetType().IsPrimitive || expected is decimal || expected is Enum)
        {
            Assert.That(actual, Is.EqualTo(expected), $"{path} expected <{Format(expected)}> but was <{Format(actual)}>.");
            return;
        }

        if (expected is Array expectedArray && actual is Array actualArray)
        {
            Assert.That(actualArray, Has.Length.EqualTo(expectedArray.Length), $"{path} length mismatch. Expected {expectedArray.Length}, got {actualArray.Length}.");

            for (int i = 0; i < expectedArray.Length; i++)
            {
                object? expectedValue = expectedArray.GetValue(i);
                object? actualValue = actualArray.GetValue(i);
                AreEqual(expectedValue, actualValue, $"{path}[{i}]");
            }

            return;
        }

        if (expected is IDictionary expectedDictionary && actual is IDictionary actualDictionary)
        {
            Assert.That(actualDictionary, Has.Count.EqualTo(expectedDictionary.Count), $"{path} count mismatch. Expected {expectedDictionary.Count}, got {actualDictionary.Count}.");

            foreach (DictionaryEntry entry in expectedDictionary)
            {
                if (!actualDictionary.Contains(entry.Key))
                {
                    Assert.Fail($"{path} missing key <{Format(entry.Key)}>.");
                }

                AreEqual(entry.Value, actualDictionary[entry.Key], $"{path}[{Format(entry.Key)}]");
            }

            foreach (DictionaryEntry entry in actualDictionary)
            {
                if (!expectedDictionary.Contains(entry.Key))
                {
                    Assert.Fail($"{path} had unexpected key <{Format(entry.Key)}>.");
                }
            }

            return;
        }

        if (expected is IList expectedList && actual is IList actualList)
        {
            Assert.That(actualList, Has.Count.EqualTo(expectedList.Count), $"{path} count mismatch. Expected {expectedList.Count}, got {actualList.Count}.");

            for (int i = 0; i < expectedList.Count; i++)
            {
                AreEqual(expectedList[i], actualList[i], $"{path}[{i}]");
            }

            return;
        }

        Type expectedType = expected.GetType();
        Type actualType = actual.GetType();
        Assert.That(actualType, Is.EqualTo(expectedType), $"{path} type mismatch. Expected <{expectedType.FullName}>, got <{actualType.FullName}>.");

        PropertyInfo[] comparableProperties = [.. expectedType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(static property => property.CanRead && property.GetIndexParameters().Length == 0)];

        if (comparableProperties.Length > 0)
        {
            foreach (PropertyInfo property in comparableProperties)
            {
                object? expectedValue = property.GetValue(expected);
                object? actualValue = property.GetValue(actual);
                AreEqual(expectedValue, actualValue, $"{path}.{property.Name}");
            }

            return;
        }

        Assert.That(actual, Is.EqualTo(expected), $"{path} expected <{Format(expected)}> but was <{Format(actual)}>.");
    }

    private static string Format(object? value)
    {
        return value is null ? "null" : value.ToString() ?? "(null)";
    }
}
