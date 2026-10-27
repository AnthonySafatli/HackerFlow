namespace HackerFlow.Models;

public class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public ReferralChance ReferralChance { get; set; } = ReferralChance.None;

    public string Website { get; set; } = "";

    public List<Connection> Connections { get; set; } = new();
    
    public List<JobApplication> Applications { get; set; } = new();
}

public enum ReferralChance
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3
}