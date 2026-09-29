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

    private async Task LoadApplicationsAsync() => 
        Applications = await _context.Applications.ToListAsync();

    public async Task OnGetAsync()
    {
        await LoadApplicationsAsync();
    }

    public async Task<IActionResult> OnPostCreate([FromForm(Name = "CreateInput")] CreateApplicationInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadApplicationsAsync();   
            return Page();
        }

        var application = new JobApplication
        {
            Company = input.Company,
            Role = input.Role,
            Url = input.Url,
            Status = input.Status,
            Method = input.Method,
            Contact = input.Contact,
            Notes = input.Notes
        };

        _context.Applications.Add(application);
        _context.SaveChanges();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdate([FromForm(Name = "UpdateInput")] UpdateApplicationInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadApplicationsAsync();
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
        application.Contact = input.Contact;
        application.Notes = input.Notes;
        application.JobDescription = input.JobDescription;

        _context.SaveChanges();

        return RedirectToPage();
    }

}
