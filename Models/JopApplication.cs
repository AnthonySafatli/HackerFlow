namespace HackerFlow.Models;

public class JobApplication
{
    public int Id { get; set; }

    public string Company { get; set; } = "";

    public string Role { get; set; } = "";

    public string Url { get; set; } = "";

    public string JobDescription { get; set; } = "";

    public DateTime? DateApplied { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public string Method { get; set; } = "";

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Bookmarked;

    public string Contact { get; set; } = "";

    public string Notes { get; set; } = "";

    public string ResumePath { get; set; } = "";

    public string CoverLetterPath { get; set; } = "";
}

public enum ApplicationStatus
{
    Bookmarked,
    Applied,
    Interviewing,
    Offered,
    Rejected
}