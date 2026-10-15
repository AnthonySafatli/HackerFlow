using System.ComponentModel.DataAnnotations;
using HackerFlow.Models;

namespace HackerFlow.InputModels.Networking;

public class UpdateConnectionInput
{
    public int Id { get; set; }
 
    [Required]
    public string Name { get; set; } = "";
 
    public List<string> CompanyNames { get; set; } = [];
 
    public Closeness Closeness { get; set; }
 
    public ConnectionLevel Level { get; set; }
 
    public int? IntroducedById { get; set; }
 
    public DateTime? ConnectedAt { get; set; }
 
    public DateTime? LastContactAt { get; set; }
 
    public bool CanIntroduceMe { get; set; }
}