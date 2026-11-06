using System.ComponentModel.DataAnnotations;
using Venture.Models;

namespace Venture.InputModels.Networking;

public class UpdateCompanyInput
{
    public int Id { get; set; }
 
    [Required]
    public string Name { get; set; } = "";
 
    public string? Website { get; set; }
 
    public ReferralChance ReferralChance { get; set; }
 
    public string? Description { get; set; }
}
