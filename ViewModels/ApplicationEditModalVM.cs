using Venture.Models;

namespace Venture.ViewModels;

public class ApplicaitonEditModalVM
{
    public JobApplication JobApplicaiton { get; set; } = new();
    public List<string> Companies { get; set; } = [];
    public List<string> Connections { get; set; } = [];
}