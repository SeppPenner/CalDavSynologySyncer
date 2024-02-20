// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalDavSynologySyncerService.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The CalDAV Synology syncer service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer;

/// <seealso cref="BackgroundService"/>
/// <inheritdoc cref="BackgroundService"/>
/// <summary>
/// The CalDAV Synology syncer service.
/// </summary>
internal sealed class CalDavSynologySyncerService : BackgroundService
{
    /// <summary>
    /// The stopwatch for the application lifetime.
    /// </summary>
    private readonly Stopwatch uptimeStopWatch = Stopwatch.StartNew();

    /// <summary>
    /// The dummy calendar needed for serialization.
    /// </summary>
    private readonly Calendar dummyCalendar = new();

    /// <summary>
    /// Gets or sets the last heartbeat timestamp.
    /// </summary>
    private DateTimeOffset LastHeartbeatAt { get; set; }

    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    private ILogger Logger { get; set; } = Log.Logger;

    /// <summary>
    /// Gets or sets the service configuration.
    /// </summary>
    private SyncerConfiguration ServiceConfiguration { get; set; }

    /// <summary>
    /// The CalDAV client.
    /// </summary>
    private readonly Client calDavClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalDavSynologySyncerService"/> class.
    /// </summary>
    /// <param name="configuration">The CalDAV Synology syncer configuration.</param>
    public CalDavSynologySyncerService(SyncerConfiguration configuration)
    {
        // Load the configuration.
        this.ServiceConfiguration = configuration;

        // Create the logger.
        this.Logger = LoggerConfig.GetLoggerConfiguration(nameof(CalDavSynologySyncerService))
            .WriteTo.Sink((ILogEventSink)Log.Logger)
            .CreateLogger();

        // Create the CalDAV client.
        this.calDavClient = new Client(configuration.SynologyCalendarUrl, configuration.SynologyUserName, configuration.SynologyPassword);
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Starting CalDAV Synology syncer service");
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Stopping CalDAV Synology syncer service");
        await base.StopAsync(cancellationToken);
    }

    /// <inheritdoc cref="BackgroundService"/>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Executing CalDAV Synology syncer service");

