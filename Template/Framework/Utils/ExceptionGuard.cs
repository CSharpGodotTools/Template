using System;

namespace __TEMPLATE__;

internal static class ExceptionGuard
{
    public static bool IsNonFatal(Exception exception)
    {
        return exception is not OutOfMemoryException
            and not StackOverflowException
            and not AccessViolationException
            and not AppDomainUnloadedException
            and not BadImageFormatException
            and not CannotUnloadAppDomainException;
    }
}
