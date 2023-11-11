// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerConfig.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the logger configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Constants;

/// <summary>
/// A class that contains the logger configuration.
/// </summary>
public static class LoggerConfig
{
    /// <summary>
    /// Gets the logger configuration for the given type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A <see cref="LoggerConfiguration"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the type is not set.</exception>
    public static LoggerConfiguration GetLoggerConfiguration(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException(nameof(type), "The type of logger must be given");
        }

        // Setup the logging for data frame output.
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithProperty(LoggingKeys.LoggerType, type);
    }
}