        while (!cancellationToken.IsCancellationRequested)
        {
            // Runs the main task of the service.
            await this.TryRunServiceTask();

            // Run the heartbeat and log some memory information.
            this.LogMemoryInformation(this.ServiceConfiguration.HeartbeatIntervalInMilliSeconds, Program.ServiceName);
            await Task.Delay(this.ServiceConfiguration.ServiceDelayInMilliSeconds, cancellationToken);
        }
    }

    /// <summary>
    /// Runs the main task of the service.
    /// </summary>
    private async Task TryRunServiceTask()
    {
        try
        {
            // Start a new stop watch.
            var stopwatch = Stopwatch.StartNew();
            this.Logger.Information("Started cyclic CalDAV Synology syncer task");

            // Delete all calendar files (Possibly olds that haven't been deleted yet.
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var directory = new DirectoryInfo(currentLocation!);

            // Delete all (maybe existing) *.ics files in the folder.
            foreach (var file in directory.GetFiles("*.ics"))
            {
                this.Logger.Information("Deleting file {FileName}", file.Name);
                FileHelper.TryDelete(file.Name, this.Logger);
            }

            // Iterate all possible calendars.
            foreach (var calendarUrl in this.ServiceConfiguration.CalendarUrls)
            {
                // Load calendar data from server.
                this.Logger.Information("Loading data from reference ICAL calendar {CalendarUrl}", calendarUrl);
                var filePath = await this.LoadCalendarFileFromServer(calendarUrl);

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    this.Logger.Error("The calendar file path is empty");
                    return;
                }

                // Read ICAL data.
                var icalData = await File.ReadAllTextAsync(filePath);
                var calendars = CalendarCollection.Load(icalData);

                // Load event data from the Synology calendar.
                var synologyCalendar = await this.calDavClient.GetCalendarByUid(this.ServiceConfiguration.SynologyCalendarId);

                if (synologyCalendar is null)
                {
                    this.Logger.Error("Synology calendar not found");
                    return;
                }

                // Iterate all calendars if more than one.
                foreach (var calendar in calendars)
                {
                    foreach (var component in calendar.Children)
                    {
                        // Skip non calendar events.
                        if (component is not CalendarEvent icalEvent)
                        {
                            continue;
                        }

                        // If the summary can be found, update the event.
                        var synologyEntries = synologyCalendar.Events.Where(e => e.Uid == icalEvent.Uid).ToList();

                        if (synologyEntries.IsEmptyOrNull())
                        {
                            // Add a new event to the server.
                            this.Logger.Information("Adding event {Summary}, {Guid}, {Start}, {End} to calendar",
                                icalEvent.Summary,
                                icalEvent.Uid,
                                icalEvent.DtStart?.AsDateTimeOffset,
                                icalEvent.DtEnd?.AsDateTimeOffset);
                            var saved = await this.calDavClient.AddOrUpdateEvent(icalEvent, this.dummyCalendar);

                            if (!saved)
                            {
                                this.Logger.Error("Unable to save event {Event} to calendar", icalEvent.ToJsonString());
                                continue;
                            }
                        }
                        else if (synologyEntries.Count > 1)
                        {
                            // If two entries were found, log an error.
                            this.Logger.Error("{Count} Synology entries with the same Guid found: {@Ids}",
                                synologyEntries.Count,
                                synologyEntries.Select(e => e.Uid).ToList());
                        }
                        else
                        {
                            // Get first (and only) entry.
                            var synologyEntry = synologyEntries.First();

                            // Check whether updates are needed.
                            var needsUpdate = false;

                            if (!icalEvent.Attachments.SequenceEqual(synologyEntry.Attachments))
                            {
                                synologyEntry.Attachments = icalEvent.Attachments;
                                needsUpdate = true;
                            }

                            if (!icalEvent.Attendees.SequenceEqual(synologyEntry.Attendees))
                            {
                                synologyEntry.Attendees = icalEvent.Attendees;
                                needsUpdate = true;
                            }

                            if (!icalEvent.Categories.SequenceEqual(synologyEntry.Categories))
                            {
                                synologyEntry.Categories = icalEvent.Categories;
                                needsUpdate = true;
                            }

                            if (icalEvent.Class != synologyEntry.Class)
                            {
                                synologyEntry.Class = icalEvent.Class;
                                needsUpdate = true;
                            }

                            if (icalEvent.Column != synologyEntry.Column)
                            {
                                synologyEntry.Column = icalEvent.Column;
                                needsUpdate = true;
                            }

                            if (!icalEvent.Comments.SequenceEqual(synologyEntry.Comments))
                            {
                                synologyEntry.Comments = icalEvent.Comments;
                                needsUpdate = true;
                            }

                            if (!icalEvent.Contacts.SequenceEqual(synologyEntry.Contacts))
                            {
                                synologyEntry.Contacts = icalEvent.Contacts;
                                needsUpdate = true;
                            }

                            if (icalEvent.Created?.AsDateTimeOffset != synologyEntry.Created?.AsDateTimeOffset)
                            {
                                synologyEntry.Created = icalEvent.Created;
                                needsUpdate = true;
                            }

                            if (icalEvent.Description != synologyEntry.Description)
                            {
                                synologyEntry.Description = icalEvent.Description;
                                needsUpdate = true;
                            }

                            if (icalEvent.DtEnd?.AsDateTimeOffset != synologyEntry.DtEnd?.AsDateTimeOffset)
                            {
                                synologyEntry.DtEnd = icalEvent.DtEnd;
                                needsUpdate = true;
                            }

                            if (icalEvent.DtStamp?.AsDateTimeOffset != synologyEntry.DtStamp?.AsDateTimeOffset)
                            {
                                synologyEntry.DtStamp = icalEvent.DtStamp;
                                needsUpdate = true;
                            }

                            if (icalEvent.DtStart?.AsDateTimeOffset != synologyEntry.DtStart?.AsDateTimeOffset)
                            {
                                synologyEntry.DtStart = icalEvent.DtStart;
                                needsUpdate = true;
                            }

                            if (icalEvent.Duration != synologyEntry.Duration)
                            {
                                synologyEntry.Duration = icalEvent.Duration;
                                needsUpdate = true;
                            }

                            if (!icalEvent.ExceptionDates.SequenceEqual(synologyEntry.ExceptionDates))
                            {
                                synologyEntry.ExceptionDates = icalEvent.ExceptionDates;
                                needsUpdate = true;
                            }

                            if (!icalEvent.ExceptionRules.SequenceEqual(synologyEntry.ExceptionRules))
                            {
                                synologyEntry.ExceptionRules = icalEvent.ExceptionRules;
                                needsUpdate = true;
                            }

                            if (icalEvent.GeographicLocation != synologyEntry.GeographicLocation)
                            {
                                synologyEntry.GeographicLocation = icalEvent.GeographicLocation;
                                needsUpdate = true;
                            }

                            if (icalEvent.Group != synologyEntry.Group)
                            {
                                synologyEntry.Group = icalEvent.Group;
                                needsUpdate = true;
                            }

                            if (icalEvent.IsAllDay != synologyEntry.IsAllDay)
                            {
                                synologyEntry.IsAllDay = icalEvent.IsAllDay;
                                needsUpdate = true;
                            }

                            if (icalEvent.LastModified?.AsDateTimeOffset != synologyEntry.LastModified?.AsDateTimeOffset)
                            {
                                synologyEntry.LastModified = icalEvent.LastModified;
                                needsUpdate = true;
                            }

                            if (icalEvent.Line != synologyEntry.Line)
                            {
                                synologyEntry.Line = icalEvent.Line;
                                needsUpdate = true;
                            }

                            if (icalEvent.Location != synologyEntry.Location)
                            {
                                synologyEntry.Location = icalEvent.Location;
                                needsUpdate = true;
                            }

                            if (icalEvent.Name != synologyEntry.Name)
                            {
                                synologyEntry.Name = icalEvent.Name;
                                needsUpdate = true;
                            }

                            if (icalEvent.Organizer != synologyEntry.Organizer)
                            {
                                synologyEntry.Organizer = icalEvent.Organizer;
                                needsUpdate = true;
                            }

                            // Don't check parent value.
                            //if (icalEvent.Parent != synologyEntry.Parent)
                            //{
                            //    synologyEntry.Parent = icalEvent.Parent;
                            //    needsUpdate = true;
                            //}

                            if (icalEvent.Priority != synologyEntry.Priority)
                            {
                                synologyEntry.Priority = icalEvent.Priority;
                                needsUpdate = true;
                            }

                            if (!icalEvent.RecurrenceDates.SequenceEqual(synologyEntry.RecurrenceDates))
                            {
                                synologyEntry.RecurrenceDates = icalEvent.RecurrenceDates;
                                needsUpdate = true;
                            }

                            if (icalEvent.RecurrenceId?.AsDateTimeOffset != synologyEntry.RecurrenceId?.AsDateTimeOffset)
                            {
                                synologyEntry.RecurrenceId = icalEvent.RecurrenceId;
                                needsUpdate = true;
                            }

                            if (!icalEvent.RecurrenceRules.SequenceEqual(synologyEntry.RecurrenceRules))
                            {
                                synologyEntry.RecurrenceRules = icalEvent.RecurrenceRules;
                                needsUpdate = true;
                            }

                            if (!icalEvent.RelatedComponents.SequenceEqual(synologyEntry.RelatedComponents))
                            {
                                synologyEntry.RelatedComponents = icalEvent.RelatedComponents;
                                needsUpdate = true;
                            }

                            if (!icalEvent.RequestStatuses.SequenceEqual(synologyEntry.RequestStatuses))
                            {
                                synologyEntry.RequestStatuses = icalEvent.RequestStatuses;
                                needsUpdate = true;
                            }

                            if (!icalEvent.Resources.SequenceEqual(synologyEntry.Resources))
                            {
                                synologyEntry.Resources = icalEvent.Resources;
                                needsUpdate = true;
                            }

                            if (icalEvent.Sequence != synologyEntry.Sequence)
                            {
                                synologyEntry.Sequence = icalEvent.Sequence;
                                needsUpdate = true;
                            }

                            if (icalEvent.Status != synologyEntry.Status)
                            {
                                synologyEntry.Status = icalEvent.Status;
                                needsUpdate = true;
                            }

                            if (icalEvent.Summary != synologyEntry.Summary)
                            {
                                synologyEntry.Summary = icalEvent.Summary;
                                needsUpdate = true;
                            }

                            if (icalEvent.Transparency != synologyEntry.Transparency)
                            {
                                synologyEntry.Transparency = icalEvent.Transparency;
                                needsUpdate = true;
                            }

                            if (icalEvent.Url != synologyEntry.Url)
                            {
                                synologyEntry.Url = icalEvent.Url;
                                needsUpdate = true;
                            }

                            if (!needsUpdate)
                            {
                                this.Logger.Information("No update needed for event {Summary}, {Guid}, {Start}, {End}",
                                    icalEvent.Summary,
                                    icalEvent.Uid,
                                    icalEvent.DtStart?.AsDateTimeOffset,
                                    icalEvent.DtEnd?.AsDateTimeOffset);
                            }
                            else
                            {
                                // Update event to the server.
                                this.Logger.Information("Updating event {Summary}, {Guid}, {Start}, {End}",
                                    icalEvent.Summary,
                                    icalEvent.Uid,
                                    icalEvent.DtStart?.AsDateTimeOffset,
                                    icalEvent.DtEnd?.AsDateTimeOffset);
                                var saved = await this.calDavClient.AddOrUpdateEvent(icalEvent, this.dummyCalendar);

                                if (!saved)
                                {
                                    this.Logger.Error("Unable to save event {Event} to calendar", icalEvent.ToJsonString());
                                    continue;
                                }
                            }
                        }
                    }
                }

                // Remove all entries that start with a '*' from the Synology calendar and have a corresponding entry
                // without a star in the Synology calendar within a certain tie range.
                if (this.ServiceConfiguration.RemoveEntriesWithStar)
                {
                    var synologyEntriesWithStar = synologyCalendar.Events
                        .Where(c => c is CalendarEvent && c is not null)
                        .Where(c => c.Summary.StartsWith("*"))
                        .ToList();

                    foreach (var synologyEntryWithStar in synologyEntriesWithStar)
                    {
                        var correspondingCalendarEntries = synologyCalendar.Events
                            .Where(c => c is CalendarEvent && c is not null)
                            .Where(c => c.Summary == synologyEntryWithStar.Summary.Substring(1));

                        // If there is no corresponding entry, skip the deletion.
                        if (correspondingCalendarEntries.Count() == 0)
                        {
                            continue;
                        }

                        // If there is more than 1 corresponding entry, log a warning and skip the deletion.
                        if (correspondingCalendarEntries.Count() > 1)
                        {
                            this.Logger.Warning("Found {Count} corresponding entries for entry with star {Summary}",
                                correspondingCalendarEntries.Count(),
                                synologyEntryWithStar.Summary);
                            continue;
                        }

                        // Get the first and only entry.
                        var correspondingCalendarEntry = correspondingCalendarEntries.First();
                        var timeDifference = correspondingCalendarEntry.DtStart.AsDateTimeOffset - synologyEntryWithStar.DtStart.AsDateTimeOffset;
                        var timeDifferenceAbsolute = Math.Abs(timeDifference.TotalDays);

                        // If the time difference is less than 4 days (absolute), delete the entry.
                        if (timeDifferenceAbsolute < 4)
                        {
                            var deletedEvent = await this.calDavClient.DeleteEvent(correspondingCalendarEntry);

                            if (!deletedEvent)
                            {
                                this.Logger.Error("Couldn't delete entry with star {Summary}", synologyEntryWithStar.Summary);
                                continue;
                            }
                        }
                    }
                }

                // Delete downloaded calendar file.
                FileHelper.TryDelete(filePath, this.Logger);
            }

            // All tasks are finished, cyclic CalDAV Synology syncer task is done.
            this.Logger.Information("Finished cyclic CalDAV Synology syncer task after: {Duration}", stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "Service task failed");
        }
    }

    /// <summary>
    /// Loads a calendar file for the given calendar url.
    /// </summary>
    /// <param name="calendarUrl">The calendar url.</param>
    /// <returns>The file path the data is copied to.</returns>
    private async Task<string?> LoadCalendarFileFromServer(string calendarUrl)
    {
        try
        {
            using var httpClient = new HttpClient();
            using var stream = await httpClient.GetStreamAsync(calendarUrl);
            var fileName = $"{DateTimeOffset.Now:yyyyMMdd_HHmmss}.ics";
            using var fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fileStream);
            return fileName;
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "Error loading the ICS calendar for url {CalendarUrl}.", calendarUrl);
            return null;
        }
    }

    /// <summary>
    /// Logs the memory information.
    /// </summary>
    /// <param name="heartbeatIntervalInMilliSeconds">The heartbeat interval in milliseconds.</param>
    /// <param name="serviceName">The service name.</param>
    private void LogMemoryInformation(int heartbeatIntervalInMilliSeconds, string serviceName)
    {
        // Log memory information if the heartbeat is expired.
        if (this.LastHeartbeatAt.IsExpired(TimeSpan.FromMilliseconds(heartbeatIntervalInMilliSeconds)))
        {
            // Run the heartbeat and log some memory information.
            this.LogMemoryInformation(serviceName);
            this.LastHeartbeatAt = DateTimeOffset.Now;
        }
    }

    /// <summary>
    /// Logs the memory information.
    /// </summary>
    /// <param name="serviceName">The service name.</param>
    private void LogMemoryInformation(string serviceName)
    {
        var totalMemory = GC.GetTotalMemory(false);
        var memoryInfo = GC.GetGCMemoryInfo();
        var totalMemoryFormatted = SystemGlobals.GetValueWithUnitByteSize(totalMemory);
        var heapSizeFormatted = SystemGlobals.GetValueWithUnitByteSize(memoryInfo.HeapSizeBytes);
        var memoryLoadFormatted = SystemGlobals.GetValueWithUnitByteSize(memoryInfo.MemoryLoadBytes);
        this.Logger.Information(
            "Heartbeat for service {ServiceName}: Total {Total}, heap size: {HeapSize}, memory load: {MemoryLoad}, uptime {Uptime}",
            serviceName, totalMemoryFormatted, heapSizeFormatted, memoryLoadFormatted, this.uptimeStopWatch.Elapsed);
    }
}
