// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationException.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An exception that is thrown when the configuration is invalid.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Exceptions;

/// <seealso cref="Exception"/>
/// <inheritdoc cref="Exception"/>
/// <summary>
/// An exception that is thrown when the configuration is invalid.
/// </summary>
public class ConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Exception"/> class.
    /// </summary>
    public ConfigurationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Exception"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Exception"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">
    ///     The exception that is the cause of the current exception, or a null reference
    ///     (Nothing in Visual Basic) if no inner exception is specified.
    /// </param>
    public ConfigurationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
