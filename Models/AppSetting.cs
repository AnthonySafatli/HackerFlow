namespace HackerFlow.Models;

public class AppSetting
{
    public const string ResumeFolder = "ResumePath";
    public const string GerenatedResumeFolder = "GeneratedResumePath";

    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";
}

/*
Current Settings

- Resume Save Path: The path where resumes will be saved. Default is "~/Document/Resumes/Generated".
- Cover Letter Save Path: The path where cover letters will be saved. Default is "~/Document/CoverLetters/Generated".
- Prompt: The prompt used for generating for AI generated cover letter
*/