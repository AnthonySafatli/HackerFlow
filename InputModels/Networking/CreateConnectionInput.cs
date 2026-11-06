using System.ComponentModel.DataAnnotations;
using Venture.Models;

namespace Venture.InputModels.Networking;

public class CreateConnectionInput
{
    [Required]
    public string Name { get; set; } = "";

    public List<string> CompanyNames { get; set; } = new();

    public Closeness Closeness { get; set; }

    public ConnectionLevel Level { get; set; }

    public int? IntroducedById { get; set; }

    public DateTime? ConnectedAt { get; set; }

    public DateTime? LastContactAt { get; set; }

    public bool CanIntroduceMe { get; set; }
}