namespace HackerFlow.InputModels.Resume;

public class CreateResumeInput
{
    public string Name { get; set; } = "";
    public IFormFile File { get; set; } = null!;
}