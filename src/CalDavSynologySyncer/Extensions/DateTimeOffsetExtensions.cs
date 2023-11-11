// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeOffsetExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains extension methods for the DateTimeOffset type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Extensions;

/// <summary>
/// A class that contains extension methods for the DateTimeOffset type.
/// </summary>
public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Checks the <see cref="DateTimeOffset"/> for expiration.
    /// </summary>
    /// <param name="timestamp">The timestamp to check.</param>
    /// <param name="duration">The timespan to check.</param>
    /// <returns>True when expired otherwise false.</returns>
    public static bool IsExpired(this DateTimeOffset timestamp, TimeSpan duration)
    {
        return timestamp.Add(duration) < DateTimeOffset.Now;
    }
}
