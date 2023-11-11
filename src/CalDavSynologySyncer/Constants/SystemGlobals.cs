// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemGlobals.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains some system settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Constants;

/// <summary>
/// A class that contains some system settings.
/// </summary>
public static class SystemGlobals
{
    /// <summary>
    /// The gigabytes divider. (Used to convert from bytes to gigabytes).
    /// </summary>
    public const decimal GigaBytesDivider = 1024 * 1024 * 1024;

    /// <summary>
    /// The megabytes divider. (Used to convert from bytes to megabytes).
    /// </summary>
    public const decimal MegaBytesDivider = 1024 * 1024;

    /// <summary>
    /// The kilobytes divider. (Used to convert from bytes to kilobytes).
    /// </summary>
    public const decimal KiloBytesDivider = 1024;

    /// <summary>
    /// Gets the value with the unit byte size.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="allowedDecimals">The allowed decimals.</param>
    /// <returns>The value as <see cref="string"/>.</returns>
    public static string GetValueWithUnitByteSize(long value, int allowedDecimals = 2)
    {
        return GetValueWithUnitByteSize((decimal)value, allowedDecimals);
    }

    /// <summary>
    /// Gets the value with the unit byte size.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="allowedDecimals">The allowed decimals.</param>
    /// <returns>The value as <see cref="string"/>.</returns>
    private static string GetValueWithUnitByteSize(decimal value, int allowedDecimals = 2)
    {
        if (value > GigaBytesDivider)
        {
            return $"{Math.Round(value / GigaBytesDivider, allowedDecimals)} GB";
        }

        if (value > MegaBytesDivider)
        {
            return $"{Math.Round(value / MegaBytesDivider, allowedDecimals)} MB";
        }

        if (value > KiloBytesDivider)
        {
            return $"{Math.Round(value / KiloBytesDivider, allowedDecimals)} kB";
        }

        return $"{value} bytes";
    }
}
