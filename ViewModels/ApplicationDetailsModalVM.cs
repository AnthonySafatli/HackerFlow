using HackerFlow.Models;

namespace HackerFlow.ViewModels;

public class ApplicaitonDetailsModalVM
{
    public JobApplication JobApplicaiton { get; set; } = new();
    public List<string> ResumeGroups { get; set; } = [];
    public List<string> CoverLetterGroups { get; set; } = [];
    public List<string> Prompts { get; set; } = [];
}