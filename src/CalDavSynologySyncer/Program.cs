// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The program class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer;

/// <summary>
/// The program class.
/// </summary>
public static class Program
{
    /// <summary>
    /// Gets the service assembly name.
    /// </summary>
    private static AssemblyName? ServiceAssemblyName => Assembly.GetEntryAssembly()?.GetName();

    /// <summary>
    /// Gets the service version.
    /// </summary>
    private static Version ServiceVersion => ServiceAssemblyName?.Version ?? new Version();

    /// <summary>
    /// Gets or sets the CalDav Synology syncer service configuration.
    /// </summary>
    private static SyncerConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// Gets the environment name.
    /// </summary>
    private static string EnvironmentName
    {
        get
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(env))
            {
                env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
            }

            return env;
        }
    }

    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public static string ServiceName { get; set; } = ServiceAssemblyName?.Name ?? "---";

    /// <summary>
    /// The main method.
    /// </summary>
    /// <param name="args">Some arguments.</param>
    /// <returns>The result code.</returns>
    public static async Task<int> Main(string[] args)
    {
        ReadConfiguration();
        SetupLogging();

        try
        {
            Log.Information("Starting {ServiceName}, Version {Version}", ServiceName, ServiceVersion);
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            await CreateHostBuilder(args, currentLocation!).Build().RunAsync();
        }
        catch(Exception exc)
        {
            Log.Fatal(exc, "Host terminated unexpectedly.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return 0;
    }

    /// <summary>
    /// Creates the host builder.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="currentLocation">The current assembly location.</param>
    /// <returns>A new <see cref="IHostBuilder"/>.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args, string currentLocation)
    {
        return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(
            webBuilder =>
            {
                webBuilder.UseContentRoot(currentLocation);
                webBuilder.UseStartup<Startup>();
            })
            .UseSerilog()
            .UseWindowsService()
            .UseSystemd();
    }

    /// <summary>
    /// Reads the configuration.
    /// </summary>
    private static void ReadConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", false, true);

        if (!string.IsNullOrWhiteSpace(EnvironmentName))
        {
            var appsettingsFileName = $"appsettings.{EnvironmentName}.json";

            if (File.Exists(appsettingsFileName))
            {
                configurationBuilder.AddJsonFile(appsettingsFileName, false, true);
            }
        }

        var configuration = configurationBuilder.Build();
        configuration.Bind(ServiceName, Configuration);

        if (!Configuration.IsValid())
        {
            throw new InvalidOperationException("The configuration is invalid!");
        }
    }

    /// <summary>
    /// Sets up the logging.
    /// </summary>
    private static void SetupLogging()
    {
        const string customTemplate = "{Timestamp:dd.MM.yy HH:mm:ss.fff}\t[{Level:u3}]\t{Type}\t{Id}\t{Message}{NewLine}{Exception}";

        var loggerConfiguration = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .WriteTo.Console(outputTemplate: customTemplate);            
            // Todo: Enable back.
            //.WriteTo.Telegram(Configuration.TelegramBotToken, Configuration.TelegramChatId, restrictedToMinimumLevel: LogEventLevel.Warning);

        if (EnvironmentName != "Development")
        {
            loggerConfiguration
                .MinimumLevel.Information();
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }
}
