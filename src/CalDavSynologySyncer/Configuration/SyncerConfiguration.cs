// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncerConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class containing the CalDav Synology syncer service configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Configuration;

/// <summary>
/// A class containing the CalDav Synology syncer service configuration.
/// </summary>
public sealed class SyncerConfiguration
{
    /// <summary>
    /// Gets or sets the calendar urls to check.
    /// </summary>
    public List<string> CalendarUrls { get; set; } = new();

    /// <summary>
    /// Gets or sets the Synology calendar URL.
    /// </summary>
    public string SynologyCalendarUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Synology calendar identifier.
    /// </summary>
    public string SynologyCalendarId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service delay in milliseconds.
    /// </summary>
    public int ServiceDelayInMilliSeconds { get; set; } = 3000;

    /// <summary>
    /// Gets or sets the heartbeat interval in milliseconds.
    /// </summary>
    public int HeartbeatIntervalInMilliSeconds { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the Telegram bot token.
    /// </summary>
    public string TelegramBotToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Telegram chat identifier.
    /// </summary>
    public string TelegramChatId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Synology user name.
    /// </summary>
    public string SynologyUserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Synology password.
    /// </summary>
    public string SynologyPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the configuration is valid or not.
    /// </summary>
    /// <returns>A <see cref="bool"/> value indicating whether the configuration is valid or not.</returns>
    public bool IsValid()
    {
        if (this.CalendarUrls.IsEmptyOrNull())
        {
            throw new ConfigurationException("The calendar urls are empty.");
        }

        if (string.IsNullOrWhiteSpace(this.SynologyCalendarUrl))
        {
            throw new ConfigurationException("The Synology calendar url is not set.");
        }

        if (string.IsNullOrWhiteSpace(this.SynologyCalendarId))
        {
            throw new ConfigurationException("The Synology calendar identifier is not set.");
        }

        if (this.ServiceDelayInMilliSeconds <= 0)
        {
            throw new ConfigurationException("The service delay is invalid.");
        }

        if (this.HeartbeatIntervalInMilliSeconds <= 0)
        {
            throw new ConfigurationException("The heartbeat interval is invalid.");
        }

        if (string.IsNullOrWhiteSpace(this.SynologyUserName))
        {
            throw new ConfigurationException("The Synology user name is not set.");
        }

        if (string.IsNullOrWhiteSpace(this.SynologyPassword))
        {
            throw new ConfigurationException("The Synology password is not set.");
        }

        return true;
    }
}
