using System.ComponentModel.DataAnnotations;

namespace HackerFlow.InputModels;

public class SettingUpdateInput
{
    [Required]
    public string Value { get; set; } = "";
}