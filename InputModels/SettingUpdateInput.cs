using System.ComponentModel.DataAnnotations;

namespace Venture.InputModels;

public class SettingUpdateInput
{
    [Required]
    public string Value { get; set; } = "";
}