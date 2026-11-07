namespace Venture.Utils;

public static class PathUtils
{
    /// <summary>
    /// Expands "~", validates, and normalizes a user-supplied absolute path.
    /// On failure, returns false with a user-friendly error.
    /// </summary>
    public static bool TryNormalize(string? input, out string normalized, out string? error)
    {
        normalized = string.Empty;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Path is required.";
            return false;
        }

        var path = ExpandHome(input);

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            error = "Path contains invalid characters.";
            return false;
        }

        if (!Path.IsPathFullyQualified(path))
        {
            error = "Path must be absolute.";
            return false;
        }

        // Validate each segment after the root (so "C:" isn't flagged on Windows)
        var root = Path.GetPathRoot(path) ?? string.Empty;
        var segments = path[root.Length..]
            .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                   StringSplitOptions.RemoveEmptyEntries);

        var invalidNameChars = Path.GetInvalidFileNameChars();

        foreach (var segment in segments)
        {
            if (segment is "." or "..") continue;

            if (segment.IndexOfAny(invalidNameChars) >= 0)
            {
                error = $"\"{segment}\" contains invalid characters.";
                return false;
            }

            if (OperatingSystem.IsWindows() && (segment.EndsWith(' ') || segment.EndsWith('.')))
            {
                error = $"\"{segment}\" can't end with a space or period.";
                return false;
            }
        }

        try
        {
            path = Path.GetFullPath(path); // resolves "..", fixes separators
        }
        catch (PathTooLongException)
        {
            error = "Path is too long.";
            return false;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException)
        {
            error = "Path is not valid on this system.";
            return false;
        }

        // Drop trailing separators, but keep a bare root like "/" or "C:\"
        var fullRoot = Path.GetPathRoot(path) ?? string.Empty;
        if (path.Length > fullRoot.Length)
            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        normalized = path;
        return true;
    }

    public static string ExpandHome(string path)
    {
        path = path.Trim();

        if (path == "~" || path.StartsWith("~/") || path.StartsWith("~\\"))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(home, path[1..].TrimStart('/', '\\'));
        }

        return path;
    }
}