using System;

namespace __TEMPLATE__;

/// <summary>
/// Classifies exceptions that are safe to catch and continue from.
/// </summary>
internal static class ExceptionGuard
{
    /// <summary>
    /// Determines whether an exception is considered non-fatal for recovery paths.
    /// </summary>
    /// <param name="exception">Exception instance to classify.</param>
    /// <returns><see langword="true"/> when the exception can be handled safely.</returns>
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
