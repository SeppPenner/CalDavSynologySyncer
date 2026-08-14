// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileHelperTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="FileHelper" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Tests;

/// <summary>
/// A class to test the <see cref="FileHelper"/> class. Every test works in its own directory below the temp path
/// and removes it afterwards, so a test run leaves the working tree untouched.
/// </summary>
[TestClass]
public class FileHelperTests
{
    /// <summary>
    /// The directory of the running test.
    /// </summary>
    private string testDirectory = string.Empty;

    /// <summary>
    /// Creates the directory of the test.
    /// </summary>
    [TestInitialize]
    public void Initialize()
    {
        this.testDirectory = TestDataProvider.CreateTestDirectory();
    }

    /// <summary>
    /// Removes the directory of the test.
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(this.testDirectory))
        {
            Directory.Delete(this.testDirectory, true);
        }
    }

    /// <summary>
    /// Checks whether an existing file is deleted and nothing is logged.
    /// </summary>
    [TestMethod]
    public void AnExistingFileIsDeleted()
    {
        var path = Path.Combine(this.testDirectory, "calendar.ics");
        File.WriteAllText(path, "BEGIN:VCALENDAR");
        var sink = new CollectingSink();

        var deleted = FileHelper.TryDelete(path, TestDataProvider.GetLogger(sink));

        Assert.IsTrue(deleted);
        Assert.IsFalse(File.Exists(path));
        Assert.AreEqual(0, sink.Events.Count);
    }

    /// <summary>
    /// Checks whether a missing file is reported as deleted. <see cref="File.Delete"/> does not throw for a path
    /// that does not exist, so the caller cannot tell a deletion from a no-op. That is why the service has to
    /// pass the full path: with a relative name it deleted nothing and still logged a deletion.
    /// </summary>
    [TestMethod]
    public void AMissingFileIsReportedAsDeleted()
    {
        var path = Path.Combine(this.testDirectory, "notthere.ics");
        var sink = new CollectingSink();

        var deleted = FileHelper.TryDelete(path, TestDataProvider.GetLogger(sink));

        Assert.IsTrue(deleted);
        Assert.AreEqual(0, sink.Events.Count);
    }

    /// <summary>
    /// Checks whether a file that cannot be deleted ends up as false and as one logged error.
    /// </summary>
    [TestMethod]
    public void AnOpenFileIsNotDeletedAndTheErrorIsLogged()
    {
        var path = Path.Combine(this.testDirectory, "locked.ics");
        var sink = new CollectingSink();
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

        var deleted = FileHelper.TryDelete(path, TestDataProvider.GetLogger(sink));

        Assert.IsFalse(deleted);
        Assert.IsTrue(File.Exists(path));
        Assert.AreEqual(1, sink.Events.Count);
        Assert.AreEqual(LogEventLevel.Error, sink.Events[0].Level);
        Assert.AreEqual("File couldn't be deleted.", sink.Events[0].RenderMessage());
        Assert.IsNotNull(sink.Events[0].Exception);
    }

    /// <summary>
    /// Checks whether a directory that is passed as a file ends up as false and as one logged error. The cleanup
    /// of the service lists files only, this pins that a wrong path does not tear the cycle down.
    /// </summary>
    [TestMethod]
    public void ADirectoryIsNotDeletedAndTheErrorIsLogged()
    {
        var sink = new CollectingSink();

        var deleted = FileHelper.TryDelete(this.testDirectory, TestDataProvider.GetLogger(sink));

        Assert.IsFalse(deleted);
        Assert.IsTrue(Directory.Exists(this.testDirectory));
        Assert.AreEqual(1, sink.GetMessages(LogEventLevel.Error).Count);
    }
}
