using System.ComponentModel.DataAnnotations;
using HackerFlow.Models;

namespace HackerFlow.InputModels.Index;

public class CreatePromptInput
{
    [Required] 
    public string Company { get; set; } = "";

    [Required]
    public string Role { get; set; } = "";
    
    [Required]
    public string Url { get; set; } = "";
    
    [Required]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Bookmarked;
    
    [Required]
    public string Method { get; set; } = "";
    
    public string? Contact { get; set; } = "";
    
    public string? Notes { get; set; } = "";
}
