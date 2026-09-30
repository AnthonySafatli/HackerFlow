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
    public async Task<string> SaveAsync(string directory, string fileName, Stream content)
    {
        var path = BuildPath(directory, fileName);
        await using var stream = File.Create(path);
        await content.CopyToAsync(stream);
        return path;
    }

    /// <summary>
    /// Saves a stream directly to the specified path. Creates the parent directory if needed.
    /// </summary>
    public async Task<string> SaveAsync(string path, Stream content)
    {
        var fullPath = Resolve(path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var stream = File.Create(fullPath);
        await content.CopyToAsync(stream);
        return fullPath;
    }

    /// <summary>
    /// Saves a byte array to a file, creating the directory if needed. Overwrites any existing file.
    /// </summary>
    public async Task<string> SaveAsync(string directory, string fileName, byte[] content)
    {
        var path = BuildPath(directory, fileName);
        await File.WriteAllBytesAsync(path, content);
        return path;
    }

    /// <summary>
    /// Saves a byte array directly to the specified path. Creates the parent directory if needed.
    /// </summary>
    public async Task<string> SaveAsync(string path, byte[] content)
    {
        var fullPath = Resolve(path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(fullPath, content);
        return fullPath;
    }

    /// <summary>
    /// Gets the full path of an existing file without creating any directories.
    /// </summary>
    public string? GetPath(string directory, string fileName)
    {
        var path = BuildPath(directory, fileName, create: false);
        return File.Exists(path) ? path : null;
    }

    /// <summary>
    /// Gets the full path of an existing file from its complete path.
    /// </summary>
    public string? GetPath(string path)
    {
        var fullPath = Resolve(path);
        return File.Exists(fullPath) ? fullPath : null;
    }

    /// <summary>
    /// Reads all bytes of a file.
    /// </summary>
    public async Task<byte[]?> ReadAsync(string directory, string fileName)
    {
        var path = GetPath(directory, fileName);
        return path == null ? null : await File.ReadAllBytesAsync(path);
    }

    /// <summary>
    /// Reads all bytes of a file from its complete path.
    /// </summary>
    public async Task<byte[]?> ReadAsync(string path)
    {
        var fullPath = GetPath(path);
        return fullPath == null ? null : await File.ReadAllBytesAsync(fullPath);
    }

    /// <summary>
    /// Lists the names of all files directly inside a directory (non-recursive).
    /// The directory may be relative, absolute, or "~"-prefixed.
    /// </summary>
    public IEnumerable<string> List(string directory)
    {
        var dir = Resolve(directory);

        return Directory.Exists(dir)
            ? Directory.EnumerateFiles(dir)
                .Select((string? path) => Path.GetFileName(path) ?? "")
                .Where(name => name != null)
            : [];
    }

    /// <summary>
    /// Checks whether a file exists in a directory.
    /// </summary>
    public bool Exists(string directory, string fileName) =>
        File.Exists(BuildPath(directory, fileName, create: false));

    /// <summary>
    /// Checks whether a file exists at the specified complete path.
    /// </summary>
    public bool Exists(string path) =>
        File.Exists(Resolve(path));

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    public bool Delete(string directory, string fileName)
    {
        var path = BuildPath(directory, fileName, create: false);

        if (!File.Exists(path))
            return false;

        File.Delete(path);
        return true;
    }

    /// <summary>
    /// Deletes a file by its complete path.
    /// </summary>
    public bool Delete(string path)
    {
        var fullPath = Resolve(path);

        if (!File.Exists(fullPath))
            return false;

        File.Delete(fullPath);
        return true;
    }

    public async Task<string?> MoveAsync(
    string sourcePath,
    string destinationDirectory,
    string? destinationFileName = null)
    {
        var source = Resolve(sourcePath);

        if (!File.Exists(source))
            return null;

        var fileName = destinationFileName ?? Path.GetFileName(source);
        var destination = BuildPath(destinationDirectory, fileName);

        File.Move(source, destination, overwrite: true);

        return destination;
    }

    /// <summary>
    /// Expands a leading "~" to the user's home folder and returns the full absolute path.
    /// </summary>
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
    /// to block path traversal.
    /// </summary>
    private static string BuildPath(
        string directory,
        string fileName,
        bool create = true)
    {
        var dir = Resolve(directory);

        if (create)
            Directory.CreateDirectory(dir);

        return Path.Combine(dir, Path.GetFileName(fileName));
    }
}

/// <summary>
/// Abstraction for simple directory-based file storage.
/// </summary>
public interface IFileService
{
    Task<string> SaveAsync(string directory, string fileName, Stream content);

    Task<string> SaveAsync(string path, Stream content);

    Task<string> SaveAsync(string directory, string fileName, byte[] content);

    Task<string> SaveAsync(string path, byte[] content);

    string? GetPath(string directory, string fileName);

    string? GetPath(string path);

    Task<byte[]?> ReadAsync(string directory, string fileName);

    Task<byte[]?> ReadAsync(string path);

    IEnumerable<string> List(string directory);

    bool Exists(string directory, string fileName);

    bool Exists(string path);

    bool Delete(string directory, string fileName);

    bool Delete(string path);

    Task<string?> MoveAsync(
        string sourcePath, 
        string destinationDirectory, 
        string? destinationFileName = null);
}
