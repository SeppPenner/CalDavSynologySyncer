// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemGlobalsTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="SystemGlobals" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Tests;

/// <summary>
/// A class to test the <see cref="SystemGlobals"/> class. The formatted values end up in the heartbeat log line.
/// </summary>
[TestClass]
public class SystemGlobalsTests
{
    /// <summary>
    /// Checks whether a value below a kilobyte keeps the unit bytes.
    /// </summary>
    [TestMethod]
    public void ASmallValueIsFormattedInBytes()
    {
        Assert.AreEqual("0 bytes", SystemGlobals.GetValueWithUnitByteSize(0));
        Assert.AreEqual("512 bytes", SystemGlobals.GetValueWithUnitByteSize(512));
    }

    /// <summary>
    /// Checks whether the dividers are compared with greater than and not with greater than or equal, so that
    /// exactly one kilobyte, one megabyte and one gigabyte are still reported with the smaller unit.
    /// </summary>
    [TestMethod]
    public void AValueExactlyOnTheDividerKeepsTheSmallerUnit()
    {
        Assert.AreEqual("1024 bytes", SystemGlobals.GetValueWithUnitByteSize(1024));
        Assert.AreEqual("1024 kB", SystemGlobals.GetValueWithUnitByteSize(1024 * 1024));
        Assert.AreEqual("1024 MB", SystemGlobals.GetValueWithUnitByteSize(1024L * 1024 * 1024));
    }

    /// <summary>
    /// Checks whether the unit follows the size of the value.
    /// </summary>
    [TestMethod]
    public void TheUnitFollowsTheSizeOfTheValue()
    {
        Assert.AreEqual("2 kB", SystemGlobals.GetValueWithUnitByteSize(2 * 1024));
        Assert.AreEqual("5 MB", SystemGlobals.GetValueWithUnitByteSize(5 * 1024 * 1024));
        Assert.AreEqual("3 GB", SystemGlobals.GetValueWithUnitByteSize(3L * 1024 * 1024 * 1024));
    }

    /// <summary>
    /// Checks whether the value is rounded to two decimals by default and to the given number of decimals
    /// otherwise.
    /// </summary>
    [TestMethod]
    public void TheValueIsRoundedToTheAllowedDecimals()
    {
        var previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.AreEqual("1.5 kB", SystemGlobals.GetValueWithUnitByteSize(1536));
            Assert.AreEqual("1.33 kB", SystemGlobals.GetValueWithUnitByteSize(1360));
            Assert.AreEqual("1.3 kB", SystemGlobals.GetValueWithUnitByteSize(1360, 1));
            Assert.AreEqual("1 kB", SystemGlobals.GetValueWithUnitByteSize(1360, 0));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// Checks whether the decimal separator follows the current culture. The method interpolates the value
    /// without a format provider, so the heartbeat line of a German machine carries a comma.
    /// </summary>
    [TestMethod]
    public void TheDecimalSeparatorFollowsTheCurrentCulture()
    {
        var previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Assert.AreEqual("1,5 kB", SystemGlobals.GetValueWithUnitByteSize(1536));

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.AreEqual("1.5 kB", SystemGlobals.GetValueWithUnitByteSize(1536));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// Checks whether a negative value falls through to the unit bytes. None of the three dividers matches a
    /// negative value, and the garbage collector does not report one, this pins the behaviour anyway.
    /// </summary>
    [TestMethod]
    public void ANegativeValueIsFormattedInBytes()
    {
        Assert.AreEqual("-2048 bytes", SystemGlobals.GetValueWithUnitByteSize(-2048));
    }
}
