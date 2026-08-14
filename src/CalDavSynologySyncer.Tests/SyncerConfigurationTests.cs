// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncerConfigurationTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="SyncerConfiguration" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Tests;

/// <summary>
/// A class to test the <see cref="SyncerConfiguration"/> class. Every check throws instead of returning false,
/// which is what makes a bad configuration end the process before the first cycle.
/// </summary>
[TestClass]
public class SyncerConfigurationTests
{
    /// <summary>
    /// Checks whether a complete configuration is valid.
    /// </summary>
    [TestMethod]
    public void ACompleteConfigurationIsValid()
    {
        Assert.IsTrue(TestDataProvider.GetValidConfiguration().IsValid());
    }

    /// <summary>
    /// Checks whether the default configuration is rejected. A fresh instance carries an empty calendar url
    /// list, that is the first check that fails.
    /// </summary>
    [TestMethod]
    public void TheDefaultConfigurationIsRejected()
    {
        var configuration = new SyncerConfiguration();

        var exception = Assert.ThrowsExactly<ConfigurationException>(() => configuration.IsValid());
        Assert.AreEqual("The calendar urls are empty.", exception.Message);
    }

    /// <summary>
    /// Checks whether an empty calendar url list is rejected.
    /// </summary>
    [TestMethod]
    public void AnEmptyCalendarUrlListIsRejected()
    {
        var configuration = TestDataProvider.GetValidConfiguration();
        configuration.CalendarUrls = [];

        var exception = Assert.ThrowsExactly<ConfigurationException>(() => configuration.IsValid());
        Assert.AreEqual("The calendar urls are empty.", exception.Message);
    }

    /// <summary>
    /// Checks whether a missing Synology calendar url is rejected.
    /// </summary>
    [TestMethod]
    public void AMissingSynologyCalendarUrlIsRejected()
    {
        var configuration = TestDataProvider.GetValidConfiguration();
        configuration.SynologyCalendarUrl = "   ";

        var exception = Assert.ThrowsExactly<ConfigurationException>(() => configuration.IsValid());
        Assert.AreEqual("The Synology calendar url is not set.", exception.Message);
    }

    /// <summary>
    /// Checks whether a missing Synology calendar identifier is rejected.
    /// </summary>
    [TestMethod]
    public void AMissingSynologyCalendarIdentifierIsRejected()
    {
        var configuration = TestDataProvider.GetValidConfiguration();
        configuration.SynologyCalendarId = string.Empty;

        var exception = Assert.ThrowsExactly<ConfigurationException>(() => configuration.IsValid());
        Assert.AreEqual("The Synology calendar identifier is not set.", exception.Message);
    }

    /// <summary>
    /// Checks whether a service delay of zero or less is rejected.
    /// </summary>
    [TestMethod]
    public void AnInvalidServiceDelayIsRejected()
    {
        var configuration = TestDataProvider.GetValidConfiguration();
        configuration.ServiceDelayInMilliSeconds = 0;

        var exception = Assert.ThrowsExactly<ConfigurationException>(() => configuration.IsValid());
        Assert.AreEqual("The service delay is invalid.", exception.Message);

        configuration.ServiceDelayInMilliSeconds = -1;
        Assert.ThrowsExactly<ConfigurationException>(() => configuration.IsValid());
    }

    /// <summary>
    /// Checks whether a heartbeat interval of zero or less is rejected.
    /// </summary>
    [TestMethod]
    public void AnInvalidHeartbeatIntervalIsRejected()
    {
        var configuration = TestDataProvider.GetValidConfiguration();
        configuration.HeartbeatIntervalInMilliSeconds = 0;

        var exception = Assert.ThrowsExactly<ConfigurationException>(() => configuration.IsValid());
        Assert.AreEqual("The heartbeat interval is invalid.", exception.Message);
    }

    /// <summary>
    /// Checks whether missing Synology credentials are rejected.
    /// </summary>
    [TestMethod]
    public void MissingSynologyCredentialsAreRejected()
    {
        var withoutUserName = TestDataProvider.GetValidConfiguration();
        withoutUserName.SynologyUserName = string.Empty;

        var userNameException = Assert.ThrowsExactly<ConfigurationException>(() => withoutUserName.IsValid());
        Assert.AreEqual("The Synology user name is not set.", userNameException.Message);

        var withoutPassword = TestDataProvider.GetValidConfiguration();
        withoutPassword.SynologyPassword = string.Empty;

        var passwordException = Assert.ThrowsExactly<ConfigurationException>(() => withoutPassword.IsValid());
        Assert.AreEqual("The Synology password is not set.", passwordException.Message);
    }

    /// <summary>
    /// Checks whether the two optional settings stay optional. The Telegram credentials are only needed for the
    /// Telegram sink, and both values of <see cref="SyncerConfiguration.RemoveEntriesWithStar"/> are allowed.
    /// </summary>
    [TestMethod]
    public void TheOptionalSettingsAreNotChecked()
    {
        var configuration = TestDataProvider.GetValidConfiguration();
        configuration.TelegramBotToken = string.Empty;
        configuration.TelegramChatId = string.Empty;
        configuration.RemoveEntriesWithStar = false;

        Assert.IsTrue(configuration.IsValid());
    }
}
