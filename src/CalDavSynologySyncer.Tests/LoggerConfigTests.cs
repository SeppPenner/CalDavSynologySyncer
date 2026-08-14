// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerConfigTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="LoggerConfig" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Tests;

/// <summary>
/// A class to test the <see cref="LoggerConfig"/> class.
/// </summary>
[TestClass]
public class LoggerConfigTests
{
    /// <summary>
    /// The logger type used in the tests.
    /// </summary>
    private const string LoggerType = nameof(LoggerConfigTests);

    /// <summary>
    /// Checks whether a missing type is rejected with the message as the message and the parameter name as the
    /// parameter name. Up to version 1.1.1.0 both were swapped, so the exception carried the text
    /// <c>type</c> and named the message as its parameter.
    /// </summary>
    [TestMethod]
    public void AMissingTypeIsRejected()
    {
        foreach (var type in new[] { string.Empty, "   " })
        {
            var exception = Assert.ThrowsExactly<ArgumentException>(() => LoggerConfig.GetLoggerConfiguration(type));
            Assert.AreEqual("type", exception.ParamName);
            Assert.IsTrue(exception.Message.StartsWith("The type of logger must be given", StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// Checks whether null is rejected as well.
    /// </summary>
    [TestMethod]
    public void ANullTypeIsRejected()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(() => LoggerConfig.GetLoggerConfiguration(null!));

        Assert.AreEqual("type", exception.ParamName);
    }

    /// <summary>
    /// Checks whether the type ends up as a property of every log event. The console template of the service
    /// prints that property as the column <c>Type</c>.
    /// </summary>
    [TestMethod]
    public void TheTypeEndsUpAsAPropertyOfEveryEvent()
    {
        var sink = new CollectingSink();
        using var logger = LoggerConfig.GetLoggerConfiguration(LoggerType)
            .WriteTo.Sink(sink)
            .CreateLogger();

        logger.Information("Something happened");

        Assert.AreEqual(1, sink.Events.Count);
        Assert.IsTrue(sink.Events[0].Properties.TryGetValue(LoggingKeys.LoggerType, out var property));
        Assert.AreEqual(LoggerType, (property as ScalarValue)?.Value);
    }

    /// <summary>
    /// Checks whether the minimum level is debug, so that the debug events of the service are kept.
    /// </summary>
    [TestMethod]
    public void TheMinimumLevelIsDebug()
    {
        var sink = new CollectingSink();
        using var logger = LoggerConfig.GetLoggerConfiguration(LoggerType)
            .WriteTo.Sink(sink)
            .CreateLogger();

        logger.Verbose("Not kept");
        logger.Debug("Kept");

        Assert.AreEqual(1, sink.Events.Count);
        Assert.AreEqual(LogEventLevel.Debug, sink.Events[0].Level);
    }

    /// <summary>
    /// Checks whether the framework noise is filtered. Everything below a warning that carries a source context
    /// below <c>Microsoft</c> is dropped, the same event from another source context is kept.
    /// </summary>
    [TestMethod]
    public void TheMicrosoftSourceContextIsOverriddenToWarning()
    {
        var sink = new CollectingSink();
        using var logger = LoggerConfig.GetLoggerConfiguration(LoggerType)
            .WriteTo.Sink(sink)
            .CreateLogger();

        logger.ForContext(Serilog.Core.Constants.SourceContextPropertyName,"Microsoft.Hosting.Lifetime").Information("Dropped");
        logger.ForContext(Serilog.Core.Constants.SourceContextPropertyName,"Microsoft.Hosting.Lifetime").Warning("Kept");
        logger.ForContext(Serilog.Core.Constants.SourceContextPropertyName,"CalDavSynologySyncer").Information("Kept as well");

        Assert.AreEqual(2, sink.Events.Count);
        Assert.AreEqual("Kept", sink.Events[0].RenderMessage());
        Assert.AreEqual("Kept as well", sink.Events[1].RenderMessage());
    }

    /// <summary>
    /// Checks whether the machine name enricher is part of the configuration.
    /// </summary>
    [TestMethod]
    public void TheMachineNameIsEnriched()
    {
        var sink = new CollectingSink();
        using var logger = LoggerConfig.GetLoggerConfiguration(LoggerType)
            .WriteTo.Sink(sink)
            .CreateLogger();

        logger.Information("Something happened");

        Assert.IsTrue(sink.Events[0].Properties.ContainsKey("MachineName"));
    }
}
