// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectingSink.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A Serilog sink that keeps every log event in memory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Tests;

/// <seealso cref="ILogEventSink"/>
/// <inheritdoc cref="ILogEventSink"/>
/// <summary>
/// A Serilog sink that keeps every log event in memory. It is the only way the tests can check what the code
/// under test logged, and it writes nowhere, so a test run stays silent.
/// </summary>
internal sealed class CollectingSink : ILogEventSink
{
    /// <summary>
    /// The collected log events.
    /// </summary>
    private readonly List<LogEvent> events = new();

    /// <summary>
    /// Gets the collected log events.
    /// </summary>
    public IReadOnlyList<LogEvent> Events => this.events;

    /// <inheritdoc cref="ILogEventSink"/>
    /// <seealso cref="ILogEventSink"/>
    public void Emit(LogEvent logEvent)
    {
        this.events.Add(logEvent);
    }

    /// <summary>
    /// Gets the rendered messages of all collected log events of the given level.
    /// </summary>
    /// <param name="level">The log event level.</param>
    /// <returns>The rendered messages as <see cref="List{T}"/> of <see cref="string"/>.</returns>
    public List<string> GetMessages(LogEventLevel level)
    {
        return this.events
            .Where(e => e.Level == level)
            .Select(e => e.RenderMessage())
            .ToList();
    }
}
