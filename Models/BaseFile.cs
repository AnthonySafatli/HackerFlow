namespace HackerFlow.Models;

public class BaseFile
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Version { get; set; } = 1;

    public string FilePath { get; set; } = "";

    public string Notes { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}