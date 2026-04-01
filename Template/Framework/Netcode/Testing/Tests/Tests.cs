using GdUnit4;
using static GdUnit4.Assertions;

namespace Template.Setup.Testing;

[TestSuite]
/// <summary>
/// Basic smoke tests for test project setup validation.
/// </summary>
public class Tests
{
    /// <summary>
    /// Verifies expected lowercase conversion behavior.
    /// </summary>
    [TestCase]
    public static void StringToLower()
    {
        TestOutput.Header(nameof(StringToLower));
        AssertString("AbcD".ToLower()).IsEqual("abcd");
    }
}
