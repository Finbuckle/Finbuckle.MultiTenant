// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant;

/// <summary>
/// Represents an exception that is thrown when an error occurs in the Finbuckle.MultiTenant library.
/// </summary>
public class MultiTenantException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTenantException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MultiTenantException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTenantException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public MultiTenantException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}