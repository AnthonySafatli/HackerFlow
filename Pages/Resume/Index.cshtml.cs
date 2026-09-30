using Microsoft.AspNetCore.Mvc.RazorPages;
using HackerFlow.Models;
using HackerFlow.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HackerFlow.InputModels.Resume;
using HackerFlow.Services;
using HackerFlow.InputModels;

namespace HackerFlow.Pages.Resume;

public class IndexModel : PageModel
{
    private readonly HackerFlowContext _context;
    private readonly IAppSettingsService _settings;
    private readonly IFileService _file;

    public IndexModel(HackerFlowContext context, IAppSettingsService settings, IFileService file)
    {
        _context = context;
        _settings = settings;
        _file = file;
    }

    public string ResumeFolder { get; set; } = "";
    public string GeneratedResumeFolder { get; set; } = "";
    public List<Models.Resume> Resumes { get; private set; } = [];

    public IEnumerable<IGrouping<string, Models.Resume>> ResumeGroups =>
        Resumes
            .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(r => r.Version)
            .GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase);

    private async Task LoadProperties()
    {
        Resumes = await _context.Resumes.ToListAsync();
        ResumeFolder = await _settings.GetSetting(AppSetting.ResumeFolder);
        GeneratedResumeFolder = await _settings.GetSetting(AppSetting.GerenatedResumeFolder);
    }

    public async Task OnGetAsync()
    {
        await LoadProperties();
    }

    public async Task<IActionResult> OnPostResumeFolderAsync([FromForm(Name = "ValueInput")] SettingUpdateInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        await _settings.SetSetting(AppSetting.ResumeFolder, input.Value);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGeneratedFolderAsync([FromForm(Name = "ValueInput")] SettingUpdateInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }
        
        await _settings.SetSetting(AppSetting.GerenatedResumeFolder, input.Value);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm(Name = "CreateInput")] CreateResumeInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }
        
        var resumeFolder = await _settings.GetSetting(AppSetting.ResumeFolder);
        var fileName = input.Name + Guid.NewGuid().ToString() + ".pdf";

        string path = await _file.SaveAsync(resumeFolder, input.Name, input.File.OpenReadStream());

        var versionCount = await _context.Resumes.CountAsync(r => r.Name == input.Name);

        var resume = new Models.Resume
        {
            Name = input.Name,
            Version = versionCount + 1,
            FilePath = path,
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
        _file.Delete("", resume.FilePath);

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}