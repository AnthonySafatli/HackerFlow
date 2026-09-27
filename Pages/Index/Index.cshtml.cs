using Microsoft.AspNetCore.Mvc.RazorPages;
using HackerFlow.Models;
using Microsoft.AspNetCore.Mvc;
using HackerFlow.Data;
using Microsoft.EntityFrameworkCore;

namespace HackerFlow.Pages.Index;

public class IndexModel : PageModel
{
    private readonly HackerFlowContext _context;

    public IndexModel(HackerFlowContext context)
    {
        _context = context;
    }

    public List<JobApplication> Applications { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Applications = await _context.Applications.ToListAsync();
    }

    public IActionResult OnPostCreate(JobApplication Application)
    {
        // Database/file-system logic will go here.

        return RedirectToPage();
    }

    public IActionResult OnPostUpdate(JobApplication Application)
    {
        // Database/file-system logic will go here.

        return RedirectToPage();
    }

}
