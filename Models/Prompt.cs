namespace HackerFlow.Models;

public class Prompt
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string PromptString { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}