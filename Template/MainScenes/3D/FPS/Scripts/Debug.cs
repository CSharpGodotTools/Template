using System.Diagnostics;

namespace __TEMPLATE__.FPS;

public class Debug
{
    [StackTraceHidden]
    public static void MyTest()
    {
        throw new System.Exception("Test");
    }
}

