using System.ComponentModel.DataAnnotations;
using HackerFlow.Models;

namespace HackerFlow.InputModels.Prompt;

public class CreatePromptInput
{
    [Required] 
    public string Name { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";
    
    [Required]
    public string PromptString { get; set; } = "";
}
