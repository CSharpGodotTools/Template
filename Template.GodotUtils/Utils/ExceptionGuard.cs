using System;

namespace GodotUtils;

/// <summary>
/// Classifies exceptions as fatal or non-fatal for guarded background execution.
/// </summary>
internal static class ExceptionGuard
{
    /// <summary>
    /// Returns whether an exception is considered non-fatal for the process.
    /// </summary>
    /// <param name="exception">Exception to classify.</param>
    /// <returns><see langword="true"/> when exception is non-fatal.</returns>
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
