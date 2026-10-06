using HackerFlow.Models;

namespace HackerFlow.InputModels.Networking;

public class CreateCompanyInput
{
    public string Name { get; set; } = "";

    public string Website { get; set; } = "";

    public ReferralChance ReferralChance { get; set; } = ReferralChance.Low;

    public string Description { get; set; } = "";
}