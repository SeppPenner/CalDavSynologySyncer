# Project rules for Claude

## What this is

CalDavSynologySyncer is a service that copies events from one or more read only ICS calendar urls
into one calendar on a Synology NAS or on any other CalDAV server. It runs as a console
application, as a Windows service, as a systemd service or in Docker. The published artifacts are
the Docker images [sepppenner/caldavsynologysyncer](https://hub.docker.com/repository/docker/sepppenner/caldavsynologysyncer)
and [sepppenner/caldavsynologysyncer-arm](https://hub.docker.com/repository/docker/sepppenner/caldavsynologysyncer-arm)
plus the zipped binaries under `Published/`. The repository is **not** a NuGet package: no
`GeneratePackageOnBuild`, no push script for nuget.org.

One solution `src/CalDavSynologySyncer.sln` with exactly one project:

- `src/CalDavSynologySyncer/CalDavSynologySyncer.csproj`, SDK `Microsoft.NET.Sdk.Web`, `OutputType`
  `Exe`, target framework `net10.0`. The CalDAV access itself comes from the NuGet package
  [HaemmerElectronics.SeppPenner.CalDAVNet](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.CalDAVNet/),
  which is the sibling repository `D:\Projekte\Github\CSharpUndVB\CalDAVNet`.

Layout inside `src/CalDavSynologySyncer`:

- `Program.cs`: the entry point. `ReadConfiguration` reads `appsettings.json` before a host exists,
  `SetupLogging` builds the global `Log.Logger`, `CreateHostBuilder` builds a web host with
  `UseSerilog`, `UseWindowsService` and `UseSystemd`. `ServiceName` is the assembly name and doubles
  as the name of the configuration section.
- `Startup.cs`: the dependency injection. Binds the configuration section a second time, registers
  the configuration, the global logger and `CalDavSynologySyncerService` as a singleton and as
  `IHostedService`.
- `CalDavSynologySyncerService.cs`: the whole synchronisation, a `BackgroundService`. `ExecuteAsync`
  loops with the configured delay, `TryRunServiceTask` runs one cycle and swallows every exception,
  `LoadCalendarFileFromServer` downloads one ICS file, the two `LogMemoryInformation` overloads are
  the heartbeat. New sync logic belongs into `TryRunServiceTask` or into a private helper next to
  it.
- `Configuration/SyncerConfiguration.cs`: every setting plus `IsValid`.
- `Constants/`: `LoggerConfig` (the Serilog configuration for one logger type), `LoggingKeys` (one
  single key) and `SystemGlobals` (the byte size formatting for the heartbeat).
- `Exceptions/ConfigurationException.cs`: thrown by `IsValid`.
- `Extensions/`: `DateTimeOffsetExtensions.IsExpired` and `ObjectExtensions.IsEmptyOrNull`.
- `Helpers/FileHelper.TryDelete`: deletes a file and logs instead of throwing.
- `GlobalUsings.cs`: all usings of the project, including the alias `ILogger = Serilog.ILogger`.
- `appsettings.json`: the shipped configuration, filled with placeholders only.
  `Properties/launchSettings.json` is the Visual Studio profile.
- `Dockerfile` and `Dockerfile.armv7`: the two images. They only copy a finished publish output,
  they do not build.

Repository root: `README.md` (badges and the framework list), `HowToUse.md` (the only usage
documentation, with the annotated JSON sample and the Docker commands), `Changelog.md`,
`License.txt` (MIT), the four batch files, `Published/<version>/*.zip` (the tracked release
binaries), `doc/` (`Links.txt` with the CalDAV RFC and one sample ICS file), `.gitattributes` and
`.gitignore`. The editorconfig lives in `src/.editorconfig`, the dockerignore in `src/.dockerignore`.
There is no `.github` folder, no pipeline file, no `Updating.md` and no screenshots.

## Build

```powershell
dotnet build src/CalDavSynologySyncer.sln -c Release
```

- Single target framework, no multi-targeting. `RuntimeIdentifiers` are not in the project file, the
  four batch files pass `-r` on the command line: `win-x64`, `linux-x64` (Docker), `linux-arm`
  (Docker ARM) and `linux-arm64`.
- All build properties live directly in the one `.csproj`. There is **no**
  `Directory.Build.props` in this repository.
- `TreatWarningsAsErrors` is enabled, so every warning breaks the build, NuGet warnings (`NU****`)
  from restore included. A clean build reports zero warnings, keep it that way.
- `NU1803` (HTTP source usage during restore) is the one warning suppressed via `NoWarn`. Fix
  warnings instead of extending that list. `NuGetAudit` and `NuGetAuditMode=all` are on, so a
  vulnerable transitive package fails the build too.
- Do not reference a package that the ASP.NET Core shared framework already ships.
  `Microsoft.Extensions.Configuration.Json` and `Microsoft.Extensions.Hosting` were referenced
  explicitly until version 1.1.1.0, since .NET 10 that is `NU1510` and therefore a build error. The
  types are available anyway, the reference was redundant.
- The version of `HaemmerElectronics.SeppPenner.CalDAVNet` is 1.0.3, the newest one on nuget.org.
  Version 1.0.4 exists in the sibling repository, drops `net9.0` and updates to Ical.Net 5.2.3,
  which is a breaking change for `CalDavSynologySyncerService`. Do not bump the reference before
  that package is published and the Ical.Net 5 migration is done.
- Versions come from GitVersion.MsBuild out of the git tags, for example `1.1.2-1` for the first
  commit after tag `1.1.1`. Never edit a version property or an assembly version by hand.
- Restore needs nuget.org. If a private feed is configured globally on the machine and answers 404
  for public packages, restore fails with `NU1301`. Then build with an explicit source:
  `dotnet build src/CalDavSynologySyncer.sln --source https://api.nuget.org/v3/index.json`.
- There is no test project, so there is nothing to run with `dotnet test`. A behaviour change is
  verified by running the service against a CalDAV server, or at least by starting the published
  binary and reading the log output. Never claim a run happened without running it.
- The service needs a reachable CalDAV server and a reachable ICS url. With the placeholder
  `appsettings.json` it starts, logs the heartbeat and logs an error per cycle, that is the cheapest
  smoke test.

## Code conventions

Follow the surrounding code, it is consistent throughout every file:

- File header comment block with `<copyright file="..." company="Hämmer Electronics">` and a
  `<summary>`, then the file-scoped namespace.
- XML doc comments on every type and every member, private members included, no exceptions.
  Overrides of `BackgroundService` additionally carry `<inheritdoc cref="BackgroundService"/>`, the
  class itself carries `<seealso cref="BackgroundService"/>` as well.
- `Nullable` and `ImplicitUsings` are enabled, `LangVersion` is not set, so the default of the
  target framework applies.
- New `using` directives go into `GlobalUsings.cs`, inside the existing `#pragma warning disable
  IDE0065` block, never at the top of a file. The editorconfig requires usings inside the namespace
  (`csharp_using_directive_placement=inside_namespace:warning`), which global usings cannot satisfy,
  that is what the pragma is for. Do not add other pragmas. The comment text in that block is German
  because Visual Studio generated it, leave it alone.
- Fields, properties, methods and events are always accessed with `this.` qualification
  (`dotnet_style_qualification_for_*` at severity `warning`).
- Log messages use Serilog message templates with named holes (`{CalendarUrl}`), never string
  interpolation, and they are English.
- `src/.editorconfig` also enforces braces everywhere, no multiple blank lines, four spaces, CRLF,
  UTF-8, file scoped namespaces, `System` usings sorted first and `IDE0005` as warning. Analyzer
  warnings are fixed, not silenced.
- All source files are UTF-8 **without** BOM with CRLF line endings, the umlaut in `Hämmer
  Electronics` is stored as `C3 A4`. Only the solution file has a BOM.

## Known quirks

Do not silently "clean up" these, they are existing behaviour:

- **The configuration section is named after the assembly.** `Program.ServiceName` is
  `Assembly.GetEntryAssembly()?.GetName().Name`, and both `Program.ReadConfiguration` and the
  `Startup` constructor bind that string as the section name. The section in `appsettings.json` is
  therefore called `CalDavSynologySyncer`. Renaming the assembly silently empties the configuration.
- **The configuration is read twice into two objects.** `Program.ReadConfiguration` fills the static
  `Program.Configuration`, which is only used for the Telegram credentials in `SetupLogging`. The
  host reads `appsettings.json` again and `Startup` binds it into its own `SyncerConfiguration`,
  which is the instance the service gets. Adding a setting that both sides need means both objects
  see it, adding one that only the logging needs still requires the section to be bound twice.
- **`IsValid` throws instead of returning false.** Every failing check throws a
  `ConfigurationException`, the method can only return `true`. The
  `if (!Configuration.IsValid()) throw new InvalidOperationException(...)` in `Program` is therefore
  dead code, and a bad configuration crashes the process with a `ConfigurationException` before any
  log sink except the console exists.
- **`RemoveEntriesWithStar` is not validated.** It is the only setting `IsValid` does not check,
  because `false` is a valid value.
- **The web host serves nothing.** `ConfigureWebHostDefaults` starts Kestrel on `ASPNETCORE_URLS`
  and `Startup` registers controllers, JSON options and Razor page options, but there is no
  controller, no page and no endpoint in the repository, so every request answers 404. The web host
  exists because the service was written from a web template, the actual work happens in the
  `BackgroundService`. The Docker images set port 5000 but `docker run` in `HowToUse.md` publishes
  no port.
- **Two loggers write into each other.** `Program.SetupLogging` creates the global `Log.Logger`
  (console plus optional Telegram), the service creates a second logger via
  `LoggerConfig.GetLoggerConfiguration` and attaches the global one as a sink with
  `WriteTo.Sink((ILogEventSink)Log.Logger)`. That cast works because Serilog's `Logger` implements
  `ILogEventSink`. The service logger has `MinimumLevel.Debug`, the global one `Information`, so the
  effective level is the stricter one of the two.
- **Telegram logging is optional, the rest is not.** `SetupLogging` only adds the Telegram sink when
  `TelegramBotToken` is set, and it only sends `Warning` and above. There is no file sink at all,
  the console is the log.
- **ICS files live next to the assembly, not in the working directory.** `LoadCalendarFileFromServer`
  builds the path from `Assembly.GetExecutingAssembly().Location`, and the cleanup loop at the start
  of every cycle deletes with `FileInfo.FullName`. Both have to stay absolute: up to version
  1.1.1.0 the download wrote a relative name and the cleanup deleted a relative name, so with any
  working directory other than the assembly folder the downloaded files piled up while the log
  claimed they were deleted. Keep the file name pattern `yyyyMMdd_HHmmss.ics`, the cleanup finds
  files by the extension only.
- **`appsettings.json` is read without a base path, and that is fine.** `ReadConfiguration` calls
  `AddJsonFile("appsettings.json", false, true)` on a bare `ConfigurationBuilder`. That looks like it
  depends on the working directory, but `GetFileProvider` falls back to `AppContext.BaseDirectory`,
  so the file is read next to the assembly. Verified by starting the published binary with `C:\` as
  the working directory.
- **The star rule.** Events whose summary starts with `*` are preliminary, usually entered by hand
  in the Synology calendar. When `RemoveEntriesWithStar` is on, such an event is compared with the
  event of the same summary without the star, and if exactly one exists and both start less than
  four days apart, the preliminary one with the star is deleted. The four days are hard coded. Up to
  version 1.1.1.0 the code deleted the corresponding event without the star instead, which threw
  away the real appointment.
- **One empty calendar for every write.** `CalDavSynologySyncerService.dummyCalendar` is a single
  empty `Ical.Net.Calendar` that is passed to `Client.AddOrUpdateEvent` for every event, the
  parameter only exists because the library needs a calendar to serialize into.
- **The update check compares 30 properties by hand.** `TryRunServiceTask` has one `if` per
  `CalendarEvent` property and sets `needsUpdate`. `Parent` is commented out on purpose, comparing
  it would report a difference for every event. Adding a property means adding another `if`.
- **The Dockerfiles do not build.** Both consist of `FROM`, `WORKDIR /app`, `COPY publish .`, the
  environment and the `CMD`. `dotnet publish --output publish/` has to run first, in
  `src/CalDavSynologySyncer`, which is also the build context. The two files are identical except
  for the `--platform=linux/arm/v7` in `Dockerfile.armv7`.
- **The publish folder is not ignored.** `src/CalDavSynologySyncer/publish` is the output of every
  build script and of the Docker build, but `.gitignore` does not list it, so it shows up as
  untracked. Never `git add -A` in this repository without looking at what that collects.
- **`src/.dockerignore` is in the wrong folder.** Docker only reads the `.dockerignore` of the build
  context, and the build context is `src/CalDavSynologySyncer`, not `src`. The file has no effect on
  the images.
- **`appsettings.Development.json` is gitignored but not excluded from publish.** The Web SDK copies
  every `appsettings.*.json` into the publish output, so a local development configuration ends up
  in the zips and in the Docker images. That is how real credentials got into
  `Published/1.1.1/win-x64.zip` and into all four published images (`caldavsynologysyncer` and
  `caldavsynologysyncer-arm`, tags `1.1.0` and `1.1.1`), see the changelog of version 1.1.2.0.
  Check the publish output for that file before building an image or a zip.
- **Tracked release binaries.** Every release adds a folder `Published/<version>/` with
  `win-x64.zip` and `linux-arm64.zip`, roughly one megabyte each. Version 1.0.0 used one single
  `publish.zip` instead. There is no zip for `linux-x64`, that platform only gets a Docker image.
- **AppVeyor badge without CI in the repository.** `README.md` links an AppVeyor build that is
  configured outside of this repository. There is no `.github` folder and no pipeline file here.
- **`.gitattributes` sets `* text=auto`** and every rule of the Visual Studio template below it is
  commented out. The tracked zip files are recognized as binary by content, a new binary file that
  git could misread needs its own rule.

## Releasing

1. Make the change.
2. Add an entry at the top of `Changelog.md` in the existing format:
   `* **Version 1.1.2.0 (2026-08-11)** : Short description.`
3. Bump the image tag in `buildAndUploadDocker.bat` and `buildAndUploadDockerForArm.bat`, both the
   `docker build` and the `docker push` line.
4. Commit that.
5. Tag the commit with the plain version number, no `v` prefix (`1.1.1`, `1.1.0`, `1.0.0`). The
   existing tags are lightweight tags, create new ones the same way. The tag has to exist **before**
   the binaries are built, otherwise GitVersion stamps a prerelease version such as `1.1.2-1` into
   the shipped assembly.
6. Run `buildForWindows.bat` and `buildForLinuxArm64.bat`, zip the two publish outputs into
   `Published/<version>/win-x64.zip` and `Published/<version>/linux-arm64.zip` and commit them.
   Check the zip content before committing, the publish folder is reused between runs.
7. Push the commits and the tag.
8. `buildAndUploadDocker.bat` and `buildAndUploadDockerForArm.bat` publish, build the image, log in
   with `DOCKERHUB_CLI_TOKEN` from the environment and push to Docker Hub. Never run them unless
   explicitly asked to publish.

The version in `Changelog.md` has four parts (`1.1.2.0`), the tag has three (`1.1.2`), the Docker
tag has three as well. GitVersion turns the tag into the assembly version.

Also update the "Available for" list in `README.md` when the target framework changes.

## Git

- **Never amend a commit.** No `git commit --amend`, not for a typo in the message, not to add a
  forgotten file, not even when the commit is still local. Write a follow-up commit instead. The
  release versions come from tags on exact commits, an amended commit leaves its tag pointing at a
  commit that no longer exists in the branch.

## Writing style

- Commit messages are written **in English only**: short, precise subject line, explanatory body
  when needed.
- Code comments and comments in project files such as `.csproj` are **always English**, regardless
  of the language used in the conversation.
- **No em dashes or en dashes** (`—`, `–`), neither in prose, commit messages, code comments nor
  documentation. Use a regular hyphen, comma, colon, parentheses or a separate sentence.
- German texts (documentation, chat replies) always use real umlauts and ß, never ASCII
  transliterations such as `ae`, `oe`, `ue` or `ss`. Identifiers, file names and configuration keys
  stay unchanged where umlauts are technically undesirable.
