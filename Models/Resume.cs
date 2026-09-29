namespace HackerFlow.Models;

public class Resume
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Version { get; set; } = 1;

    public string FilePath { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}