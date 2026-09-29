using Microsoft.AspNetCore.Mvc.RazorPages;
using HackerFlow.Models;
using HackerFlow.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HackerFlow.InputModels.Resume;

namespace HackerFlow.Pages.Resume;

public class IndexModel : PageModel
{
    private readonly HackerFlowContext _context;

    public IndexModel(HackerFlowContext context)
    {
        _context = context;
    }

    public List<Models.Resume> Resumes { get; private set; } = [];

    private async Task LoadResumesAsync() => 
        Resumes = await _context.Resumes.ToListAsync();

    public async Task OnGetAsync()
    {
        await LoadResumesAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm(Name = "CreateInput")] CreateResumeInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadResumesAsync();
            return Page();
        }
        
        // Save the file to the file system

        var versionCount = await _context.Resumes.CountAsync(r => r.Name == input.Name);

        var resume = new Models.Resume
        {
            Name = input.Name,
            Version = versionCount + 1,
            FilePath = "path/to/file", // TODO: Set the actual file path
            Notes = input.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Resumes.Add(resume);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var resume = await _context.Resumes.FindAsync(id);
        if (resume == null)
        {
            return NotFound();
        }

        _context.Resumes.Remove(resume);

        // TODO: Delete the file from the file system as well

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}