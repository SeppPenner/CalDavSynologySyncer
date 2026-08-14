// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeOffsetExtensionsTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="DateTimeOffsetExtensions" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Tests;

/// <summary>
/// A class to test the <see cref="DateTimeOffsetExtensions"/> class. The service uses the extension to decide
/// whether the heartbeat is due.
/// </summary>
[TestClass]
public class DateTimeOffsetExtensionsTests
{
    /// <summary>
    /// Checks whether a timestamp that is older than the duration is expired.
    /// </summary>
    [TestMethod]
    public void ATimestampOlderThanTheDurationIsExpired()
    {
        var timestamp = DateTimeOffset.Now.AddMinutes(-2);

        Assert.IsTrue(timestamp.IsExpired(TimeSpan.FromMinutes(1)));
    }

    /// <summary>
    /// Checks whether a timestamp inside the duration is not expired.
    /// </summary>
    [TestMethod]
    public void ATimestampInsideTheDurationIsNotExpired()
    {
        var timestamp = DateTimeOffset.Now.AddSeconds(-1);

        Assert.IsFalse(timestamp.IsExpired(TimeSpan.FromMinutes(1)));
    }

    /// <summary>
    /// Checks whether a timestamp in the future is not expired.
    /// </summary>
    [TestMethod]
    public void ATimestampInTheFutureIsNotExpired()
    {
        var timestamp = DateTimeOffset.Now.AddMinutes(5);

        Assert.IsFalse(timestamp.IsExpired(TimeSpan.FromMinutes(1)));
    }

    /// <summary>
    /// Checks whether the default timestamp is expired. That is what makes the service log the heartbeat in its
    /// very first cycle, the backing property starts out unset.
    /// </summary>
    [TestMethod]
    public void TheDefaultTimestampIsExpired()
    {
        var timestamp = default(DateTimeOffset);

        Assert.IsTrue(timestamp.IsExpired(TimeSpan.FromDays(365)));
    }

    /// <summary>
    /// Checks whether a duration of zero makes every past timestamp expired.
    /// </summary>
    [TestMethod]
    public void ADurationOfZeroExpiresEveryPastTimestamp()
    {
        var timestamp = DateTimeOffset.Now.AddMilliseconds(-1);

        Assert.IsTrue(timestamp.IsExpired(TimeSpan.Zero));
    }
}
