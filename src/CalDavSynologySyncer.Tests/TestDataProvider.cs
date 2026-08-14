// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataProvider.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to provide the test data used in the tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Tests;

/// <summary>
/// A class to provide the test data used in the tests. The values are the ones of the shipped
/// <c>appsettings.json</c>, so a test failure can be compared with a real configuration.
/// </summary>
internal static class TestDataProvider
{
    /// <summary>
    /// The source calendar url used in the tests.
    /// </summary>
    public const string CalendarUrl = "https://ical.de/ical1123";

    /// <summary>
    /// The Synology calendar url used in the tests.
    /// </summary>
    public const string SynologyCalendarUrl = "http://192.168.2.2/caldav.php/user/someid";

    /// <summary>
    /// The Synology calendar identifier used in the tests.
    /// </summary>
    public const string SynologyCalendarId = "/caldav.php/user/uniqueid/";

    /// <summary>
    /// The Synology user name used in the tests.
    /// </summary>
    public const string SynologyUserName = "test";

    /// <summary>
    /// The Synology password used in the tests.
    /// </summary>
    public const string SynologyPassword = "password";

    /// <summary>
    /// Gets a configuration that passes every check of <see cref="SyncerConfiguration.IsValid"/>.
    /// </summary>
    /// <returns>A valid <see cref="SyncerConfiguration"/>.</returns>
    public static SyncerConfiguration GetValidConfiguration()
    {
        return new SyncerConfiguration
        {
            CalendarUrls = [CalendarUrl],
            SynologyCalendarUrl = SynologyCalendarUrl,
            SynologyCalendarId = SynologyCalendarId,
            SynologyUserName = SynologyUserName,
            SynologyPassword = SynologyPassword,
            ServiceDelayInMilliSeconds = 30000,
            HeartbeatIntervalInMilliSeconds = 30000,
            RemoveEntriesWithStar = true
        };
    }

    /// <summary>
    /// Gets a logger that writes into the given sink and nowhere else.
    /// </summary>
    /// <param name="sink">The sink that collects the log events.</param>
    /// <returns>An <see cref="ILogger"/>.</returns>
    public static ILogger GetLogger(CollectingSink sink)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Sink(sink)
            .CreateLogger();
    }

    /// <summary>
    /// Creates an own directory below the temp path for one test.
    /// </summary>
    /// <returns>The full path of the created directory as <see cref="string"/>.</returns>
    public static string CreateTestDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"CalDavSynologySyncerTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
