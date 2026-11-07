namespace Venture.Utils;

public static class PathUtils
{
    /// <summary>
    /// Trims the path and expands a leading "~" to the current user's home directory.
    /// </summary>
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