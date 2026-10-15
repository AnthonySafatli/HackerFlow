namespace HackerFlow.Models;

public class Connection
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public Closeness Closeness { get; set; } 

    public ConnectionLevel Level { get; set; }

    public bool CanIntroduceMe { get; set; }

    public int? IntroducedById { get; set; }
    public Connection? IntroducedBy { get; set; }

    // Inverse of IntroducedBy
    public List<Connection> IntroducedTo { get; set; } = new();

    // Many-to-many with Company
    public List<Company> Companies { get; set; } = new();


    public DateTime? ConnectedAt { get; set; }

    public DateTime? LastContactAt { get; set; }
}

public enum Closeness
{
    Weak = 1,
    Familiar = 2,
    Close = 3,
    VeryClose = 4
}

public enum ConnectionLevel
{
    Direct = 0,
    First = 1,
    Second = 2,
    Third = 3
}