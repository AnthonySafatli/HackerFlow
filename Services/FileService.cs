namespace HackerFlow.Services;

public class FileService : IFileService
{
    public async Task<string> SaveAsync(string directory, string fileName, Stream content)
    {
        var path = BuildPath(directory, fileName);
        await using var stream = File.Create(path);
        await content.CopyToAsync(stream);
        return path;
    }

    public async Task<string> SaveAsync(string directory, string fileName, byte[] content)
    {
        var path = BuildPath(directory, fileName);
        await File.WriteAllBytesAsync(path, content);
        return path;
    }

    public string? GetPath(string directory, string fileName)
    {
        var path = BuildPath(directory, fileName, create: false);
        return File.Exists(path) ? path : null;
    }

    public async Task<byte[]?> ReadAsync(string directory, string fileName)
    {
        var path = GetPath(directory, fileName);
        return path == null ? null : await File.ReadAllBytesAsync(path);
    }

    public IEnumerable<string> List(string directory)
    {
        var dir = Resolve(directory);
        return Directory.Exists(dir)
            ? Directory.EnumerateFiles(dir)
                .Select((string? path) => Path.GetFileName(path) ?? "")!
            : [];
    }

    public bool Exists(string directory, string fileName) =>
        File.Exists(BuildPath(directory, fileName, create: false));

    public bool Delete(string directory, string fileName)
    {
        var path = BuildPath(directory, fileName, create: false);
        if (!File.Exists(path)) return false;
        File.Delete(path);
        return true;
    }

    // Expands "~" to the user's home folder
    private static string Resolve(string path)
    {
        if (path.StartsWith("~"))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(home, path[1..].TrimStart('/', '\\'));
        }
        return Path.GetFullPath(path);
    }

    // Strips directory parts from fileName to block path traversal
    private static string BuildPath(string directory, string fileName, bool create = true)
    {
        var dir = Resolve(directory);
        if (create) Directory.CreateDirectory(dir);
        return Path.Combine(dir, Path.GetFileName(fileName));
    }
}

public interface IFileService
{
    Task<string> SaveAsync(string directory, string fileName, Stream content);
    Task<string> SaveAsync(string directory, string fileName, byte[] content);
    string? GetPath(string directory, string fileName);
    Task<byte[]?> ReadAsync(string directory, string fileName);
    IEnumerable<string> List(string directory);
    bool Exists(string directory, string fileName);
    bool Delete(string directory, string fileName);
}