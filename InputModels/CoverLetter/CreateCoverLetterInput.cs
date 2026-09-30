using System.ComponentModel.DataAnnotations;

namespace HackerFlow.InputModels.CoverLetter;

public class CreateCoverLetterInput
{
    [Required]
    public string Name { get; set; } = "";

    [Required]
    public IFormFile File { get; set; } = null!;

    public string? Notes { get; set; } = "";
}