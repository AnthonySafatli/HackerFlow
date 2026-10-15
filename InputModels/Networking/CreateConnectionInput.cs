using HackerFlow.Models;

namespace HackerFlow.InputModels.Networking;

public class CreateConnectionInput
{
    public string Name { get; set; } = "";

    public List<string> CompanyNames { get; set; } = new();

    public Closeness Closeness { get; set; }

    public ConnectionLevel Level { get; set; }

    public int? IntroducedById { get; set; }

    public DateTime? ConnectedAt { get; set; }

    public DateTime? LastContactAt { get; set; }

    public bool CanIntroduceMe { get; set; }
}