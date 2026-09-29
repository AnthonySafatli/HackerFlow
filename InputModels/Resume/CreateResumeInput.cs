using System.ComponentModel.DataAnnotations;

namespace HackerFlow.InputModels.Resume;

public class CreateResumeInput
{
    [Required]
    public string Name { get; set; } = "";

    [Required]
    public IFormFile File { get; set; } = null!;

    public string Notes { get; set; } = "";
}