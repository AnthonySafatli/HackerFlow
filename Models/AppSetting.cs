namespace HackerFlow.Models;

public class AppSetting
{
    public const string ResumeFolder = "ResumePath";
    public const string GerenatedResumeFolder = "GeneratedResumePath";
    public const string CoverLetterFolder = "CoverLetterPath";
    public const string GerenatedCoverLetterFolder = "GeneratedCoverLetterPath";

    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";
}