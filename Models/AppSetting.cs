namespace Venture.Models;

public class AppSetting
{
    public const string ResumeFolder = "ResumePath";
    public const string GeneratedResumeFolder = "GeneratedResumePath";
    public const string CoverLetterFolder = "CoverLetterPath";
    public const string GeneratedCoverLetterFolder = "GeneratedCoverLetterPath";
    public const string PackageOutputFolder = "PackageOutputPath";

    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";
}