namespace Freya.Core.IO;

/// <summary>
///     Provides file copy helpers with basic validation and diagnostics.
/// </summary>
public static class FileCopy
{
    /// <summary>
    ///     Copies a file to a destination path, ensuring the destination directory exists.
    /// </summary>
    /// <param name="sourceFilePath">The existing source file path.</param>
    /// <param name="destinationFilePath">The destination file path to create/overwrite.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public static async Task CopyAsync(
        string sourceFilePath,
        string destinationFilePath,
        CancellationToken cancellationToken)
    {
        var destinationDir = Path.GetDirectoryName(destinationFilePath);
        if (!string.IsNullOrWhiteSpace(destinationDir)) Directory.CreateDirectory(destinationDir);

        await using var sourceStream = File.OpenRead(sourceFilePath);
        await using var destinationStream = File.Create(destinationFilePath);

        await sourceStream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Returns the file size in bytes or 0 if the file does not exist.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public static long GetSizeOrZero(string filePath)
    {
        return File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
    }
}