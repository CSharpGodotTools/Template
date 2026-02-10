using GdUnit4;
using static GdUnit4.Assertions;

namespace Template.Setup.Testing;

[TestSuite]
public class Tests
{
    [TestCase]
    public static void StringToLower()
    {
        TestOutput.Header(nameof(StringToLower));
        AssertString("AbcD".ToLower()).IsEqual("abcd");
    }
}
