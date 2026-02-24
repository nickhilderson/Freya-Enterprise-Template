namespace Freya.Core.IO;

/// <summary>
/// Provides path helpers.
/// </summary>
public static class Paths
{
    /// <summary>
    /// Gets a relative path using forward slashes, suitable for manifests and markdown.
    /// </summary>
    public static string GetRelativePath(string basePath, string fullPath)
    {
        var relative = Path.GetRelativePath(basePath, fullPath);
        return relative.Replace('\\', '/');
    }
}