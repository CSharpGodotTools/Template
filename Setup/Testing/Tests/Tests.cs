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
        try
        {
            AssertString("AbcD".ToLower()).IsEqual("abcd");
        }
        finally
        {
            TestOutput.Footer();
        }
    }
}
