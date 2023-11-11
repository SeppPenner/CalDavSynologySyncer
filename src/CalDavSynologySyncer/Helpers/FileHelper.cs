namespace CalDavSynologySyncer.Helpers;

/// <summary>
/// A class that contains helper methods for the file class.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Tries to delete a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A value indicating whether the file was deleted or not.</returns>
    public static bool TryDelete(string path, ILogger logger)
    {
        try
        {
            File.Delete(path);
            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "File couldn't be deleted.");
            return false;
        }
    }
}
