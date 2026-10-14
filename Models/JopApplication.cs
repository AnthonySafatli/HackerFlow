namespace HackerFlow.Models;

public class JobApplication
{
    public int Id { get; set; }

    public Company Company { get; set; } = new Company();
    public int CompanyId { get; set; }

    public string Role { get; set; } = "";

    public string Url { get; set; } = "";

    public string JobDescription { get; set; } = "";

    public DateTime? DateApplied { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public string Method { get; set; } = "";

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Bookmarked;

    public Connection? Contact { get; set; }
    public int? ContactId { get; set; }

    public string Notes { get; set; } = "";

    public string ResumePath { get; set; } = "";

    public string CoverLetterPath { get; set; } = "";
}

public enum ApplicationStatus
{
    Bookmarked = 0,
    Applying = 1,
    Applied = 2,
    Interviewing = 3,
    Offered = 4,
    Accepted = 5,
    Rejected = 6,
    Archived = 7
}