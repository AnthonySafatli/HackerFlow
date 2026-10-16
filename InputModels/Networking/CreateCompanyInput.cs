using HackerFlow.Models;

namespace HackerFlow.InputModels.Networking;

public class CreateCompanyInput
{
    public string Name { get; set; } = "";

    public string? Website { get; set; }

    public ReferralChance ReferralChance { get; set; }

    public string? Description { get; set; }
}