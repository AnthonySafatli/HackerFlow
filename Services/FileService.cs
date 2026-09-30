namespace HackerFlow.Services;

/// <summary>
/// File storage service for saving, reading, listing, and deleting files
/// within a directory. Supports "~" expansion and blocks path traversal in file names.
/// </summary>
public class FileService : IFileService
{
    /// <summary>
    /// Saves a stream to a file, creating the directory if needed. Overwrites any existing file.
    /// </summary>
    /// <param name="directory">Target directory. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <param name="content">Stream whose contents are written to the file.</param>
    /// <returns>The full path of the saved file.</returns>
    public async Task<string> SaveAsync(string directory, string fileName, Stream content)
    {
        var path = BuildPath(directory, fileName);
        await using var stream = File.Create(path);
        await content.CopyToAsync(stream);
        return path;
    }

    /// <summary>
    /// Saves a byte array to a file, creating the directory if needed. Overwrites any existing file.
    /// </summary>
    /// <param name="directory">Target directory. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <param name="content">Bytes to write to the file.</param>
    /// <returns>The full path of the saved file.</returns>
    public async Task<string> SaveAsync(string directory, string fileName, byte[] content)
    {
        var path = BuildPath(directory, fileName);
        await File.WriteAllBytesAsync(path, content);
        return path;
    }

    /// <summary>
    /// Gets the full path of an existing file without creating any directories.
    /// </summary>
    /// <param name="directory">Directory to look in. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <returns>The full path if the file exists; otherwise <c>null</c>.</returns>
    public string? GetPath(string directory, string fileName)
    {
        var path = BuildPath(directory, fileName, create: false);
        return File.Exists(path) ? path : null;
    }

    /// <summary>
    /// Reads all bytes of a file.
    /// </summary>
    /// <param name="directory">Directory to look in. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <returns>The file's bytes, or <c>null</c> if the file doesn't exist.</returns>
    public async Task<byte[]?> ReadAsync(string directory, string fileName)
    {
        var path = GetPath(directory, fileName);
        return path == null ? null : await File.ReadAllBytesAsync(path);
    }

    /// <summary>
    /// Lists the names of all files directly inside a directory (non-recursive).
    /// </summary>
    /// <param name="directory">Directory to list. Supports "~" for the user's home folder.</param>
    /// <returns>File names only (not full paths). Empty if the directory doesn't exist.</returns>
    public IEnumerable<string> List(string directory)
    {
        var dir = Resolve(directory);
        return Directory.Exists(dir)
            ? Directory.EnumerateFiles(dir)
                .Select((string? path) => Path.GetFileName(path) ?? "")!
            : [];
    }

    /// <summary>
    /// Checks whether a file exists in a directory.
    /// </summary>
    /// <param name="directory">Directory to look in. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <returns><c>true</c> if the file exists; otherwise <c>false</c>.</returns>
    public bool Exists(string directory, string fileName) =>
        File.Exists(BuildPath(directory, fileName, create: false));

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    /// <param name="directory">Directory containing the file. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <returns><c>true</c> if the file was deleted; <c>false</c> if it didn't exist.</returns>
    public bool Delete(string directory, string fileName)
    {
        var path = BuildPath(directory, fileName, create: false);
        if (!File.Exists(path)) return false;
        File.Delete(path);
        return true;
    }

    /// <summary>
    /// Expands a leading "~" to the user's home folder and returns the full absolute path.
    /// </summary>
    /// <param name="path">A relative, absolute, or "~"-prefixed path.</param>
    /// <returns>The resolved absolute path.</returns>
    private static string Resolve(string path)
    {
        if (path.StartsWith("~"))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(home, path[1..].TrimStart('/', '\\'));
        }
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Builds the full file path, stripping directory parts from <paramref name="fileName"/>
    /// to block path traversal (e.g. "../../secret.txt" becomes "secret.txt").
    /// </summary>
    /// <param name="directory">Target directory. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <param name="create">If <c>true</c>, creates the directory when it doesn't exist.</param>
    /// <returns>The full path to the file.</returns>
    private static string BuildPath(string directory, string fileName, bool create = true)
    {
        var dir = Resolve(directory);
        if (create) Directory.CreateDirectory(dir);
        return Path.Combine(dir, Path.GetFileName(fileName));
    }
}

/// <summary>
/// Abstraction for simple directory-based file storage.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Saves a stream to a file, creating the directory if needed. Overwrites any existing file.
    /// </summary>
    /// <param name="directory">Target directory. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <param name="content">Stream whose contents are written to the file.</param>
    /// <returns>The full path of the saved file.</returns>
    Task<string> SaveAsync(string directory, string fileName, Stream content);

    /// <summary>
    /// Saves a byte array to a file, creating the directory if needed. Overwrites any existing file.
    /// </summary>
    /// <param name="directory">Target directory. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <param name="content">Bytes to write to the file.</param>
    /// <returns>The full path of the saved file.</returns>
    Task<string> SaveAsync(string directory, string fileName, byte[] content);

    /// <summary>
    /// Gets the full path of an existing file without creating any directories.
    /// </summary>
    /// <param name="directory">Directory to look in. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <returns>The full path if the file exists; otherwise <c>null</c>.</returns>
    string? GetPath(string directory, string fileName);

    /// <summary>
    /// Reads all bytes of a file.
    /// </summary>
    /// <param name="directory">Directory to look in. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <returns>The file's bytes, or <c>null</c> if the file doesn't exist.</returns>
    Task<byte[]?> ReadAsync(string directory, string fileName);

    /// <summary>
    /// Lists the names of all files directly inside a directory (non-recursive).
    /// </summary>
    /// <param name="directory">Directory to list. Supports "~" for the user's home folder.</param>
    /// <returns>File names only (not full paths). Empty if the directory doesn't exist.</returns>
    IEnumerable<string> List(string directory);

    /// <summary>
    /// Checks whether a file exists in a directory.
    /// </summary>
    /// <param name="directory">Directory to look in. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <returns><c>true</c> if the file exists; otherwise <c>false</c>.</returns>
    bool Exists(string directory, string fileName);

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    /// <param name="directory">Directory containing the file. Supports "~" for the user's home folder.</param>
    /// <param name="fileName">Name of the file. Any directory parts are stripped.</param>
    /// <returns><c>true</c> if the file was deleted; <c>false</c> if it didn't exist.</returns>
    bool Delete(string directory, string fileName);
}