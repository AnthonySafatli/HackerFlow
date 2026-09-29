using System.ComponentModel.DataAnnotations;
using HackerFlow.Models;

public class UpdateApplicationInput
{
    [Required]
    public int Id { get; set; }
    
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

    public DateTime? DateApplied { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public string Contact { get; set; } = "";

    public string Notes { get; set; } = "";

    public string JobDescription { get; set; } = "";
}