using Microsoft.AspNetCore.Mvc.RazorPages;
using HackerFlow.Models;
using Microsoft.AspNetCore.Mvc;
using HackerFlow.Data;
using Microsoft.EntityFrameworkCore;
using HackerFlow.InputModels.Index;

namespace HackerFlow.Pages.Index;

public class IndexModel : PageModel
{
    private readonly HackerFlowContext _context;

    public IndexModel(HackerFlowContext context)
    {
        _context = context;
    }

    public List<JobApplication> Applications { get; private set; } = [];
    public List<string> ResumeGroups { get; private set; } = [];
    public List<string> CoverLetterGroups { get; private set; } = [];

    private async Task LoadProperties()
    {
        Applications = await _context.Applications.ToListAsync();
        ResumeGroups = await _context.Resumes.GroupBy(x => x.Name).Select(x => x.First().Name).ToListAsync();
        CoverLetterGroups = await _context.CoverLetters.GroupBy(x => x.Name).Select(x => x.First().Name).ToListAsync();
    }

    public async Task OnGetAsync()
    {
        await LoadProperties();
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm(Name = "CreateInput")] CreateApplicationInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();   
            return Page();
        }

        var application = new JobApplication
        {
            Company = input.Company,
            Role = input.Role,
            Url = input.Url,
            Status = input.Status,
            Method = input.Method,
            Contact = input.Contact ?? "",
            Notes = input.Notes ?? ""
        };

        _context.Applications.Add(application);
        _context.SaveChanges();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromForm(Name = "UpdateInput")] UpdateApplicationInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var application = _context.Applications.Find(input.Id);
        if (application == null)
        {
            return NotFound();
        }

        application.Company = input.Company;
        application.Role = input.Role;
        application.Url = input.Url;
        application.Status = input.Status;
        application.Method = input.Method;
        application.DateApplied = input.DateApplied;
        application.FollowUpDate = input.FollowUpDate;
        application.Contact = input.Contact ?? "";
        application.Notes = input.Notes ?? "";
        application.JobDescription = input.JobDescription ?? "";

        _context.SaveChanges();

        return RedirectToPage();
    }

    
}
