using Microsoft.AspNetCore.Mvc.RazorPages;
using Venture.Models;
using Venture.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Venture.InputModels.Resume;
using Venture.Services;
using Venture.InputModels;
using Venture.Utils;

namespace Venture.Pages.Resume;

public class IndexModel : PageModel
{
    private readonly VentureContext _context;
    private readonly IAppSettingsService _settings;
    private readonly IFileService _file;
    private readonly IOpenFileService _openFile;

    public IndexModel(VentureContext context, IAppSettingsService settings, IFileService file, IOpenFileService openFile)
    {
        _context = context;
        _settings = settings;
        _file = file;
        _openFile = openFile;
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
        GeneratedResumeFolder = await _settings.GetSetting(AppSetting.GeneratedResumeFolder);
    }

    public async Task OnGetAsync()
    {
        await LoadProperties();
    }

    private bool TryPrepareFolder(string? raw, out string folder)
    {
        const string key = "ValueInput.Value";

        if (!PathUtils.TryNormalize(raw, out folder, out var error))
        {
            ModelState.AddModelError(key, error!);
            return false;
        }

        try
        {
            Directory.CreateDirectory(folder);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ModelState.AddModelError(key, "Can't create or access that folder.");
            return false;
        }

        return true;
    }

    public async Task<IActionResult> OnPostResumeFolderAsync(
        [FromForm(Name = "ValueInput")] SettingUpdateInput input)
    {
        if (!ModelState.IsValid || !TryPrepareFolder(input?.Value, out var newFolder))
        {
            await LoadProperties();
            return Page();
        }

        var resumes = await _context.Resumes.ToListAsync();

        // Check everything exists before moving anything
        var missing = resumes.FirstOrDefault(r => !System.IO.File.Exists(r.FilePath));
        if (missing != null)
        {
            ModelState.AddModelError("", $"Could not find resume file: {missing.FilePath}");
            await LoadProperties();
            return Page();
        }

        foreach (var resume in resumes)
        {
            var newPath = await _file.MoveAsync(resume.FilePath, newFolder);

            if (newPath == null)
            {
                // Keep the DB in sync with files already moved
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", $"Could not move resume file: {resume.FilePath}");
                await LoadProperties();
                return Page();
            }

            resume.FilePath = newPath;
        }

        await _context.SaveChangesAsync();
        await _settings.SetSetting(AppSetting.ResumeFolder, newFolder);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGeneratedFolderAsync(
        [FromForm(Name = "ValueInput")] SettingUpdateInput input)
    {
        if (!ModelState.IsValid || !TryPrepareFolder(input?.Value, out var newFolder))
        {
            await LoadProperties();
            return Page();
        }

        var applications = await _context.Applications
            .Where(x => !string.IsNullOrWhiteSpace(x.ResumePath))
            .ToListAsync();

        var missing = applications.FirstOrDefault(a => !System.IO.File.Exists(a.ResumePath));
        if (missing != null)
        {
            ModelState.AddModelError("", $"Could not find generated file: {missing.ResumePath}");
            await LoadProperties();
            return Page();
        }

        foreach (var application in applications)
        {
            var newPath = await _file.MoveAsync(application.ResumePath, newFolder);

            if (newPath == null)
            {
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", $"Could not move generated file: {application.ResumePath}");
                await LoadProperties();
                return Page();
            }

            application.ResumePath = newPath;
        }

        await _context.SaveChangesAsync();
        await _settings.SetSetting(AppSetting.GeneratedResumeFolder, newFolder);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm(Name = "CreateInput")] CreateResumeInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var version = (await _context.Resumes
            .Where(r => r.Name == input.Name)
            .MaxAsync(r => (int?)r.Version) ?? 0) + 1;
        
        var ext = Path.GetExtension(input.File.FileName).ToLowerInvariant();
        var resumeFolder = await _settings.GetSetting(AppSetting.ResumeFolder);
        var fileName = $"{input.Name}_{version}_{Guid.NewGuid()}{ext}";

        string path = await _file.SaveAsync(resumeFolder, fileName, input.File.OpenReadStream());

        var resume = new Models.Resume
        {
            Name = input.Name,
            Version = version,
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
            ModelState.AddModelError("", "Resume not found.");
            await LoadProperties();
            return Page();
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
            ModelState.AddModelError("", "Resume not found.");
            await LoadProperties();
            return Page();
        }

        _openFile.OpenFile(resume.FilePath);

        // stays on the current page
        return new NoContentResult(); 
    }
}