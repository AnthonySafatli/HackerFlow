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

        var newFolder = input.Value;

        Directory.CreateDirectory(newFolder);

        var resumes = await _context.Resumes.ToListAsync();

        foreach (var resume in resumes)
        {
            var newPath = await _file.MoveAsync(
                resume.FilePath,
                newFolder);

            if (newPath == null)
            {
                ModelState.AddModelError(
                    "",
                    $"Could not find resume file: {resume.FilePath}");

                await LoadProperties();
                return Page();
            }

            resume.FilePath = newPath;
        }

        await _context.SaveChangesAsync();
        await _settings.SetSetting(AppSetting.ResumeFolder, newFolder);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGeneratedFolderAsync([FromForm(Name = "ValueInput")] SettingUpdateInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var newFolder = input.Value;

        Directory.CreateDirectory(newFolder);

        var applications = await _context.Applications.Where(x => x.ResumePath != null).ToListAsync();

        foreach (var resume in applications)
        {
            var newPath = await _file.MoveAsync(
                resume.ResumePath,
                newFolder);

            if (newPath == null)
            {
                ModelState.AddModelError(
                    "",
                    $"Could not find resume file: {resume.ResumePath}");

                await LoadProperties();
                return Page();
            }

            resume.ResumePath = newPath;
        }

        await _context.SaveChangesAsync();
        await _settings.SetSetting(AppSetting.GerenatedResumeFolder, newFolder);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm(Name = "CreateInput")] CreateResumeInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var versionCount = await _context.Resumes.CountAsync(r => r.Name == input.Name);
        
        var resumeFolder = await _settings.GetSetting(AppSetting.ResumeFolder);
        var fileName = $"{input.Name}_{versionCount}_{Guid.NewGuid()}.pdf";

        string path = await _file.SaveAsync(resumeFolder, fileName, input.File.OpenReadStream());

        var resume = new Models.Resume
        {
            Name = input.Name,
            Version = versionCount + 1,
            FilePath = path,
            Notes = input.Notes ?? "",
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
        _file.Delete(resume.FilePath);

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostOpenAsync(int id)
    {
        var resume = await _context.Resumes.FindAsync(id);
        if (resume == null || !System.IO.File.Exists(resume.FilePath))
        {
            return NotFound();
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = resume.FilePath,
            UseShellExecute = true
        });

        // stays on the current page
        return new NoContentResult(); 
    }
}